/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/
using MIConvexHull;
namespace LostCity
{



    public class Cell : TriangulationCell<Vertex, Cell>
    {
        static System.Random rnd = new System.Random();





        double Det(double[,] m)
        {
            return m[0, 0] * ((m[1, 1] * m[2, 2]) - (m[2, 1] * m[1, 2])) - m[0, 1] * (m[1, 0] * m[2, 2] - m[2, 0] * m[1, 2]) + m[0, 2] * (m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1]);
        }

        double LengthSquared(double[] v)
        {
            double norm = 0;
            for (int i = 0; i < v.Length; i++)
            {
                var t = v[i];
                norm += t * t;
            }
            return norm;
        }




    }
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Vertex : IVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public Vertex(double[] location)
        {
            Position = location;
        }

        public Vertex(double[] location, double result)
        {
            Position = location;
            Result = result;
        }
        public Vertex(double[] location, double result, double h)
        {
            Position = location;
            Result = result;
            EyeHeight = h;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// **** You must have a constructor that takes 0 arguments for 
        /// **** both the IVertexConvHull and IFaceConvHull inherited
        /// **** classes! ******
        /// </summary>
        public Vertex()
        {
        }
        public Vertex(double x, double y)
        {
            Position = new double[] { x, y };
        }
        public Vertex(double x, double y, double z)
        {
            Position = new double[] { x, y, z };
        }
        public double Result { get; set; }
        public double EyeHeight { get; set; }
        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        public double[] Position { get; set; }
    }
}
