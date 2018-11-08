using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostCity
{
    class MapTools
    {
        public static List<Polyline> readPolylines(string filepath,bool withnames)
        {
            List<Polyline> polylines = new List<Polyline>();
            StreamReader sr = new StreamReader(filepath);
            string line = sr.ReadLine();
            while (line!=null)
            {
                string[] parts = line.Split(',');
                Polyline pl = new Polyline();
                if (withnames)
                {
                    pl.name = parts[0];
                    var p = parts.ToList();
                    p.RemoveAt(0);
                    parts = p.ToArray();
                }
                double[] coord = new double[2];
                for (int i=0;i<parts.Length;i++)
                {

                    if (i % 2 == 0)
                    {
                        coord[0] = Convert.ToDouble(parts[i]);
                    }
                    else
                    {
                        coord[1] = Convert.ToDouble(parts[i]);
                        pl.vertices.Add(coord);
                        coord = new double[2];
                    }
                }
                polylines.Add(pl);
                line = sr.ReadLine();
            }
            sr.Close();
            return polylines;
        }
        public static bool isPointInPolygon(double[] point, List<double[]> vs)
        {
            // ray-casting algorithm based on
            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html

            double x = point[0], y = point[1];

            bool inside = false;
            for (int i = 0, j = vs.Count - 1; i < vs.Count; j = i++)
            {
                double xi = vs[i][0], yi = vs[i][1];
                double xj = vs[j][0], yj = vs[j][1];

                bool intersect = ((yi > y) != (yj > y))
                    && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                if (intersect) inside = !inside;
            }

            return inside;
        }
    }
    public class Sitio
    {
        public string name { get; set; }
        public Polyline boundary { get; set; }
        public double populationL { get; set; }
        public double populationF { get; set; }
        public double area { get; set; }
    }
    public class Polyline
    {
        public List<double[]> vertices { get; set; }
        public string name { get; set; }
        public Polyline()
        {
            vertices = new List<double[]>();
        }

    }
}
