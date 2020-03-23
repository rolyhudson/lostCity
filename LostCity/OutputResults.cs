using Accord.Statistics.Analysis;
using Accord.Statistics.Testing;
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
    class OutputResults
    {
        List<double> intervisscores = new List<double>();
        List<double> terrainvisscores = new List<double>();
        double intervisactualScore;
        double terrainvisactualScore;
        string resultsfolder;
        string title;
        public OutputResults(string folder)
        {
            this.resultsfolder = folder;
            if (this.resultsfolder.Contains("noWater"))
            {
                this.title = "Viewshed test results\n" + "random sites located in elevation range 380 to 1750 metres";
            }
            if(this.resultsfolder.Contains("NoSlope"))
            {
                this.title = "Viewshed test results\n" + "random sites located in elevation range 380 to 1750 metres, minimum distance to water source 125m";
            }
            else
            {
                this.title = "Viewshed test results\n" + "random sites located in elevation range 380 to 1750 metres, minimum distance to water source 125m, terrain slope between 16 and 36 degrees";
            }
        }
        public void interVisTest()
        {
            //read from pre summed files
            readResults(resultsfolder + "interVisScores.csv",ref intervisscores,ref intervisactualScore);
            readResults(resultsfolder + "terrainVisScores.csv", ref terrainvisscores, ref terrainvisactualScore);
            CreateGraph();
            //checkDistribution(this.scores.ToArray());
            //zTest();
        }
        private void checkDistribution(double[] samples)
        {
            var analysis = new DistributionAnalysis(samples);

            // Compute the analysis
            analysis.Compute();

            // Get the most likely distribution (first)
            var mostLikely = analysis.GoodnessOfFit[0];

            // The result should be Poisson(x; lambda = 0.420961)
            var result = mostLikely.Distribution.ToString();
        }
        private void zTest(List<double> scores, double hypothesismean)
        {
            // Creates the Simple Descriptive Analysis of the given source
            DescriptiveAnalysis da = new DescriptiveAnalysis(scores.ToArray());
            int sampleSize = scores.Count;
            double sampleMean = da.Means[0];


            double standardDeviation = da.StandardDeviations[0];
            double hypothesizedMean = hypothesismean;
            ZTest test = new ZTest(sampleMean, standardDeviation, sampleSize,
                hypothesizedMean, OneSampleHypothesis.ValueIsSmallerThanHypothesis);

            // Now, we can check whether this result would be
            // unlikely under a standard significance level:

            bool significant = test.Significant;

            // We can also check the test statistic and its P-Value
            double statistic = test.Statistic;
            double pvalue = test.PValue;
        }
        private void descriptiveAnalysis(List<double> scores)
        {
            // Creates the Simple Descriptive Analysis of the given source
            DescriptiveAnalysis da = new DescriptiveAnalysis();
            //da.Learn(scores.ToArray()));
            
        }
        public void readAllResults()
        {
            //read from individual results
            string[] terrainpaths = Directory.GetFiles(this.resultsfolder, "terrainVis*");
            string[] interpaths = Directory.GetFiles(this.resultsfolder, "interVis*");
            totalScore(terrainpaths, ref terrainvisscores, ref terrainvisactualScore);
            totalScore(interpaths, ref intervisscores, ref intervisactualScore);
            CreateGraph();
        }
        private void totalScore(string[] paths, ref List<double> scores,ref double actual)
        {
            
            foreach(string path in paths)
            {
                if (path.Contains("Score")) continue;
                StreamReader sr = new StreamReader(path);
                string line = sr.ReadLine();
                double total = 0;
                while (line != null)
                {
                    var parts = line.Split(',');
                    
                    foreach(string p in parts)
                    {
                        total += Convert.ToDouble(p);
                    }
                    line = sr.ReadLine();
                }
                if (path.Contains("interVisTest0")|| path.Contains("terrainVis0")) actual = total;
                else scores.Add(total);
                
                sr.Close();
            }
            scores.Sort();
        }
        private void readResults(string path,ref List<double> scores, ref double actual)
        {
            StreamReader sr = new StreamReader(path);
            string line = sr.ReadLine();
            actual = Convert.ToDouble(line);
            line = sr.ReadLine();
            while (line != null)
            {
                scores.Add(Convert.ToDouble(line));
                line = sr.ReadLine();
            }
            scores.Sort();
            sr.Close();
        }
        private void addPaneToMaster(MasterPane master,List<double> scores, double hypMean,string titlePrefix)
        {
            GraphPane myPane = new GraphPane();
            myPane.Title.Text = titlePrefix+" test results";
            myPane.XAxis.Title.Text = "Test Num";
            myPane.YAxis.Title.Text = titlePrefix+" Score";
            myPane.XAxis.Scale.Max = 1000;
            myPane.Border.IsVisible = false;
            LineItem myLine = myPane.AddCurve("Randomly generated sites", null, scores.ToArray(), Color.Red, SymbolType.Circle);
            myLine.Line.IsVisible = false;
            myLine.Symbol.Size = 5f;
            double[] score = new double[] { hypMean };
            LineItem myLine2 = myPane.AddCurve("Actual site", null, score, Color.Blue,SymbolType.Circle);
            myLine2.Line.IsVisible = false;
            myLine2.Symbol.Size = 10f;
            myLine2.Symbol.Fill.IsVisible = true;
            myLine2.Symbol.Fill = new Fill(Color.Blue);
            master.Add(myPane);
        }
        private void CreateGraph_JapaneseCandleStick(ZedGraphControl z1)
        {
            GraphPane myPane = z1.GraphPane;

            myPane.Title.Text = "Japanese Candlestick Chart Demo";
            myPane.XAxis.Title.Text = "Trading Date";
            myPane.YAxis.Title.Text = "Share Price, $US";

            StockPointList spl = new StockPointList();
            Random rand = new Random();

            // First day is feb 1st
            XDate xDate = new XDate(2006, 2, 1);
            double open = 50.0;

            for (int i = 0; i < 100; i++)
            {
                double x = xDate.XLDate;
                double close = open + rand.NextDouble() * 10.0 - 5.0;
                double hi = Math.Max(open, close) + rand.NextDouble() * 5.0;
                double low = Math.Min(open, close) - rand.NextDouble() * 5.0;

                StockPt pt = new StockPt(x, hi, low, open, close, 100000);
                spl.Add(pt);

                open = close;
                // Advance one day
                xDate.AddDays(1.0);
                // but skip the weekends
                if (XDate.XLDateToDayOfWeek(xDate.XLDate) == 6)
                    xDate.AddDays(2.0);
            }

            //CandleStickItem myCurve = myPane.AddCandleStick( "trades", spl, Color.Black );
            JapaneseCandleStickItem myCurve = myPane.AddJapaneseCandleStick("trades", spl);
            myCurve.Stick.IsAutoSize = true;
            myCurve.Stick.Color = Color.Blue;

            // Use DateAsOrdinal to skip weekend gaps
            myPane.XAxis.Type = AxisType.DateAsOrdinal;

            // pretty it up a little
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);
            myPane.Fill = new Fill(Color.White, Color.FromArgb(220, 220, 255), 45.0f);

            // Tell ZedGraph to calculate the axis ranges
            z1.AxisChange();
            z1.Invalidate();
        }
        private void CreateGraph()
        {
            ZedGraphControl zg1 = new ZedGraphControl();
            MasterPane master = zg1.MasterPane;
            master.Rect = new RectangleF(0, 0, 1200, 800);
            master.PaneList.Clear();
            master.Title.IsVisible = true;
            master.Title.FontSpec.Size = 16;
            master.Title.Text = this.title;
            master.IsFontsScaled = false;
            master.Margin.All = 10;
            master.Legend.IsVisible = false;
            addPaneToMaster(master, intervisscores, intervisactualScore, "site intervisibility");

            addPaneToMaster(master, terrainvisscores, terrainvisactualScore, "terrain visibility");
            // Refigure the axis ranges for the GraphPanes
            zg1.AxisChange();

            // Layout the GraphPanes using a default Pane Layout
            Bitmap b = new Bitmap(1200, 800);
            using (Graphics g = Graphics.FromImage(b))
            {
                master.SetLayout(g, PaneLayout.SingleColumn);
            }
            master.GetImage().Save(this.resultsfolder+"results.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }
    }
}
