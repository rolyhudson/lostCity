using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LostCity
{
    class KMLwriter
    {
        private void header(StreamWriter sw)
        {
            sw.WriteLine("<? xml version = \"1.0\" encoding = \"UTF - 8\" ?>");
            sw.WriteLine("< kml xmlns = \"http://www.opengis.net/kml/2.2\" >");
            sw.WriteLine("< Document >");
        }
        private void fileend(StreamWriter sw)
        {
            sw.WriteLine("</ Document >");
            sw.WriteLine("</ kml >");
        }
        private void defineStyle(StreamWriter sw,string lineColor,int lineWidth,string polyColor,string id)
        {
            sw.WriteLine("< Style id = \""+ id+"\" >");
            sw.WriteLine("< LineStyle >");
            sw.WriteLine("< color > " + lineColor + "</ color >");
            sw.WriteLine("< width > " + lineWidth + "</ width >");
            sw.WriteLine("</ LineStyle >");
            sw.WriteLine("< PolyStyle >");
            sw.WriteLine("< color > " + polyColor + " </ color >");
            sw.WriteLine("</ PolyStyle >");
            sw.WriteLine("</ Style >");
        }
        private void addLineString(StreamWriter sw,double[][] coords,int extrude,int tessllate)
        {
            sw.WriteLine("< LineString >");
            sw.WriteLine("< extrude > 1 </ extrude >");
            sw.WriteLine("< tessellate > 1 </ tessellate >");
            sw.WriteLine("< altitudeMode > absolute </ altitudeMode >");
            sw.Write(" < coordinates >");
            foreach(double[] c in coords)
            {
                sw.WriteLine(c[0].ToString() + "," + c[1].ToString() + "," + c[2].ToString());
            }
            
            sw.WriteLine(" </ coordinates > ");
            sw.WriteLine(" </ LineString >");
        }
        private void testkml(string filename)
        {
            double[][] coords = new double[][] {
                new double[]{-112.2550785337791,36.07954952145647,2357},
          new double[]{-112.2549277039738,36.08117083492122,2357},
          new double[]{-112.2552505069063,36.08260761307279,2357},
         new double[]{ -112.2564540158376,36.08395660588506,2357},
         new double[] {-112.2580238976449,36.08511401044813,2357},
         new double[] {-112.2595218489022,36.08584355239394,2357},
         new double[] {-112.2608216347552,36.08612634548589,2357},
          new double[]{-112.262073428656,36.08626019085147,2357},
         new double[] {-112.2633204928495,36.08621519860091,2357},
         new double[] {-112.2644963846444,36.08627897945274,2357},
         new double[] {-112.2656969554589,36.08649599090644,2357}};

            StreamWriter sw = new StreamWriter(filename);
            header(sw);
            sw.WriteLine("<name> Paths </name>");
            sw.WriteLine("<description> </description>");
            defineStyle(sw, "7dff0000", 2, "7dff0000", "#transBluePoly");
            sw.WriteLine("<Placemark>");
            sw.WriteLine("<name>Absolute Extruded</name>");
            sw.WriteLine("<description> testing line strings</ description >");
            sw.WriteLine("<styleUrl>#transBluePoly</styleUrl>");
            addLineString(sw, coords, 1, 1);
            sw.WriteLine("</Placemark>");

            fileend(sw);
            sw.Close();
        }
    }
    
}
