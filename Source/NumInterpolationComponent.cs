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
using System.Drawing;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class NumInterpolationComponent : GH_Component
    {
        public NumInterpolationComponent()
            : base("Number Interpolation", "Num Interp", "Interpolates N-dimensional points based on surrounding M-dimensional vectors", "SPM", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{32BBDCC8-4494-4C5C-9824-7AC07654506D}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Interpolation1d; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_DoubleParam("Sample Points", "SP", "Tree of lists of doubles representing N-dimensional points. These must be parallel to the list of incoming Sample Vectors.", GH_ParamAccess.tree);
            pManager.Register_DoubleParam("Sample Vectors", "SV", "Tree of lists of doubles representing M-dimensional vectors. These vectors must be parallel to the list of incoming Sample Points.", GH_ParamAccess.tree);
            pManager.Register_DoubleParam("Test Points To Interpolate", "TP", "Tree of lists of doubles representing N-dimensional points, these points will be the reference points for each interpolation", GH_ParamAccess.tree);
            pManager.Register_DoubleParam("Radius", "R", "Radius from each test point to use in the interpolation", 0.0d);
            pManager.Register_DoubleParam("Interpolation Strength", "p", "1 for linear, n for higher strengths", 2.0d);
        }
 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Vectors", "V", "Tree of lists representing N-dimensional vectors");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var samplePoints = new List<Vector>();
            var sampleVectors = new List<Vector>();
            var testPoints = new List<Vector>();
            var radius = new GH_Number();
            var p = new GH_Number();

            var pdt = new GH_Structure<GH_Number>();
            var vdt = new GH_Structure<GH_Number>();
            var tpdt = new GH_Structure<GH_Number>();

            // gather and validate inputs
            if (DA.GetDataTree(0, out pdt) && pdt == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid sample point tree. Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetDataTree(1, out vdt) && vdt == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid vector tree. Operation canceled.");
                return;
            }

            if (pdt.Branches.Count != vdt.Branches.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Point and Vector trees must be parallel (same count). Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetDataTree(2, out tpdt) && tpdt == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid test point tree. Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetData(3, ref radius) && radius == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid radius value. Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetData(4, ref p) && p == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid strength value. Operation canceled.");
                return;
            }

            // collect input Vectors
            samplePoints = GetVectorsFromTree(pdt);
            sampleVectors = GetVectorsFromTree(vdt);
            testPoints = GetVectorsFromTree(tpdt);

            // if r is 0 calculate a sane default
            double r = radius.Value;
            if (r == 0.0d)
                r = GetDefaultRadius(samplePoints);

            // get interpolated vectors
            var interpolated = new List<Vector>();
            foreach (var tp in testPoints)
            {
                var withinRadius = GetAllSampleIndicesWithinRadius(tp, samplePoints, r).ToList();
                interpolated.Add(GetInterpolatedVector(tp, samplePoints, sampleVectors, withinRadius, r, p.Value));
            }

            // set interpolated vectors as output
            var outputTree = new GH_Structure<GH_Number>();

            for (int i = 0; i < interpolated.Count; i++)
            {
                var v = interpolated[i];
                var points = new List<GH_Number>();
                
                for (int j = 0; j < v.Rank; j++)
                    points.Add(new GH_Number(v[j]));

                outputTree.AppendRange(points, new GH_Path(i));
            }
            
            DA.SetDataTree(0, outputTree);
        }

        private double GetDefaultRadius(List<Vector> samplePoints)
        {
            var output = 0.0d;
            var avgPt = new Vector(samplePoints[0].Rank);

            // calculate average point
            foreach (var p in samplePoints)
                avgPt += p;
            avgPt /= samplePoints.Count;

            // calculate max distance from average point to sample points
            foreach (var p in samplePoints)
            {
                var d = Vector.DistanceBetween(avgPt, p);
                if (d > output)
                    output = d;
            }

            return output;
        }

        private Vector GetInterpolatedVector(Vector tp, List<Vector> samplePoints, List<Vector> sampleVectors, List<int> sampleIndices, double radius, double p)
        {
            var output = new Vector(sampleVectors[0].Rank);

            double sumWeight = 0.0d;
            var weightedVecs = new Vector[sampleIndices.Count];

            for (int i = 0; i < sampleIndices.Count; i++)
            {
                int idx = sampleIndices[i];
                double d = Vector.DistanceBetween(tp, samplePoints[idx]);
                
                if (d == 0)
                    return sampleVectors[idx];

                var w = Math.Pow((radius - d) / (radius * d), p);

                weightedVecs[i] = sampleVectors[idx] * w;
                sumWeight += w;
            }

            foreach (var v in weightedVecs)
                output += v / sumWeight;

            return output;
        }

        private IEnumerable<int> GetAllSampleIndicesWithinRadius(Vector tp, List<Vector> samplePoints, double radius)
        {
            for (int i = 0; i < samplePoints.Count; i++)
                if (Vector.DistanceBetween(tp, samplePoints[i]) < radius)
                    yield return i;
        }

        private List<Vector> GetVectorsFromTree(GH_Structure<GH_Number> tree)
        {
            var output = new List<Vector>();

            for (int i = 0; i < tree.Branches.Count; i++)
            {
                var branchItems = tree[i].Count;
                output.Add(new Vector(branchItems));

                for (int j = 0; j < branchItems; j++)
                {
                    output[i][j] = (tree[i][j] as GH_Number).Value;
                    output[i].SetMagnitude();
                }
            }

            return output;
        }
    }
}
