// MIT License

// Copyright (c) 2017 Mesh Consultants Inc

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrasshopperCs
{
    class Vector
    {
        private double[] coords;
        private double magnitude;

        public double this[int idx]
        {
            get { return coords[idx]; }
            set { coords[idx] = value; }
        }

        public int Rank
        {
            get { return coords.Length; }
        }

        public double Magnitude
        {
            get { return magnitude; }
            set { magnitude = value; }
        }

        public Vector()
            : this(3)
        {
        }

        public Vector(int count)
        {
            coords = new double[count];
            
            for (int i = 0; i < count; i++)
                coords[i] = 0.0d;

            magnitude = 0.0d;
        }

        public Vector(Vector v)
            : this(v.Rank)
        {
            for (int i = 0; i < v.Rank; i++)
                coords[i] = v[i];
            magnitude = v.Magnitude;
        }

        public static double DistanceBetween(Vector from, Vector to)
        {
            var res = 0.0d;

            if (from.Rank != to.Rank)
                return res;

            for (int i = 0; i < from.Rank; i++)
                res += Math.Pow((from[i] - to[i]), 2);
            res = Math.Sqrt(res);

            return res;
        }
        
        public static Vector operator *(Vector v, double d)
        {
            var outV = new Vector(v);

            for (int i = 0; i < v.Rank; i++)
                outV[i] *= d;
            
            outV.SetMagnitude();
            return outV;
        }

        public static Vector operator /(Vector v, double d)
        {
            var outV = new Vector(v);

            for (int i = 0; i < v.Rank; i++)
                outV[i] /= d;

            outV.SetMagnitude();

            return outV;
        }

        public static Vector operator +(Vector v, Vector d)
        {
            var outV = new Vector(v);

            for (int i = 0; i < v.Rank; i++)
                outV[i] += d[i];

            outV.SetMagnitude();

            return outV;
        }

        public void SetMagnitude()
        {
            magnitude = Vector.DistanceBetween(this, new Vector(this.Rank));
        }

        public override string ToString() 
        {
            var output = "[" + Rank + "] {\n";
            for (int i = 0; i < Rank; i++)
                output += "    " + coords[i] + ",\n";
            output += "}";

            return output;
        }
    }
}