using Neo4jClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostCity
{
    class NeoGraph
    {
    }
    public class VisNet
    {
        private readonly IGraphClient _graphClient;
        public VisNet()
        {
            _graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "12345");
            _graphClient.Connect();
            //clearTheGraph();
        }
        public void analysis()
        {
            var allNodes = getAllNeoNodes();
            closenessCentrality();
            pageRank();
            shortestPath(0, 434);
            allPairsShortestPath();
            
        }
        private void clearTheGraph()
        {
            var allNodes = getAllNodes();
                foreach(SitePoint n in allNodes)
                {
                _graphClient.Cypher
                    .Match("(sp1:SitePoint)")
                    .Where((SitePoint sp1) => sp1.Id == n.Id)
                    .DetachDelete("sp1")
                    .ExecuteWithoutResults();
            }
        }
        public void addTestNeo(List<Sitio> sites)
        {
            long id = 0;
            foreach(Sitio s in sites)
            {
                addSiteUnique(s.name, id);
                id++;
            }
            linkRandom();
            connectionTests();
            
        }
        private void linkRandom()
        {
            Random r = new Random();
            var all = getAllNodes();
            for(int i = 0; i < 200; i++)
            {
                SitePoint s = all[r.Next(0, all.Count)];
                SitePoint e = all[r.Next(0, all.Count)];
                addVisLine(s.Id, e.Id,0);
            }
        }
        public void addSite(string name,long id)
        {
            //this creates new objects even if they exist
            var newsite = new SitePoint { Id = id, Name = name };
            _graphClient.Cypher
                .Create("(sp:SitePoint {newsite})")
                .WithParam("newsite", newsite)
                .ExecuteWithoutResults();
        }
        public void addSiteUnique(string name, long id)
        {
            var newsite = new SitePoint { Id = id, Name = name };
            _graphClient.Cypher
                .Merge("(sp:SitePoint { Id:{id} })")
                .OnCreate()
                .Set("sp={newsite}")
                .WithParams(new
                {
                    id = newsite.Id,
                    newsite
                })
                .ExecuteWithoutResults();
           
        }
        public void addVisLine(long startId,long endId,int weight)
        {
            _graphClient.Cypher
            .Match("(sp1:SitePoint)", "(sp2:SitePoint)")
            .Where((SitePoint sp1) => sp1.Id == startId)
            .AndWhere((SitePoint sp2) => sp2.Id == endId)
            .Create("(sp1)-[:SEES {weight:" + weight + "}]->(sp2)")
            .ExecuteWithoutResults();
        }

        public void pageRank()
        {
            var pageR = _graphClient.Cypher
                .Call("algo.pageRank.stream('SitePoint', 'SEES', { iterations: 20, dampingFactor: 0.85})")
                .Yield("nodeId,score")
                .Return((nodeId, score) => new {
                    NodeId = nodeId.As<Int32>(),
                    Score = score.As<Double>()
                })
                 .Results;
            var list = pageR.ToList();
            foreach(var anon in list)
            {
                int n = anon.NodeId;
                double d = anon.Score;
            }
        }
        public void allPairsShortestPath()
        {
            //shortest path optimised for all node pairs
            var paths = _graphClient.Cypher
                .Call("algo.allShortestPaths.stream('weight',{ nodeQuery: 'SitePoint',defaultValue: 1.0})")
                .Yield("sourceNodeId, targetNodeId, distance")
                .Where("algo.isFinite(distance) = true")
                .Match("(source:SitePoint)", "(target:SitePoint)")
                .Where("ID(source) = sourceNodeId")
                .AndWhere("ID(target) = targetNodeId")
                .Return((source, target, distance) => new
                {
                    Source = source.As<SitePoint>(),
                    Target = target.As<SitePoint>(),
                    Dist = distance.As<Double>()
                })
                .Results;
            Type t2 = paths.GetType();
        }
        public void shortestPath(long startId,long endId)
        {
            var path = _graphClient.Cypher
            .Match("(start:SitePoint)", "(end:SitePoint)")
            .Where((SitePoint start) => start.Id == startId)
            .AndWhere((SitePoint end) => end.Id == endId)
            .Call("algo.shortestPath.stream(start, end, 'weight')")
            .Yield("nodeId,cost")
            .Return((nodeId, cost) => new {
                Int32 = nodeId.As<Int32>(),
                Double = cost.As<Double>()
            })
            .Results;
        }
        public void closenessCentrality()
        {
            //return the results
            //var clcsCent = _graphClient.Cypher.Call("algo.closeness.stream('SitePoint', 'SEES', {write:true})")
            //     .Yield("nodeId,centrality")
            //     .Return((nodeId,centrality)=>new {
            //         Int32 = nodeId.As<Int32>(),
            //         Double = centrality.As<Double>()
            //     })
            //     .Results;
            //write centrality to nodes 
            var r =_graphClient.Cypher.Call("algo.closeness('SitePoint', 'SEES',  {write:true, writeProperty:'centrality'})")
                .Yield("nodes")
                .Return(nodes => nodes.As<Int32>() )
                .Results;
        }
        private void connectionTests()
        {
            var res = _graphClient.Cypher
            .OptionalMatch("(sp:SitePoint)-[SEES]-(friend:SitePoint)")
            .Where((SitePoint sp) => sp.Id == 0)
            .Return((sp, friend) => new
            {
                SitePoint = sp.As<SitePoint>(),
                Friends = friend.CollectAs<SitePoint>()
            })
            .Results;
        }
        private List<SitePoint> getAllNodes()
        {
            var all = _graphClient.Cypher
            .Match("(sp:SitePoint)")
            .Return(sp => sp.As<SitePoint>())
            .Results;
            return all.ToList();
        }
        private IEnumerable<Node<SitePoint>> getAllNeoNodes()
        {
            var all = _graphClient.Cypher
            .Match("(sp:SitePoint)")
            .Return(sp => sp.As<Node<SitePoint>>())
            .Results;
            return all.ToList();
        }
        public void cypherTests()
        {
            var all = _graphClient.Cypher
            .Match("(sp:SitePoint)")
            .Return(sp => sp.As<SitePoint>())
            .Results;
            var matchId = _graphClient.Cypher
            .Match("(sp:SitePoint)")
            .Where((SitePoint sp) => sp.Name == "Koskunguena")
            .Return(sp => sp.As<SitePoint>())
            .Results;
        }
        
    }
    public class SitePoint
    {
        //This is required to make the serializer treat the 'C#' naming style as 'Java' in the DB
        //[JsonProperty("name")]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
