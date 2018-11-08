using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostCity
{
    class LostCityObjects
    {
        List<Polyline> rios = new List<Polyline>();
        List<Polyline> caminos = new List<Polyline>();
        List<Sitio> sitios = new List<Sitio>();
        DEM elevationModel;
        List<int[]> indicesForAnalysis;
        public LostCityObjects()
        {
            elevationModel = new DEM(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\C18\C18\", 11.140131, 10.975654, -74.000914, -73.836385);
            readObjects();
            Test();
        }
        private void readObjects()
        {
            rios = MapTools.readPolylines(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\georefObjects\rios.csv", false);
            caminos = MapTools.readPolylines(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\georefObjects\caminos.csv", false);
            getSitioData();
        }
        private void Test()
        {
            getIndices();
            RefPlaneVis rpv = new RefPlaneVis(elevationModel.hts, 90);
            rpv.pointList(indicesForAnalysis);
            rpv.writeVis("topThreeAreaViewShed");
        }
        private void getIndices()
        {
            indicesForAnalysis = new List<int[]>();
            
            for(int i = 0;i<elevationModel.ptsLonLat.Count;i++)
            {
                for(int j=0;j< elevationModel.ptsLonLat[i].Count; j++)
                {
                    for (int s = 0; s < 3; s++)
                    {
                        if (MapTools.isPointInPolygon(elevationModel.ptsLonLat[i][j], sitios[s].boundary.vertices))
                        {
                            int[] index = { i, j };
                            indicesForAnalysis.Add(index);
                            break;
                        }
                    }
                }
            }
            
        }
        private void getSitioData()
        {
            List<Polyline> boundaries = MapTools.readPolylines(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\georefObjects\sitios.csv", true);
            StreamReader sr = new StreamReader(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\georefObjects\sitioData.csv");
            string line = sr.ReadLine();
            while(line!= null)
            {
                string[] parts = line.Split(',');
                Sitio s = new Sitio();
                s.name = parts[0];
                s.area = Convert.ToDouble(parts[1]);
                s.populationL = Convert.ToDouble(parts[2]);
                s.populationF = Convert.ToDouble(parts[3]);
                s.boundary = boundaries.Find(x => x.name == s.name);
                sitios.Add(s);
                line = sr.ReadLine();
            }
            sr.Close();
        }
    }
}
