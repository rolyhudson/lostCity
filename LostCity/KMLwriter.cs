﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LostCity
{
    class KMLwriter
    {
        public StreamWriter sw;
        public void header()
        {
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
            sw.WriteLine("<Document>");
        }
        public void openFolder(string foldername)
        {
            sw.WriteLine("<Folder>");
            sw.WriteLine("<name>"+foldername+"</name>");
        }
        public void closeFolder()
        {
            sw.WriteLine("</Folder>");
        }
        public void fileend()
        {
            sw.WriteLine("</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
        }
        public void defineStyle(string lineColor,int lineWidth,string polyColor,string id)
        {
            sw.WriteLine("<Style id=\""+id+"\">");
            sw.WriteLine("<LineStyle>");
            sw.WriteLine("<color>"+lineColor+"</color>");
            sw.WriteLine("<width>"+lineWidth+"</width>");
            sw.WriteLine("</LineStyle>");
            sw.WriteLine("<PolyStyle>");
            sw.WriteLine("<color>"+polyColor+"</color>");
            sw.WriteLine("</PolyStyle>");
            sw.WriteLine("</Style>");
        }
        public void addLineString(double[][] coords,int extrude,int tessllate)
        {
            sw.WriteLine("<LineString>");
            //sw.WriteLine("<extrude>1</extrude>");
            sw.WriteLine("<tessellate>1</tessellate>");
            //sw.WriteLine("<altitudeMode>clampToGround</altitudeMode>");
            sw.Write("<coordinates>");
            foreach(double[] c in coords)
            {
                sw.WriteLine(c[0].ToString() + "," + c[1].ToString() + "," + c[2].ToString());
            }
            
            sw.WriteLine("</coordinates>");
            sw.WriteLine("</LineString>");
        }
        public void addPlacemarkLineString(string name,string description, double[][] coords,string style)
        {
            sw.WriteLine("<Placemark>");
            sw.WriteLine("<name>"+name+"</name>");
            sw.WriteLine("<description>" + description + "</description>");
            sw.WriteLine("<styleUrl>"+style+"</styleUrl>");
            sw.WriteLine("<altitudeMode>absolute</altitudeMode>");
            addLineString(coords, 1, 1);
            sw.WriteLine("</Placemark>");
        }
        public void addPlacemarkPolygon(string name, string description, double[][] coords, string style)
        {
            sw.WriteLine("<Placemark>");
            sw.WriteLine("<name>" + name + "</name>");
            sw.WriteLine("<description>" + description + "</description>");
            sw.WriteLine("<styleUrl>" + style + "</styleUrl>");
            sw.WriteLine("<Polygon>");
            sw.WriteLine("<tessellate>1</tessellate>");
            sw.WriteLine("<altitudeMode>clampToGround</altitudeMode>");
            sw.WriteLine("<outerBoundaryIs>");

            
            addLinearRing(coords);
            sw.WriteLine("</outerBoundaryIs>");
            sw.WriteLine("</Polygon>");
            sw.WriteLine("</Placemark>");
        }
        private void addLinearRing(double[][] coords)
        {
            sw.WriteLine("<LinearRing>");
            sw.Write("<coordinates>");
            foreach (double[] c in coords)
            {
                sw.WriteLine(c[0].ToString() + "," + c[1].ToString() + "," + c[2].ToString());
            }
            sw.WriteLine(coords[0][0].ToString() + "," + coords[0][1].ToString() + "," + coords[0][2].ToString());
            sw.Write("</coordinates>");

            sw.Write("</LinearRing>");
        }
        public void testkml(string filename)
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
                new double[] {-112.2656969554589,36.08649599090644,2357}
            };

            sw = new StreamWriter(filename);
            header();
            sw.WriteLine("<name> Paths </name>");
            sw.WriteLine("<description> </description>");
            defineStyle( "7dff0000", 2, "7dff0000", "#transBluePoly");
            sw.WriteLine("<Placemark>");
            sw.WriteLine("<name>Absolute Extruded</name>");
            sw.WriteLine("<description> testing line strings</description>");
            sw.WriteLine("<styleUrl>#transBluePoly</styleUrl>");
            addLineString( coords, 1, 1);
            sw.WriteLine("</Placemark>");

            fileend();
            
        }
    }
    
}
