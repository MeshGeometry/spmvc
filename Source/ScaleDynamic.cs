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
using System.Windows.Forms;

using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    class ScaleDynamic : IDynamic
    {
        public bool PostProcess { get { return true; } }
        public Param Param { get; set; }
        public bool Accelerated { get { return false; } }

        public ScaleDynamic()
        {
            Param = new Param();
        }

        public void Process(List<GH_Point> points, List<GH_Vector> vectors, GH_Surface surface)
        {
            var sf = ((GH_Number)Param["S"]).Value;
            var a = ((GH_Number)Param["F"]).Value;

            foreach (var v in vectors)
            {
                var x = v.Value.Length;

                if (x > a)
                {
                    x = a + Math.Log(x + 1 - a);
                    v.Value *= x / v.Value.Length;
                }

                v.Value *= sf;
            }
        }
    }
}