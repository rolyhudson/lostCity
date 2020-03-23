using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace LostCity
{
    class Graph
    {
        public List<Node> nodes = new List<Node>();
        public List<Edge> edges = new List<Edge>();
        public List<EdgeBundle> bundles = new List<EdgeBundle>();
        public VisNet visNet = new VisNet();
        public void addEdgeByIndices(int[] start,int[] end)
        {
            Edge edg = new Edge();
            int ns = nodes.Find(x => x.demIndices.SequenceEqual(start)).id;
            int ne = nodes.Find(x => x.demIndices.SequenceEqual(end)).id;
            edg.start = ns;
            edg.end = ne;
            edges.Add(edg);
        }
        public void reduceGraph()
        {
            List<Node> usedNodes = new List<Node>();
            for(int i=0;i<edges.Count;i++)
            {
                Edge e = edges[i];
                
                string startname = nodes[e.start].name ;
                string endname = nodes[e.end].name;
                var snodes = nodes.FindAll(n => n.name == startname).Select(c=>c.id).ToList();
                var enodes = nodes.FindAll(n => n.name == endname).Select(c => c.id).ToList(); 
                var matched = edges.FindAll(x => snodes.Contains(x.start) && enodes.Contains(x.end));
                //renumber edge node start and end
                e.start = snodes[0];
                e.end = enodes[0];
                e.weight = matched.Count;
                //remove 
                int removed = edges.RemoveAll(x => snodes.Contains(x.start) && enodes.Contains(x.end) && x.weight==0);
                usedNodes.Add(nodes[e.start]);
                usedNodes.Add(nodes[e.end]);
            }
            var usedNodesD = usedNodes.Distinct().ToList();
            nodes = usedNodesD;
            //summariseNodeDegree();
            //outputNodeEdgeList();
            createNeoGraph();
        }
        private void summariseNodeDegree()
        {
            foreach(Node n in nodes)
            {
                int inW = 0;
                int outW = 0;
                foreach(Edge e in edges)
                {
                    if (e.start == n.id) outW += e.weight;
                    if (e.end == n.id) inW += e.weight;
                }
                n.inWeight = inW;
                n.outWeight = outW;
            }
            custeringCoefficientPlot();
            degreeDistributionGraph();
        }
        private int edgesInNeighbourHood(Node n)
        {
            int nEdges = 0;
            List<int> neighbourhood = new List<int>();
            //get the nodes in the hood
            foreach(Edge e in edges)
            {
                //if the start is our node the end is in the neighbourhood
                if (e.start == n.id) neighbourhood.Add(e.end);
                //if the end is our node the start is in the neighbourhood
                if (e.end == n.id) neighbourhood.Add(e.start);
            }
            //find edges in between hood nodes
            foreach (Edge e in edges)
            {
                if (neighbourhood.Contains(e.start) && neighbourhood.Contains(e.end)) nEdges += e.weight;
            }
            return nEdges;
        }
        private void custeringCoefficientPlot()
        {
            foreach (Node n in nodes)
            {
                n.totalWeight = n.inWeight + n.outWeight;
                n.clusCoeff = (double)edgesInNeighbourHood(n) / (n.totalWeight * (n.totalWeight - 1));
            }
            var sorted = nodes.OrderBy(x => x.clusCoeff);
            
            var d = sorted.Select(x => x.clusCoeff).ToList();
            var lbls = sorted.Select(x => x.name).ToList();

            ZedGraphControl zg = new ZedGraphControl();
            MasterPane master = setupPlotMaster(zg, "clustering coefficients");
            addPaneToMaster(master, d, lbls, "");
            savePlot(master, @"C:\Users\Admin\Documents\projects\LostCity\viewNet\clusterCoefficients.jpeg");
        }
        private MasterPane setupPlotMaster(ZedGraphControl zg,string title)
        {
            
            MasterPane master = zg.MasterPane;
            master.Rect = new RectangleF(0, 0, 600, 800);
            master.PaneList.Clear();
            master.Title.IsVisible = true;
            master.Title.Text = title;
            master.Margin.All = 10;
            master.Legend.IsVisible = false;
            return master;
        }
        private void savePlot(MasterPane master,string file)
        {
            // Layout the GraphPanes using a default Pane Layout
            Bitmap b = new Bitmap(600, 800);
            using (Graphics g = Graphics.FromImage(b))
            {
                master.SetLayout(g, PaneLayout.SingleColumn);
            }
            master.GetImage().Save(file, System.Drawing.Imaging.ImageFormat.Jpeg);

        }
        private void degreeDistributionGraph()
        {
            ZedGraphControl zg = new ZedGraphControl();
            MasterPane master = setupPlotMaster(zg, "degree distribution graphs");
            
            var sorted = nodes.OrderBy(x => x.inWeight);
            var d = sorted.Select(x => Convert.ToDouble(x.inWeight)).ToList();
            var lbls = sorted.Select(x => x.name).ToList();
            addPaneToMaster(master, d, lbls, "in-coming sight lines");

            sorted = nodes.OrderBy(x => x.outWeight);
            d = sorted.Select(x => Convert.ToDouble(x.outWeight)).ToList();
            lbls = sorted.Select(x => x.name).ToList();
            addPaneToMaster(master, d, lbls, "out-going sight lines");

            sorted = nodes.OrderBy(x => x.totalWeight);
            d = sorted.Select(x => Convert.ToDouble(x.totalWeight)).ToList();
            lbls = sorted.Select(x => x.name).ToList();
            addPaneToMaster(master, d, lbls, "total sight lines");
            // Refigure the axis ranges for the GraphPanes
            zg.AxisChange();

            savePlot(master, @"C:\Users\Admin\Documents\projects\LostCity\viewNet\inDegreeDist.jpeg");
        }
        
        private void addPaneToMaster(MasterPane master, List<double> degree, List<string> labels, string titlePrefix)
        {
            GraphPane myPane = new GraphPane();
            myPane.Title.Text = titlePrefix;
            myPane.XAxis.Title.Text = "site";
            myPane.YAxis.Title.Text = titlePrefix + "Visible neighbourhood";
            myPane.XAxis.Scale.Max = 27;
            myPane.Border.IsVisible = false;
            LineItem myLine = myPane.AddCurve("", null, degree.ToArray(), Color.Red, SymbolType.Circle);
            myLine.Symbol.Fill.IsVisible = true;
            myLine.Symbol.Fill = new Fill(Color.Red);
            for (int s = 0;s<myLine.Points.Count;s++)
            {
                ZedGraph.PointPair pt = myLine.Points[s];
                ZedGraph.TextObj text = new ZedGraph.TextObj(labels[s], pt.X, pt.Y,
                        ZedGraph.CoordType.AxisXYScale, ZedGraph.AlignH.Left, ZedGraph.AlignV.Center);
                text.FontSpec.FontColor = Color.Black;
                text.ZOrder = ZedGraph.ZOrder.A_InFront;
                // Hide the border and the fill
                text.FontSpec.Border.IsVisible = false;
                text.FontSpec.Fill.IsVisible = false;
                text.FontSpec.Size = 15f;
                text.FontSpec.Angle = 90;

                string lblString = "name";

                Link lblLink = new Link(lblString, "#", "");
                text.Link = lblLink;
                myPane.GraphObjList.Add(text);
            }
            
            myLine.Line.IsVisible = false;
            myLine.Symbol.Size = 5f;
            master.Add(myPane);
        }
        private void createNeoGraph()
        {
            foreach(Node n in nodes)
            {
                visNet.addSiteUnique(n.name, n.id);
            }
            foreach(Edge e in edges)
            {
                //visNet.addSiteLine(nodes.Find(x=>x.id==e.start).name, nodes.Find(x => x.id == e.end).name,e.weight);
                visNet.addVisLine(nodes.Find(x => x.id == e.start).id, nodes.Find(x => x.id == e.end).id, e.weight);
            }
            
            visNet.closenessCentrality();
        }
        public void outputNodeEdgeList()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(@"C:\Users\Admin\Documents\projects\LostCity\viewNet\nodesEdges.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {

                writer.WriteStartObject();
                writer.WritePropertyName("nodes");
                //writer.WriteValue(JsonConvert.SerializeObject(nodes));
                writer.WriteStartArray();
                foreach(Node n in nodes)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");
                    writer.WriteValue(n.id.ToString());
                    writer.WritePropertyName("name");
                    writer.WriteValue(n.name);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WritePropertyName("links");
                writer.WriteStartArray();
                foreach(Edge e in edges)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("source");
                    writer.WriteValue(e.start.ToString());
                    writer.WritePropertyName("target");
                    writer.WriteValue(e.end.ToString());
                    writer.WritePropertyName("weight");
                    writer.WriteValue(e.weight);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                
            }
        }
        public void bundlePoint()
        {
            foreach (Edge e in edges)
            {
                string startname = "s."+nodes[e.start].name + "_p_" + nodes[e.start].id;
                string endname = "s." + nodes[e.end].name + "_p_" + nodes[e.end].id;
                if (!bundles.Exists(x => x.name == startname))
                {
                    //create a new one
                    EdgeBundle eb = new EdgeBundle();
                    eb.name = startname;
                    eb.imports.Add(endname);
                    bundles.Add(eb);
                }
                else
                {
                    EdgeBundle eb = bundles.Find(x => x.name == startname);
                    eb.imports.Add(endname);
                }
            }
            outputBundles(@"C:\Users\Admin\Documents\projects\LostCity\viewNet\pointNet.json");
        }
        public void bundleSitio()
        {
            foreach(Edge e in edges)
            {
                if (!bundles.Exists(x => x.name == nodes[e.start].name))
                {
                    //create a new one
                    EdgeBundle eb = new EdgeBundle();
                    eb.name = nodes[e.start].name;
                    eb.imports.Add(nodes[e.end].name);
                    bundles.Add(eb);
                }
                else
                {
                    EdgeBundle eb = bundles.Find(x => x.name == nodes[e.start].name);
                    eb.imports.Add(nodes[e.end].name);
                }
            }
            outputBundles(@"C:\Users\Admin\Documents\projects\LostCity\viewNet\sitioNet.json");
        }
        private void outputBundles(string file)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;


            using (StreamWriter sw = new StreamWriter(file))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, bundles);
                
            }
        }
    }
    class Node
    {
        public int id { get; set; }
        public string name { get; set; }
        public int[] demIndices { get; set; }
        public int inWeight { get; set; }
        public int outWeight { get; set; }
        public int totalWeight { get; set; }
        public double clusCoeff { get; set; }
    }
    class Edge
    {
        public int start { get; set; }
        public int end { get; set; }
        public double length { get; set; }
        public int weight { get; set; }
    }
    class EdgeBundle
    {
        public string name { get; set; }
        public List<string> imports { get; set; }
        public EdgeBundle()
        {
            imports = new List<string>();
        }
    }

}
