using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostCity
{
    class DrainageNet
    {
        //REFS:
        //http://pro.arcgis.com/en/pro-app/tool-reference/spatial-analyst/how-flow-accumulation-works.htm
        //https://gis.stackexchange.com/questions/113634/how-to-extract-drainage-line-network-from-grass-gis-and-qgis/113645
        DEM dem;
        public List<List<Vector3d>> d8Flow = new List<List<Vector3d>>();
        public List<List<int>> d8Index = new List<List<int>>();
        public DrainageNet()
        {
            dem = new DEM(@"C:\Users\Admin\Documents\projects\LostCity\C18\C18\", 11.140131, 10.975654, -74.000914, -73.836385);
        }
        public void flowDirection()
        {
            for(int i = 0; i < dem.demPts.Count; i++)//rows
            {
                List<Vector3d> row = new List<Vector3d>();
                List<int> rowindex = new List<int>();
                for (int j=0;j<dem.demPts[i].Count;j++)//cols
                {
                    //check all points around and make a vector to the lowest
                    //ignore the boundary
                    List<Point3d> neighbours = new List<Point3d>();
                    double minZ = -1;
                    if (i>0&&j>0&&i< dem.demPts.Count-1&&j< dem.demPts[i].Count-1)
                    {
                        //order is anticlockwise from CAD zero angle
                        neighbours.Add(dem.demPts[i][j + 1]);

                        neighbours.Add(dem.demPts[i - 1][j + 1]);
                        neighbours.Add(dem.demPts[i - 1][j]);
                        neighbours.Add(dem.demPts[i - 1][j - 1]);
                        
                        neighbours.Add(dem.demPts[i ][j - 1]);
                        

                        neighbours.Add(dem.demPts[i + 1][j - 1]);
                        neighbours.Add(dem.demPts[i + 1][j]);
                        neighbours.Add(dem.demPts[i + 1][j + 1]);

                        minZ = neighbours[0].Z;
                    }
                    Vector3d slope = new Vector3d(Vector3d.ZAxis);
                    
                    int index = -1;
                    for(int p =0;p<neighbours.Count;p++)
                    {
                        if (neighbours[p].Z < minZ)
                        {
                            slope = neighbours[p] - dem.demPts[i][j];
                            minZ = neighbours[p].Z;
                            index = p;
                        }
                    }
                    rowindex.Add(index);
                    row.Add(slope);
                }
                this.d8Index.Add(rowindex);
                this.d8Flow.Add(row);
            }
        }
        public void printFlowIndices()
        {
            StreamWriter sw = new StreamWriter(@"C:\Users\Admin\Documents\projects\LostCity\flowResults\flowindex.txt");
            for(int i = 0; i < this.d8Index.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                for(int j=0;j< this.d8Index[i].Count; j++)
                {
                    if(j== this.d8Index[i].Count-1) sb.Append(this.d8Index[i][j]);
                    else sb.Append(this.d8Index[i][j]+",");
                }
                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
        public void flowAccumulation()
        {

        }
        public void streamVectors()
        {

        }
    }
}
