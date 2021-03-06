﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LostCity
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }
        private void readAll()
            {
            using (FileStream fs = new FileStream(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\B18\B18\N04W073.hgt", FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                {
                    byte[] chunk;

                    chunk = br.ReadBytes(2);
                    while(chunk.Length > 0)
                    {
                        //DumpBytes(chunk, chunk.Length);
                        //chunk.Reverse();
                        Array.Reverse(chunk);
                        int i = BitConverter.ToInt16(chunk, 0);
                        chunk = br.ReadBytes(2);
                    }
                }
            }
        }
        private void singlePoint(object sender, EventArgs e)
        {
            DEM hgtR = new DEM(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\C18\C18\", 11.140131, 10.975654, -74.000914, -73.836385);
            RefPlaneVis rpv = new RefPlaneVis(hgtR, 90, "ciudadPViewShedSingleP",0);
            rpv.singlePoint(124, 90);
            rpv.writeVis("ciudadPViewShedSingleP");
        }
        private void pointrange(object sender, EventArgs e)
        {
            
            DEM hgtR = new DEM(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\C18\C18\", 11.140131, 10.975654, -74.000914, -73.836385);
            RefPlaneVis rpv = new RefPlaneVis(hgtR, 90, "ciudadPViewShedPointRange",0);
            //rpv.singlePoint(124,91);
            int[] row = { 120, 128 };
            int[] cols = { 90, 91 };
            rpv.pointRange(row, cols);
            rpv.writeVis("ciudadPViewShedPointRange");
        }
        private void fullViewShed(object sender, EventArgs e)
        {
            //10.991427, -74.063284
            DEM hgtR = new DEM(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\C18\C18\", 11.140131, 10.975654, -74.000914, -73.836385);
            RefPlaneVis rpv = new RefPlaneVis(hgtR, 90, "ciudadPViewShedFull",0);
            rpv.traverse();
            rpv.writeVis("ciudadPViewShedFull");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //readAll();
            //openFile(11.140131, -74.000914);
            //int secondsN = truncateInSeconds(11.140131);
            //int secondsW = truncateInSeconds(-74.000914);
            //getSample(secondsN, secondsW);
            //south west corner 10.975654, -74.000914
            //ne corner 11.140131, -73.836385
            // need N10W074 and N11W074
            DEM hgtR = new DEM(@"C:\Users\r.hudson\Documents\WORK\projects\LostCity\C18\C18\", 11.140131, 10.975654, -74.000914, -73.836385);
            RefPlaneVis rpv = new RefPlaneVis(hgtR, 90, "ciudadPViewShed",0);
            //rpv.singlePoint(124,91);
            int[] row = { 120, 128 };
            int[] cols = { 90, 91 };
            rpv.pointRange(row, cols);
            rpv.writeVis("ciudadPViewShed");
            //rpv.traverse();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            LostCityObjects lco = new LostCityObjects();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //testCompare();
            string[] filePaths = Directory.GetFiles(@"C:\Users\Admin\Documents\projects\LostCity\results\waterSlope", "settlement*");
            RandomSettlement.makeSiteMaps(filePaths.ToList());
            OutputResults st = new OutputResults(@"C:\Users\Admin\Documents\projects\LostCity\results\waterSlope\");
            //st.readAllResults();
            st.interVisTest();
            //st = new OutputResults(@"C:\Users\Admin\Documents\projects\LostCity\results\withWater\");
            //st.interVisTest();
            
        }
        private void testCompare()
        {
            List<List<int[]>> list = new List<List<int[]>>();
            StreamReader sr = new StreamReader(@"C:\Users\Admin\Documents\projects\LostCity\results\settlement2.csv");
            string line = sr.ReadLine();
            while (line != null)
            {
                var parts = line.Split(',');
                List<int[]> linelist = new List<int[]>();
                for (int i = 0; i < parts.Length-1; i+=2)
                {
                    int[] p = new int[] { Convert.ToInt32(parts[i]), Convert.ToInt32(parts[i + 1]) };
                    linelist.Add(p);
                }
                bool flag = linelist.Any(p => p.SequenceEqual(new int[] { 100, 96 }));
                list.Add(linelist);
                line = sr.ReadLine();
            }
            sr.Close();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            DrainageNet dn = new DrainageNet();
            dn.flowDirection();
            dn.printFlowIndices();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            VisNet vnet = new VisNet();
            vnet.analysis();
        }
    }
}
