﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using System.IO;
using MIConvexHull;
namespace LostCity
{
    class RefPlaneVis
    {
        List<List<double>> hts;
        List<List<Point3d>> auxHts=new List<List<Point3d>>();
        List<List<int>> visScore = new List<List<int>>();
        double cell;
        public RefPlaneVis(List<List<double>> eledata, double cellSize)
        {
            hts = eledata;
            
            cell = cellSize;
            setScores();
            
        }
        private void makeMesh()
        {
            var points = new List<Vertex>();
            var voronoiMesh = VoronoiMesh.Create<Vertex, Cell>(points);
            var vi = new Vertex()
            foreach (var cell in voronoiMesh.Vertices)
            {
                for (int v = 0; v < 3; v++)
                {
                }
            }
        }
        private void setAux()
        {
            //resets the auxilalry grid based on a cell size in m
            auxHts = new List<List<Point3d>>();
            for (int i = 0; i < hts.Count; i++)
            {
                List<Point3d> row = new List<Point3d>();
                for (int j = 0; j < hts[i].Count; j++)
                {
                    Point3d p = new Point3d(i * cell, j * -cell, hts[i][j]);
                    row.Add(p);
                }
                auxHts.Add(row);
            }
        }
        private void setScores()
        {
            visScore = new List<List<int>>();
            for(int i=0;i<hts.Count;i++)
            {
                List<int> row = new List<int>();
                for(int j=0;j<hts[i].Count;j++)
                {
                    row.Add(0);
                }
                visScore.Add(row);
            }
        }
        private void sectorEdge(int sector, int vprow, int vpcol)
        {
            int rowInc = 0;
            int colInc = 0;
            Point3d viewPoint = auxHts[vprow][vpcol];
            
            switch (sector)
            {
                case 0:
                    rowInc = 0;
                    colInc = 1;
                   
                    break;
                case 1:
                    rowInc = -1;
                    colInc = 1;
                    
                    break;
                case 2:
                    rowInc = -1;
                    colInc = 0;
                    
                    break;
                case 3:
                    rowInc = -1;
                    colInc = -1;
                    
                    break;
                case 4:
                    rowInc = 0;
                    colInc = -1;
                    
                    break;
                case 5:
                    rowInc = 1;
                    colInc = -1;
                    
                    break;
                case 6:
                    rowInc = 1;
                    colInc = 0;
                    
                    break;
                case 7:
                    rowInc = 1;
                    colInc = 1;
                    
                    break;
            }
            int i = vprow + rowInc;
            int j = vpcol + colInc;
            bool inside = insidegrid(i,j);
            Point3d sample = new Point3d();
            Point3d prevsample = new Point3d();
            
            double viewAngle = 0;
            double testAngle = 0;
            Vector3d viewVector = new Vector3d();
            bool visible =true;
            double h = 0;
            while(inside)
            {
                sample = auxHts[i][j];
                viewVector = sample - viewPoint;
                viewAngle = Vector3d.VectorAngle(Vector3d.ZAxis, viewVector);
                //test against previous
                //loop through prev samples
                if(prevsample.Z!=0)
                {
                    viewVector = prevsample - viewPoint;
                    testAngle = Vector3d.VectorAngle(Vector3d.ZAxis, viewVector);
                    if(testAngle>viewAngle)
                    {
                        //visibility
                        visible = true;
                    }
                    else
                    {
                        visible = false;
                        //set ht equal to intersection pt
                        h = cell / Math.Tan(testAngle);
                        auxHts[i][j] = new Point3d(sample.X,sample.Y, prevsample.Z+h);
                        
                    }
                    
                }
                prevsample = auxHts[i][j];
                if (visible) visScore[i][j]++;
                
                i += rowInc;
                j += colInc;
                inside = insidegrid(i, j);
            }

        }
        private bool insidegrid(int row, int col)
        {
            if (row > auxHts.Count-1) return false;
            if (row < 0) return false;
            if (col > auxHts[0].Count-1) return false;
            if (col < 0) return false;
            return true;
        }
        public void singlePoint(int i, int j)
        {
            setAux();
            //score all rh edge diagonal or NSEW
            for (int s = 0; s < 8; s++)
            {
                sectorEdge(s, i, j);
            }
            //score sectors internal points
            for (int s = 0; s < 8; s++)
            {
                processSector(s, i, j);
                
            }
            
        }
        
        private void writeVisSector(int s)
        {
            StreamWriter sw = new StreamWriter("sectorVisTest-"+s+".csv");
            for (int i = 0; i < visScore.Count; i++)
            {
                for (int j = 0; j < visScore[i].Count; j++)
                {
                    if (j < visScore[i].Count - 1) sw.Write(visScore[i][j] + ",");
                    else sw.WriteLine(visScore[i][j]);
                }
            }
            sw.Close();
        }
        public void writeVis(string filename)
        {
            StreamWriter sw = new StreamWriter(filename + ".csv");
            for (int i = 0; i < visScore.Count; i++)
            {
                for (int j = 0; j < visScore[i].Count; j++)
                {
                    if (j < visScore[i].Count - 1) sw.Write(visScore[i][j] + ",");
                    else sw.WriteLine(visScore[i][j]);
                }
            }
            sw.Close();
        }
        private void writeVis()
        {
            StreamWriter sw = new StreamWriter("edgeVisTest.csv");
            for(int i=0;i<visScore.Count;i++)
            {
                for(int j=0; j<visScore[i].Count;j++)
                {
                    if(j<visScore[i].Count-1) sw.Write(visScore[i][j] + ",");
                    else sw.WriteLine(visScore[i][j]);
                }
            }
                sw.Close();
        }
        private void processSector(int sector, int vprow, int vpcol)
        {
           
            Point3d xp = new Point3d();
            Point3d yp = new Point3d();
            //vp is viewer location
            Point3d vp = new Point3d();
            vp = auxHts[vprow][vpcol];
            
            //ref to plane point relative to sample
            int xrefi = 0;
            int yrefi = 0;
            int xrefj = 0;
            int yrefj = 0;
            //set first sample indexes
            int i = 0;
            int j = 0;
            switch (sector)
            {
                case 0:
                    i = vprow - 1;
                    j = vpcol + 2;
                    xrefi = 1;
                    xrefj = -1;
                    yrefi = 0;
                    yrefj = -1;
                    break;
                case 1:
                    i = vprow - 2;
                    j = vpcol + 1;
                    xrefi = 1;
                    xrefj = 0;
                    yrefi = 1;
                    yrefj = -1;
                    break;
                case 2:
                    i = vprow - 2;
                    j = vpcol - 1;
                    xrefi = 1;
                    xrefj = 1;
                    yrefi = 1;
                    yrefj = 0;
                    break;
                case 3:
                    i = vprow - 1;
                    j=vpcol - 2;
                    xrefi = 0;
                    xrefj = 1;
                    yrefi = 1;
                    yrefj = 1;
                    break;
                case 4:
                    i = vprow + 1;
                    j=vpcol - 2;
                    xrefi = -1;
                    xrefj = 1;
                    yrefi = 0;
                    yrefj = 1;
                    break;
                case 5:
                    i = vprow + 2;
                    j =vpcol - 1;
                    xrefi = -1;
                    xrefj = 0;
                    yrefi = -1;
                    yrefj = 1;
                    break;
                case 6:
                    i = vprow + 2;
                    j=vpcol + 1;
                    xrefi = -1;
                    xrefj = -1;
                    yrefi = -1;
                    yrefj = 0;
                    break;
                case 7:
                    i = vprow + 1;
                    j = vpcol + 2;
                    xrefi = 0;
                    xrefj = -1;
                    yrefi = -1;
                    yrefj = -1;
                    break;
            }
            //ij is the first point to test
            //are the view point indices in the grid
            bool inside = insidegrid(i, j);
            Point3d sample = new Point3d();
            List<int[]> indices = new List<int[]>();
            Line vert = new Line();
            double param = 0;
            if (inside)
            {
               //get indices of points in sector
                indices = sectorIndices(sector, i, j);
                //checkSector(indices,sector);
                foreach (int[] ind in indices)
                {
                    //get the xpoint and y point
                    xp = auxHts[ind[0] + xrefi][ind[1] + xrefj];
                    yp = auxHts[ind[0] + yrefi][ind[1] + yrefj];
                    //make the plane
                    Plane vPlane = new Plane(vp, xp, yp);
                    sample = auxHts[ind[0]][ind[1]];
                    vert = new Line(sample, Vector3d.ZAxis, 1);

                    Rhino.Geometry.Intersect.Intersection.LinePlane(vert, vPlane, out param);
                    Point3d planePt = vert.PointAt(param);

                    if (planePt.Z > sample.Z)
                    {
                        //no sightline
                        //set aux z = to projected plane pt z
                        auxHts[ind[0]][ind[1]] = planePt;
                    }
                    else
                    {
                        //sightline
                        visScore[ind[0]][ind[1]]++;
                        //set aux z = to sample z
                        auxHts[ind[0]][ind[1]] = sample;
                    }
                }
            }
            
            
        }
        private void checkSector(List<int[]> indices,int snum)
        {
            StreamWriter sw = new StreamWriter("sectorCheck_"+snum+".csv");
            int v = 0;
            for (int i = 0; i < indices.Count; i++)
            {
                sw.WriteLine(indices[i][0].ToString() + "," + indices[i][1].ToString());
            }

            sw.Close();
        }
        private List<int[]> sectorIndices(int sector, int row, int col)
        {
            List<int[]> indexList = new List<int[]>();
            
            int j = 0;
            int i = 0;
            int onGrid = 0;
            //set the first sample point
            int[] pair = { row,col};
            indexList.Add(pair);
            switch (sector)
            {
                case 0:
                    //increment in j+ i- col first
                    for(;;)
                    {
                        j++;
                        onGrid = 0;
                        for (i=0;i>-(j+1);i--)
                        {
                            if(insidegrid(row+i,col+j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }
                        }
                        if (onGrid == 0) break;
                    }
                    break;
                case 1:
                    //increment in i- j+ row first
                    for (;;)
                    {
                        i--;
                        onGrid = 0;
                        for (j = 0; j < -(i - 1); j++)
                        {
                            if (insidegrid(row + i, col + j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }

                        }
                        if (onGrid == 0) break;
                    }
                    break;
                case 2:
                    //increment in i- j- row first
                    for (;;)
                    {
                        i--;
                        onGrid = 0;
                        for (j = 0; j > (i - 1); j--)
                        {
                            if (insidegrid(row + i, col + j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }
                        }
                        if (onGrid == 0) break;
                    }
                    break;
                case 3:
                    //increment in j- i- col first
                    for (;;)
                    {
                        j--;
                        onGrid = 0;
                        for (i = 0; i > (j - 1); i--)
                        {
                            if (insidegrid(row + i, col + j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }
                        }
                        if (onGrid == 0) break;
                    }
                    break;
                case 4:
                    //increment in j- i+ col first
                    for (;;)
                    {
                        j--;
                        onGrid = 0;
                        for (i = 0; i < -(j - 1); i++)
                        {
                            if (insidegrid(row + i, col + j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }
                        }
                        if (onGrid == 0) break;
                    }
                    break;
                case 5:
                    //increment in i+ j- row first
                    for (;;)
                    {
                        i++;
                        onGrid = 0;
                        for (j = 0; j > -(i + 1); j--)
                        {
                            if (insidegrid(row + i, col + j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }
                        }
                        if (onGrid == 0) break;
                    }
                    break;
                case 6:
                    //increment in i+ j+ row first
                    for (;;)
                    {
                        i++;
                        onGrid = 0;
                        for (j = 0; j < (i + 1); j++)
                        {
                            if (insidegrid(row + i, col + j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }
                        }
                        if (onGrid == 0) break;
                    }
                    break;
                case 7:
                    //increment in j+ i+  col first
                    for (;;)
                    {
                        j++;
                        onGrid = 0;
                        for (i = 0; i < (j + 1); i++)
                        {
                            if (insidegrid(row + i, col + j))
                            {
                                pair = new int[2];
                                pair[0] = row + i;
                                pair[1] = col + j;
                                indexList.Add(pair);
                                onGrid++;
                            }
                        }
                        if (onGrid == 0) break;
                    }
                    break;
            }
            return indexList;
        }
        public void pointRange(int[] row, int[] cols)
        {
            for (int i = row[0]; i < row[1]; i++)
            {
                for (int j = cols[0]; j < cols[1]; j++)
                {
                    singlePoint(i, j);
                }

            }
        }
        public void pointList(List<int[]> pts)
        {
            foreach(int[] pt in pts)
            {
                singlePoint(pt[0], pt[1]);
            }
        }
        public void traverse()
        {
            for (int i = 0; i < hts.Count; i++)
            {
                for (int j = 0; j < hts[i].Count; j++)
                {
                    singlePoint(i, j);
                }
            }
        }
    }
}
