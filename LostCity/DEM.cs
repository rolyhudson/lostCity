using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostCity
{
    class DEM
    {
        string folder;
        string currentHGT = "";
        BinaryReader b;
        double north;
        double south;
        double west;
        double east;
        public List<List<double>> hts = new List<List<double>>();
        public List<List<double[]>> ptsLonLat = new List<List<double[]>>();
        int[] threeSecs = { 0, 0, 3 };
        public double threedec = 0;
        double startW=0;
        double startN=0;
        public DEM(string ifolder, double n, double s, double w, double e)
        {
            folder = ifolder;
            north = n;
            south = s;
            west = w;
            east = e;
            threedec = getDecFromDMS(threeSecs);
            getDataCell();
        }
        private void getDataCell()
        {
            //get grid of elevation points starting with nw corner

            double width = (east - west);
            double height = (north - south);
            int cols = (int)(convertToSeconds(width) / 3);
            int rows = (int)(convertToSeconds(height) / 3);
            
            //round start coord
            startN = getDecFromDMS(getDMSfromDec(north));
            startW = getDecFromDMS(getDMSfromDec(west));
            double cellN = 0;
            double cellW = 0;
            int secondsN = 0;
            int secondsW = 0;
            
            
            for (int i = 0; i < rows; i++)
            {
                List<double> column = new List<double>();
                List<double[]> columnLonLat = new List<double[]>();
                for (int j = 0; j < cols; j++)
                {
                    cellN = startN - threedec * i;
                    cellW = startW + threedec * j;
                    double[] coord = { cellW, cellN };
                    columnLonLat.Add(coord);
                    if ((decimal)cellN - Math.Truncate((decimal)cellN) == 0)
                    {
                        cellN = Math.Round(cellN);
                    }
                    if ((decimal)cellW - Math.Truncate((decimal)cellW) == 0)
                    {
                        cellW = Math.Round(cellW);
                    }
                    openFile(cellN, cellW);
                    secondsN = truncateInSeconds(cellN);
                    secondsW = truncateInSeconds(cellW);
                    //check we have the right file
                    column.Add(getSample(secondsN, secondsW));
                }
                ptsLonLat.Add(columnLonLat);
                hts.Add(column);
            }
            b.Close();
            writeDEM("dem.csv");
            writeDEM2("demheader.csv", startN, startW, threedec);
        }
        private int getSample(double ns, double ew)
        {
            //https://gis.stackexchange.com/questions/43743/extracting-elevation-from-hgt-file
            int i = 1200 - (int)(Math.Round(ns / 3, 0));
            //needs to be fixed for eastern grid cells
            int j = 0;
            if (ew>0) j = 1200 - (int)Math.Round(ew / 3, 0);
            int pos = ((i) * 1201 + (j)) * 2;
            byte[] buffer = new byte[2];
            b.BaseStream.Seek(pos, SeekOrigin.Begin);
            b.Read(buffer, 0, 2);
            Array.Reverse(buffer);
            int alt = BitConverter.ToInt16(buffer, 0);
            return alt;
        }
        private void writeDEM(string filepath)
        {
            StreamWriter sw = new StreamWriter(filepath);

            for (int i = 0; i < hts.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j <hts[i].Count; j++)
                {
                    if (j == hts[i].Count - 1) sw.WriteLine(hts[i][j]);
                    else sw.Write(hts[i][j] + ",");
                }
            }
            sw.Close();
        }
        private void writeDEM2(string filepath, double startN, double startW, double cellSize)
        {
            StreamWriter sw = new StreamWriter(filepath);
            sw.WriteLine("NW corner North:," + startN.ToString() + ",NE corner West: ," + startW.ToString() + ",cell size decimal: ,"+cellSize);
            for (int i = 0; i < hts.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < hts[i].Count; j++)
                {
                    if (j == hts[i].Count - 1) sw.WriteLine(hts[i][j]);
                    else sw.Write(hts[i][j] + ",");
                }
            }
            sw.Close();
        }
        private void openFile(double n, double w)
        {
            string file = getFileFromCoord(n, w);
            if (currentHGT != file)
            {
                if (b != null) b.Close();
                b = new BinaryReader(new FileStream(folder + file, FileMode.Open, FileAccess.Read), new ASCIIEncoding());
                currentHGT = file;
            }

        }
        private int convertToSeconds(double deg)
        {
            int seconds = 0;
            var dms = getDMSfromDec(deg);
            seconds = dms[0] * 60 * 60 + dms[1] * 60 + dms[2];
            return seconds;
        }
        private int truncateInSeconds(double deg)
        {
            int[] dms = getDMSfromDec(deg);
            int seconds = dms[1] * 60 + dms[2];
            return seconds;
        }
        private int[] getDMSfromDec(double deg)
        {
            int[] dms = new int[3];
            int degrees = 0;
            if (deg < 0) degrees = (int)Math.Ceiling(deg);
            else degrees = (int)Math.Floor(deg);
            dms[0] = degrees;
            var d = ((decimal)deg - Math.Truncate((decimal)deg));
            var minutes = ((decimal)deg - Math.Truncate((decimal)deg)) * 60;
            var seconds = (minutes - Math.Truncate(minutes)) * 60;
            dms[1] = (int)Math.Floor(Math.Abs(minutes));
            dms[2] = (int)Math.Round(Math.Abs(seconds));
            return dms;
        }
        private double getDecFromDMS(int[] dms)
        {
            double degrees = 0;
            //DD = (Seconds/3600) + (Minutes/60) + Degrees
            if (dms[0] >= 0) degrees = dms[0] + dms[1] / 60.0 + dms[2] / 3600.0;
            //DD = - (Seconds/3600) - (Minutes/60) + Degrees
            else degrees = dms[0] - dms[1] / 60.0 - dms[2] / 3600.0;
            return degrees;
        }
        private string getFileFromCoord(double ns, double ew)
        {
            string filename = "";
            if (ns < 0) filename += "S";
            else filename += "N";

            int nsCell = (int)Math.Floor(Math.Abs(ns));
            if (nsCell < 10) filename += "0" + nsCell.ToString();
            else filename += nsCell.ToString();

            int ewCell = 0;
            if (ew < 0)
            {
                filename += "W";
                ewCell = (int)Math.Ceiling(Math.Abs(ew));
            }
            else
            {
                filename += "E";
                ewCell = (int)Math.Floor(Math.Abs(ew));
            }

            if (ewCell < 100) filename += "0" + ewCell.ToString();
            else filename += ewCell.ToString();

            filename += ".hgt";
            return filename;
        }
    }
}
