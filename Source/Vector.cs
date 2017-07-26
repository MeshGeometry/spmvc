//Copyright (c) 2011 Yolles Partnership Inc.

//This software is provided 'as-is', without any express or implied
//warranty. In no event will the authors be held liable for any damages
//arising from the use of this software.

//Permission is granted to anyone to use this software for any purpose,
//including commercial applications, and to alter it and redistribute it
//freely, subject to the following restrictions:

//   1. The origin of this software must not be misrepresented; you must not
//   claim that you wrote the original software. If you use this software
//   in a product, an acknowledgment in the product documentation would be
//   appreciated but is not required.

//   2. Altered source versions must be plainly marked as such, and must not be
//   misrepresented as being the original software.

//   3. This notice may not be removed or altered from any source
//   distribution.

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