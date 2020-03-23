using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Visualizations;
using Accord.Collections;
using System.Device.Location;

namespace LostCity
{
    class LostCityObjects
    {
        
        List<Polyline> rios = new List<Polyline>();
        KDTree<double> rioTree;
        List<Polyline> caminos = new List<Polyline>();
        List<Sitio> sitios = new List<Sitio>();
        List<Polyline> boundaries = new List<Polyline>();
        List<RefPlaneVis> interVisResults = new List<RefPlaneVis>();
        List<RefPlaneVis> terrainVisResults = new List<RefPlaneVis>();
        DEM dem;
        RefPlaneVis rpv;
        List<List<int[]>> indicesForAnalysis;
        bool getTerrain = true;
        bool getInterVis = true;
        int nConfigs = 1000;
        string resultsFolder = @"C:\Users\Admin\Documents\projects\LostCity\results\";
        public LostCityObjects()
        {
          
            dem = new DEM(@"C:\Users\Admin\Documents\projects\LostCity\C18\C18\", 11.140131, 10.975654, -74.000914, -73.836385);
            readObjects();

            analyseVisibility();
            totalScore();
            //logReg();
            //writeObjectsToKML();
        }
        private void writeObjectsToKML()
        {
            KMLwriter kmlw = new KMLwriter();
            kmlw.sw = new StreamWriter("kmltest.kml");
            kmlw.header();
            kmlw.defineStyle("64FF7800", 5, "fBE7800", "rios");
            kmlw.defineStyle("6414F0FF", 5, "fBE7800", "caminos");
            kmlw.defineStyle("641400FF", 10, "641400FF", "sitios");
            double score = 0;
            List<double> scores = new List<double>();
            for (int i = 0; i < rpv.visScore.Count - 1; i++)
            {
                for (int j = 0; j < rpv.visScore[i].Count - 1; j++)
                {
                    score = ((rpv.visScore[i][j] + rpv.visScore[i][j + 1] + rpv.visScore[i + 1][j] + rpv.visScore[i + 1][j + 1]));
                    scores.Add(score);
                }
            }

            Histogram histo = new Histogram();
            
            histo.Compute(scores.ToArray(), 5);
            for(int b = 0; b < histo.Bins.Count; b++)
            {
                Color c = rainbowRGB(histo.Bins[b].Range.Max, scores.Max(), scores.Min());
                string hex = HexConverter(c);
                kmlw.defineStyle(hex, 1, hex, "viewBin_"+b);
            }
            kmlw.openFolder("rios");
            foreach (Polyline pl in rios)
            {
                kmlw.addPlacemarkLineString("rio", "rio path", pl.vertices.ToArray(), "#rios");
            }
            kmlw.closeFolder();
            kmlw.openFolder("caminos");
            foreach (Polyline pl in caminos)
            {
                kmlw.addPlacemarkLineString("caminos", "caminos path", pl.vertices.ToArray(), "#caminos");
            }
            kmlw.closeFolder();
            kmlw.openFolder("sitios");
            foreach (Sitio s in sitios)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Site: " + s.name);
                sb.AppendLine("Rank in hierachy: "+s.rank);
                sb.AppendLine("Area: "+s.area+"ha");
                sb.AppendLine("Population according to book: " + s.populationL);
                sb.AppendLine("Population according to formula: " + s.populationF);
                
                if (s.boundary != null)
                {
                    kmlw.addPlacemarkLineString(s.name, sb.ToString(), s.boundary.vertices.ToArray(), "#sitios");
                }
                
            }
            kmlw.closeFolder();
            kmlw.openFolder("terrain visibility from top 3 sites");
            
            for(int i=0;i<rpv.visScore.Count-1;i++)
            {
                for(int j=0;j< rpv.visScore[i].Count-1; j++)
                {
                    score = (rpv.visScore[i][j] + rpv.visScore[i][j + 1] + rpv.visScore[i + 1][j] + rpv.visScore[i + 1][j + 1]);
                    if (score > 5)
                    {
                        double[][] coords = new double[][] {
                    new double[]{dem.ptsLonLat[i][j][0], dem.ptsLonLat[i][j][1], dem.hts[i][j]},
                    new double[]{dem.ptsLonLat[i][j+1][0], dem.ptsLonLat[i][j+1][1], dem.hts[i][j+1]},
                    new double[]{dem.ptsLonLat[i+1][j+1][0], dem.ptsLonLat[i+1][j+1][1], dem.hts[i+1][j+1]},
                    new double[] { dem.ptsLonLat[i+1][j][0], dem.ptsLonLat[i+1][j][1], dem.hts[i+1][j]},
                    new double[]{dem.ptsLonLat[i][j][0], dem.ptsLonLat[i][j][1], dem.hts[i][j]}
                    };
                        string style = "";
                        for (int b = 0; b < histo.Bins.Count; b++)
                        {
                            if (histo.Bins[b].Contains(score)) style = "viewBin_" + b;
                        }
                            kmlw.addPlacemarkPolygon("view quality", score.ToString(), coords, style);
                    }
                    
                }
                
            }
            kmlw.closeFolder();
            kmlw.fileend();
            
            //kmlw.testkml("kmlTest.kml");
        }
        private void readObjects()
        {
            rios = MapTools.readPolylines(@"C:\Users\Admin\Documents\projects\LostCity\georefObjects\rios.csv", false);
            //rios = MapTools.readGeoJSON(@"C:\Users\Admin\Documents\projects\LostCity\GISprocesses\whitebox proj1\PythonApplication1\WBT\data\test.geojson");
            //MapTools.writePolylines(rios, @"C:\Users\Admin\Documents\projects\LostCity\georefObjects\rivertest.csv");
            rioTree = setKDTreeFromPolylines(rios);
            caminos = MapTools.readPolylines(@"C:\Users\Admin\Documents\projects\LostCity\georefObjects\caminos.csv", false);
            getSitioData();
        }
        
        private void analyseDistToWater()
        {
            List<double[]> waterDist = new List<double[]>();
            List<double[]> means = new List<double[]>();
            List<double[]> medians = new List<double[]>();
            List < Accord.DoubleRange> ranges = new List<Accord.DoubleRange>();
            StreamWriter sw = new StreamWriter(resultsFolder+"actualWater_SlopeAnalysis.csv",false, Encoding.UTF8);
            sw.WriteLine("site,waterDist,slope");
            foreach (Sitio s in this.sitios)
            {
                List<double> dists = new List<double>();
                List<double> slopes = new List<double>();
                 foreach (int[] p in s.gridPoints)
                {
                    //p is i,j of the dem
                    var pt = dem.ptsLonLat[p[0]][p[1]];
                    double lat = pt[1];
                    double lon = pt[0];
                    double[] query = new double[] { lon, lat };
                    var closestPt = this.rioTree.Nearest(query);
                    GeoCoordinate site = new GeoCoordinate(lat, lon);
                    GeoCoordinate riverpt = new GeoCoordinate(closestPt.Position[1], closestPt.Position[0]);
                    double dist = site.GetDistanceTo(riverpt);
                    //dists.Add(dist);
                    //slopes.Add(this.dem.slope[p[0]][p[1]]);
                    sw.WriteLine(s.name + "," + dist.ToString() + "," + this.dem.slope[p[0]][p[1]]);
                }
                if (dists.Count > 0)
                {
                    // Create the descriptive analysis
                    //var wateranalysis = new DescriptiveAnalysis(dists.ToArray());
                    //var slopeananlysis = new DescriptiveAnalysis(slopes.ToArray());
                    
                    //sw.WriteLine("site name", "num DEM pts", "mean dist", "median dist", "mode dist", "closest dist", "furthest dist");
                    
                    //sw.WriteLine(outDescripAnalysis(s, wateranalysis)+ outDescripAnalysis(s, slopeananlysis));
                }
                
            }
            sw.Close();
        }
        private string outDescripAnalysis(Sitio s, DescriptiveAnalysis da)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.name + ",");
            sb.Append(s.gridPoints.Count + ",");

            sb.Append(da.Means[0] + ",");
            sb.Append(da.Medians[0] + ",");
            sb.Append(da.Modes[0] + ",");
            sb.Append(da.Ranges[0].Min + ",");
            sb.Append(da.Ranges[0].Max + ",");
            return sb.ToString();
        }

        private void siteScores()
        {
            
            foreach (Sitio s in this.sitios)
            {
                int score = 0;
                foreach (int[] pt in s.gridPoints)
                {
                    score+= rpv.visScore[pt[0]][pt[1]];
                }
                if(rpv.name.Contains("terrain"))
                {
                    s.terrainVisScore = score;
                }
                else
                {
                    s.interVisScore = score;
                }
            }
            
        }
        private void printSiteScores()
        {
            StreamWriter sw = new StreamWriter(this.resultsFolder + "siteScores.csv", false, Encoding.UTF8);
            sw.WriteLine("name,pts,terrain,intervisibility,area1,population,area2");
            foreach (Sitio s in this.sitios)
            {
                if (s.boundary != null)
                {
                    sw.WriteLine(s.name + "," + s.gridPoints.Count + "," + s.terrainVisScore + "," + s.interVisScore + "," + s.area + "," + s.populationL + "," + Math.Abs(s.boundary.areaHa));

                }
            }
            sw.Close();
        }
        private void analyseVisibility()
        {
            getIndices();
            analyseDistToWater();
            printwantedIndices(this.resultsFolder + "settlement0.csv");
            if (this.getTerrain)
            {
                rpv = new RefPlaneVis(dem, 90, "actual sites terrain", 0);
                rpv.terrainVisibility(indicesForAnalysis);
                siteScores();
                rpv.writeVis("terrainVis" + 0);
                terrainVisResults.Add(rpv);
            }
            if (this.getInterVis)
            {
                rpv = new RefPlaneVis(dem, 90, "actual sites intervisibility", 0);
                rpv.interVisibility(sitios);
                siteScores();
                rpv.writeVis("interVisTest" + 0);
                interVisResults.Add(rpv);
            }
            printSiteScores();
            generateTestRanSites();
        }
        
        private void generateTestRanSites()
        {
            for (int r = 0; r < this.nConfigs; r++)
            {
                RandomSettlement rs = new RandomSettlement(sitios, dem.demPts, this.rioTree, dem.slope, true,true);
                printRandomSettlement(rs, this.resultsFolder + "settlement" + (r + 1) + ".csv");
                if (getTerrain)
                {
                    rpv = new RefPlaneVis(dem, 90, "random sites terrain " + (r + 1), (r + 1));
                    rpv.terrainVisibility(rs.indicesForAnalysis);
                    rpv.writeVis("terrainVis" + (r + 1));
                    terrainVisResults.Add(rpv);
                }
                if (getInterVis)
                {
                    rpv = new RefPlaneVis(dem, 90, "random sites intervisibility " + (r + 1), (r + 1));
                    rpv.interVisibility(rs.sitiosRandom);
                    rpv.writeVis("interVisTest" + (r + 1));
                    interVisResults.Add(rpv);
                }
                
            }
        }
        private void totalScore()
        {
            List<int> terrainVisTotalScores = new List<int>();
            foreach (RefPlaneVis rpv in terrainVisResults)
            {
                int score = 0;
                for (int i = 0; i < rpv.visScore.Count; i++)
                {
                    for (int s = 0; s < rpv.visScore[i].Count; s++)
                    {
                        score += rpv.visScore[i][s];
                    }
                }
                terrainVisTotalScores.Add(score);
            }
            printScores(terrainVisTotalScores, this.resultsFolder + "terrainVisScores.csv");
            List<int> interVisTotalScores = new List<int>();
            foreach (RefPlaneVis rpv in interVisResults)
            {
                int score = 0;
                for (int i = 0; i < rpv.visScore.Count; i++)
                {
                    for (int s = 0; s < rpv.visScore[i].Count; s++)
                    {
                        score += rpv.visScore[i][s];
                    }
                }
                interVisTotalScores.Add(score);
                
            }
            printScores(interVisTotalScores, this.resultsFolder + "interVisScores.csv");
        }
        private void printScores(List<int> visTotalScores,string path)
        {
            StreamWriter sw = new StreamWriter(path);
            foreach (int s in visTotalScores) sw.WriteLine(s);
            sw.Close();
        }
        private void logReg()
        {
            int numObs = terrainVisResults[0].visScore.Count * terrainVisResults[0].visScore[0].Count;
            
            
            foreach(RefPlaneVis rpv in terrainVisResults)
            {
                double[][] input = new double[numObs][];
                double[] output = new double[numObs];
                int oNum = 0;
                for (int i=0;i<rpv.visScore.Count;i++)
                {
                    for(int s=0;s< rpv.visScore[i].Count;s++)
                    {
                        double score = rpv.visScore[i][s];
                        if (Double.IsNaN(score)) score = 0;
                       input[oNum] = new double[] { score, rpv.groupNum,  score + rpv.groupNum };//score * rpv.groupNum,
                        if (rpv.observationPts[i][s])
                        {
                            output[oNum] = 1;
                        }
                        else
                        {
                            output[oNum] = 0;
                        }
                        oNum++;
                    }
                }
                printInputOutput(input, output);
                var lra = new LogisticRegressionAnalysis();


                // Now, we can use the learner to finally estimate our model:
                LogisticRegression regression = lra.Learn(input, output);
                var cf = lra.Coefficients;
            } 
        }
        private void printInputOutput(double[][] input, double[] output)
        {
            StreamWriter sw = new StreamWriter("inputoutput.csv");
            sw.WriteLine("score,groupNum, score + rpv.groupNum,settled");
            for(int i=0;i<input.Length;i++)
            {
                sw.WriteLine(input[i][0] + "," + input[i][1] + "," + input[i][2] + "," + output[i]);
            }
            sw.Close();
        }
        private void getIndices()
        {
            indicesForAnalysis = new List<List<int[]>>();
            for (int s = 0; s < sitios.Count; s++)
            {
                if (sitios[s].boundary != null)
                {
                    for (int i = 0; i < dem.ptsLonLat.Count; i++)
                    {
                        for (int j = 0; j < dem.ptsLonLat[i].Count; j++)
                        {
                            
                            if (MapTools.isPointInPolygon(dem.ptsLonLat[i][j], sitios[s].boundary.vertices))
                            {
                                int[] index = { i, j };
                                
                                sitios[s].gridPoints.Add(index);
                                
                            }
                        }
                    }
                    indicesForAnalysis.Add(sitios[s].gridPoints);
                }
            }
            
        }
        private void printwantedIndices(string path)
        {
            StreamWriter sw = new StreamWriter(path);
            for (int i = 0; i < indicesForAnalysis.Count; i++)
            {
                for (int j = 0; j < indicesForAnalysis[i].Count; j++)
                {
                    if (j < indicesForAnalysis[i].Count - 1) sw.Write(indicesForAnalysis[i][j][0] + "," + indicesForAnalysis[i][j][1] + ",");
                    else sw.WriteLine(indicesForAnalysis[i][j][0]+","+ indicesForAnalysis[i][j][1]);
                }
            }
            sw.Close();
        }
        private void printRandomSettlement(RandomSettlement rs,string path)
        {
            StreamWriter sw = new StreamWriter(path);
            for (int i = 0; i < rs.sitiosRandom.Count; i++)
            {
                for (int j = 0; j < rs.sitiosRandom[i].gridPoints.Count; j++)
                {
                    if (j < rs.sitiosRandom[i].gridPoints.Count - 1) sw.Write(rs.sitiosRandom[i].gridPoints[j][0] + "," + rs.sitiosRandom[i].gridPoints[j][1] + ",");
                    else sw.WriteLine(rs.sitiosRandom[i].gridPoints[j][0] + "," + rs.sitiosRandom[i].gridPoints[j][1]);
                }
            }
            sw.Close();
        }
        private void getSitioData()
        {
            boundaries = MapTools.readPolylines(@"C:\Users\Admin\Documents\projects\LostCity\georefObjects\sitios.csv", true);
            StreamReader sr = new StreamReader(@"C:\Users\Admin\Documents\projects\LostCity\georefObjects\sitioData.csv");
            string line = sr.ReadLine();
            int rank = 1;
            while(line!= null)
            {
                string[] parts = line.Split(',');
                Sitio s = new Sitio();
                s.rank = rank;
                s.name = parts[0];
                s.area = Convert.ToDouble(parts[1]);
                s.populationL = Convert.ToDouble(parts[2]);
                s.populationF = Convert.ToDouble(parts[3]);
                s.boundary = boundaries.Find(x => x.name == s.name);
                sitios.Add(s);
                line = sr.ReadLine();
                rank++;
            }
            sr.Close();
        }
        private KDTree<double> setKDTreeFromPolylines(List<Polyline> polylines)
        {
            //2d tree
            List<double[]> points = new List<double[]>();
            foreach (Polyline pl in polylines)
            {
                foreach(double[] vertex in pl.vertices)
                {
                    double[] pt = new double[] { vertex[0], vertex[1] };
                    
                    points.Add(pt);
                }
            }
            // To create a tree from a set of points, we use
            KDTree<double> tree = KDTree.FromData<double>(points.ToArray());
            return tree;
        }
        private void testKDTree()
        {
            // This is the same example found in Wikipedia page on
            // k-d trees: http://en.wikipedia.org/wiki/K-d_tree

            // Suppose we have the following set of points:

            double[][] points =
            {
                new double[] { 2, 3 },
                new double[] { 5, 4 },
                new double[] { 9, 6 },
                new double[] { 4, 7 },
                new double[] { 8, 1 },
                new double[] { 7, 2 },
            };


            // To create a tree from a set of points, we use
            KDTree<int> tree = KDTree.FromData<int>(points);

            // Now we can manually navigate the tree
            KDTreeNode<int> node = tree.Root.Left.Right;

            // Or traverse it automatically
            foreach (KDTreeNode<int> n in tree)
            {
                double[] location = n.Position;
//Assert.AreEqual(2, location.Length);
            }

            // Given a query point, we can also query for other
            // points which are near this point within a radius

            double[] query = new double[] { 5, 3 };
            
            // Locate all nearby points within an euclidean distance of 1.5
            // (answer should be be a single point located at position (5,4))
            var result = tree.Nearest(query, radius: 1.5);

            // We can also use alternate distance functions
            //tree.Distance = Accord.Math.Distance.Manhattan;

            // And also query for a fixed number of neighbor points
            // (answer should be the points at (5,4), (7,2), (2,3))
            var neighbors = tree.Nearest(query, neighbors: 3);
        }
        private Color rainbowRGB(double value, double max, double min)
        {
            double range = max - min;
            double percent = (value - min) / range;
            double startCol = 0.2;
            double endCol = 1;
            int red;
            int grn;
            int blu;
            Color rgb = new Color();
            //percent is the position in the spectrum 0 = red 1 = violet
            //first flip 

            percent = 1 - percent;
            double threeSixty = Math.PI * 2;
            //but then we shift to squeeze into the desired range
            double scaledCol = percent * (endCol - startCol) + startCol;
            //startCol is a % into the roygbinv spectrum
            //endCol is a % before the end of the roygbinv spectrum
            red = Convert.ToInt16(Math.Sin(threeSixty * scaledCol + 2 * Math.PI / 3) * 128 + 127);
            grn = Convert.ToInt16(Math.Sin(threeSixty * scaledCol + 4 * Math.PI / 3) * 128 + 127);
            blu = Convert.ToInt16(Math.Sin(threeSixty * scaledCol + 0) * 128 + 127);
            if (red < 0) red = 0; if (red > 255) red = 255;
            if (grn < 0) grn = 0; if (grn > 255) grn = 255;
            if (blu < 0) blu = 0; if (blu > 255) blu = 255;

            ColorMine.ColorSpaces.Rgb colMRGB = new ColorMine.ColorSpaces.Rgb { R = red, B = blu, G = grn };
            colMRGB.R = red;
            colMRGB.G = grn;
            colMRGB.B = blu;
            var colMHSV = colMRGB.To<ColorMine.ColorSpaces.Hsv>();
            colMHSV.S = colMHSV.S * 0.7;
            colMRGB = colMHSV.To<ColorMine.ColorSpaces.Rgb>();
            rgb = Color.FromArgb((int)colMRGB.R, (int)colMRGB.G, (int)colMRGB.B);
            return rgb;
        }
        private String HexConverter(System.Drawing.Color c)
        {
            //7f gives 50% opacity
            return "7f" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}
