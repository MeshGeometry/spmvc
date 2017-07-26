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
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public struct Algos
    {
        private static int GetNearestSamplePointIndex(List<Basis> samples, ref Point3d point, double tolerance)
        {
            int sampleIndex = 0;
            double closestDistance = double.MaxValue;

            for (int i = 0; i < samples.Count; i++)
            {
                double d = samples[i].Point.Value.DistanceTo(point);

                if (d < closestDistance)
                {
                    closestDistance = d;
                    sampleIndex = i;
                }
            }

            if (closestDistance > tolerance)
                return -1;

            return sampleIndex;
        }

        private static List<int> GetNearestVectorsWithin(List<Basis> bases, ref Point3d point, double tolerance, double radius)
        {
            var vecs = new List<int>();
            bool isTolerant = false;

            for (int i = 0; i < bases.Count; i++)
            {
                double d = bases[i].Point.Value.DistanceTo(point);
                
                if (d < radius)
                    vecs.Add(i);

                if (d < tolerance)
                    isTolerant = true;
            }

            if (isTolerant)
                return vecs;
            else
                return null;
        }

        private static GH_Vector GetNextInterpolatedVector(List<Basis> bases, List<int> vectorIdxs, GH_Vector lastVec, bool lineCont, int tensorDir, ref Point3d point, double radius)
        {
            var next = new GH_Vector();
            int n = vectorIdxs.Count;

            double sumWeight = 0.0d;
            var weightedVecs = new Vector3d[n];

            for (int i = 0; i < n; i++)
            {
                int idx = vectorIdxs[i];

                double d = bases[idx].Point.Value.DistanceTo(point);
                
                // if d is 0 we are directly on top of a vector, its weight will overrule
                // any other vectors in the area, use it exclusively
                if (d == 0)
                    return bases[idx].Vector;

                var w = Math.Pow((radius - d) / (radius * d), 2);
                
                var vec = bases[idx].Vector.Value;
                
                if (tensorDir != -1)
                    vec = bases[idx].Vectors[tensorDir].Value;

                if (lineCont && !lastVec.Value.IsZero)
                    vec = bases[idx].GetBestContinuousVector(lastVec.Value);
                
                weightedVecs[i] = vec * w;
                sumWeight += w;
            }

            foreach (var v in weightedVecs)
                next.Value += v / sumWeight;

            return next;
        }

        private static GH_Vector GetNextVector(List<Basis> bases, GH_Vector lastVec, bool lineCont, int tensorDir, ref Point3d point, double tolerance, double radius)
        {
            GH_Vector next = null;

            // 0 radius means don't interpolate -- use nearest point
            if (radius == 0.0d)
            {
                var idx = GetNearestSamplePointIndex(bases, ref point, tolerance);
                
                if (idx == -1)
                    return null;

                if (lineCont && !lastVec.Value.IsZero)
                {
                    next = new GH_Vector(bases[idx].GetBestContinuousVector(lastVec.Value));
                }
                else
                {
                    if (tensorDir != -1)
                        next = bases[idx].Vectors[tensorDir];
                    else
                        next = bases[idx].Vector;
                }
            }
            else
            {
                // use interpolation
                var sampleIdxs = GetNearestVectorsWithin(bases, ref point, tolerance, radius);

                // if samples is null, all vectors gathered were out of the tolerance range
                if (sampleIdxs != null)
                    next = GetNextInterpolatedVector(bases, sampleIdxs, lastVec, lineCont, tensorDir, ref point, radius);
            }

            return next;
        }

        private static GH_Vector GetInterpolatedVector(List<Basis> bases, GH_Vector lastVec, bool lineCont, int tensorDir, ref Point3d point, double tolerance, double radius)
        {
            var k1 = GetNextVector(bases, lastVec, lineCont, tensorDir, ref point, tolerance, radius);
            if (k1 == null)
                return null;

            var tmpTrvlr = point + (k1.Value / 2);
            var k2 = GetNextVector(bases, lastVec, lineCont, tensorDir, ref tmpTrvlr, tolerance, radius);
            if (k2 == null)
                return null;

            tmpTrvlr = point + (k2.Value / 2);
            var k3 = GetNextVector(bases, lastVec, lineCont, tensorDir, ref tmpTrvlr, tolerance, radius);
            if (k3 == null)
                return null;

            tmpTrvlr = point + k3.Value;
            var k4 = GetNextVector(bases, lastVec, lineCont, tensorDir, ref tmpTrvlr, tolerance, radius);
            if (k4 == null)
                return null;

            // kv = ((1/6) * (k1 + 2k2 + 2k3 + k4))
            return new GH_Vector((1d / 6d) * (k1.Value + ((2 * k2.Value) + (2 * k3.Value) + k4.Value)));
        }

        public static bool SampleForNextPoint(List<Basis> bases, Point3d point, Basis start, 
                                              GH_Vector lastVec, StaticSettings settings, 
                                              out Basis outBasis)
        {
            var tolerance = settings.tolerance;
            var radius = settings.avgRadius;
            var lineCont = settings.lineCont;
            var tensorDir = settings.tensorDir;

            outBasis = new Basis();

            GH_Vector ghkv = GetInterpolatedVector(bases, lastVec, lineCont, tensorDir, ref point, tolerance, radius);
            if (ghkv == null)
                return false;

            var kv = ghkv.Value;

            // po = pt + kv
            outBasis.Vector = new GH_Vector(kv);
            outBasis.Point = new GH_Point(point + kv);

            return true;
        }

        private static bool CheckStopped(Vector3d vector)
        {
            // zero vector check
            if (vector.Length < 0.001d)
                return true;
            return false;
        }

        public static bool CheckIfOffSurface(Basis point, GH_Surface surface)
        {
            // surface constraint check
            if (surface.IsValid)
            {
                double u, v;
                surface.Face.ClosestPoint(point.Point.Value, out u, out v);

                point.Point = new GH_Point(surface.Face.PointAt(u, v));

                if (surface.Face.IsPointOnFace(u, v) == PointFaceRelation.Boundary)
                    return true;
            }

            return false;
        }

        private static bool CheckInsideBounds(Basis outBasis, GH_Point traveller, GH_Brep bounds, ref bool add)
        {
            // check if ptOut is inside bounds, if they are given
            if (bounds.IsValid && !bounds.Value.IsPointInside(outBasis.Point.Value, 0.001d, false))
            {
                Curve curve;
                Curve[] overlaps;
                Point3d[] intersections;

                curve = Curve.CreateControlPointCurve(new Point3d[] { traveller.Value, outBasis.Point.Value });
                Rhino.Geometry.Intersect.Intersection.CurveBrep(curve, bounds.Value, 0.001d, out overlaps, out intersections);

                // if we're outside the bounds, but there is an intersection, add that intersection point to the results
                // if there are no intersections it's because we're starting from outside the bounds, don't add these
                if (intersections.Length > 0)
                {
                    outBasis.Point.Value = intersections[0];
                    add = true;
                }

                return true;
            }

            return false;
        }

        private static bool CheckIfOrbitting(Basis outBasis, Basis start, Vector3d vector, double snapTol, double snapAngle, ref bool add)
        {
            // orbit check, if tolerances have been set up
            if (snapTol != 0.0d && snapAngle != 0.0d & !start.Vector.Value.IsZero)
            {
                if (start.Point.Value.DistanceTo(outBasis.Point.Value) < snapTol)
                {
                    var va = Vector3d.VectorAngle(vector, start.Vector.Value);

                    if (va < snapAngle)
                    {
                        outBasis.Point = start.Point;
                        add = true;
                        return false;
                    }
                }
            }
            
            return false;
        }

        public static bool CheckHaltingConditions(GH_Point traveller, Basis start, Basis outBasis, Vector3d previous, out bool add, StaticSettings settings)
        {
            var stop = settings.stop;
            var surface = settings.surface;
            var bounds = settings.bounds;
            var snapTol = settings.snapTol;
            var snapAngle = settings.snapAngle;

            add = false;

            if (stop && !previous.IsZero && CheckStopped(previous))
                return false;

            if (CheckIfOffSurface(outBasis, surface))
                return false;

            if (CheckInsideBounds(outBasis, traveller, bounds, ref add))
                return false;

            if (CheckIfOrbitting(outBasis, start, previous, snapTol, snapAngle, ref add))
                return false;

            add = true;
            return true;
        }

        public static void ProcessDynamics(IDynamic dyn, List<GH_Point> points, List<GH_Vector> vectors, GH_Surface surface)
        {
            dyn.Process(points, vectors, surface);

            if (dyn.Accelerated)
            {
                // account for acceleration, if necessary
                var output = new List<GH_Vector>();
                for (int i = 0; i < vectors.Count; i++)
                    output.Add(new GH_Vector());

                if (dyn.Param.Contains("accelerationVectors"))
                {
                    var v0 = dyn.Param["accelerationVectors"] as List<GH_Vector>;
                    
                    double drag = 1.0d;                    
                    if (dyn.Param.Contains("Dg"))
                        drag = (dyn.Param["Dg"] as GH_Number).Value;

                    // if vectors have been added (say via new particles) we need to 
                    // add the difference in new vectors (unset)
                    if (v0.Count < vectors.Count)
                    {
                        for (int i = vectors.Count - v0.Count; i >= 0; i--)
                            v0.Add(new GH_Vector(Vector3d.Unset));
                    }

                    for (int i = 0; i < vectors.Count; i++)
                    {
                        // if v0 is unset its because we want to reset this accel vector externally
                        if (v0[i].Value == Vector3d.Unset)
                            output[i] = new GH_Vector();
                        else
                        {
                            output[i] = new GH_Vector(v0[i].Value + (vectors[i].Value * drag));
                            vectors[i].Value = v0[i].Value + ((vectors[i].Value * drag) / 2);
                        }
                    }
                }

                dyn.Param["accelerationVectors"] = output;
            }
        }

        public static void ClearDynamics(List<IDynamic> dynamics)
        {
            dynamics.ForEach(d => d.Param.Remove("accelerationVectors"));
        }

        public static void SortDynamicsByPriority(List<IDynamic> dynamics)
        {
            var sorted = dynamics.Where(d => d.PostProcess == false).ToList();
            sorted.AddRange(dynamics.Where(d => d.PostProcess == true));
            dynamics = sorted;
        }

        public static GH_Point GetPointModifiedByDynamics(GH_Point traveller, Basis outBasis, List<IDynamic> dynamics, StaticSettings spm_settings)
        {
            var output = new GH_Point();

            foreach (var d in dynamics)
            {
                var p = new List<GH_Point>();
                p.Add(traveller);

                var v = new List<GH_Vector>();

                if (d.PostProcess)
                    v.Add(outBasis.Vector);
                else
                    v.Add(new GH_Vector());

                ProcessDynamics(d, p, v, spm_settings.surface);

                if (d.PostProcess)
                    outBasis.Vector.Value = v[0].Value;
                else
                    outBasis.Vector.Value += v[0].Value;
            }

            // realign acceleration if necessary
            var outVec = new List<GH_Vector>() { outBasis.Vector };
            Algos.RealignAccelerationVectors(dynamics, outVec);
            outBasis.Vector = outVec[0];

            // compute our resultant point via the traveller + resultant vectors
            var res = traveller.Value + (outBasis.Vector.Value);
            return new GH_Point(res);
        }

        public static void RealignAccelerationVectors(List<IDynamic> dynamics, List<GH_Vector> vectors)
        {
            var realignWhich = new List<int>();
            var accelList = new List<GH_Vector>();
            double bounce = 1.0d;

            foreach (var d in dynamics)
            {
                if (d.Param.Contains("alignAccVectors"))
                {
                    realignWhich = d.Param["alignAccVectors"] as List<int>;
                    bounce = (d.Param["R"] as GH_Number).Value;
                }

                if (d.Accelerated)
                    accelList = d.Param["accelerationVectors"] as List<GH_Vector>;
            }

            if (accelList.Count > 0 && realignWhich.Count > 0)
            {
                for (int i = 0; i < realignWhich.Count; i++)
                {
                    var idx = realignWhich[i];
                    accelList[idx].Value = (accelList[idx].Value.Length / vectors[idx].Value.Length) * vectors[idx].Value;
                    accelList[idx].Value *= bounce;
                }
            }
        }

        public static Vector3d CartesianToSpherical(Vector3d cartesian)
        {
            var spherical = new Vector3d();

            spherical.X = Math.Sqrt(Math.Pow(cartesian.X, 2) + Math.Pow(cartesian.Y, 2) + Math.Pow(cartesian.Z, 2));
            spherical.Y = Math.Acos(cartesian.Z / spherical.X);
            spherical.Z = Math.Atan(cartesian.Y / cartesian.X);

            return spherical;
        }

        public static bool IsWoundPast(Vector3d v1, Vector3d v2, double bounds, ref double xy, ref double yz, ref double xz)
        {
            var tv1 = new Vector3d(v1.X, v1.Y, 0);
            var tv2 = new Vector3d(v2.X, v2.Y, 0);
            var tvc = Vector3d.CrossProduct(tv2, tv1);

            xy += Vector3d.VectorAngle(tv1, tv2) * Math.Sign(tvc.Z);

            tv1 = new Vector3d(0, v1.Y, v1.Z);
            tv2 = new Vector3d(0, v2.Y, v2.Z);
            tvc = Vector3d.CrossProduct(tv2, tv1);

            yz += Vector3d.VectorAngle(tv1, tv2) * Math.Sign(tvc.X);

            tv1 = new Vector3d(v1.X, 0, v1.Z);
            tv2 = new Vector3d(v2.X, 0, v2.Z);
            tvc = Vector3d.CrossProduct(tv2, tv1);

            xz += Vector3d.VectorAngle(tv1, tv2) * Math.Sign(tvc.Y);
            
            return (xy > bounds || xy < -bounds ||
                    yz > bounds || yz < -bounds ||
                    xz > bounds || xz < -bounds);
        }

        public static double DotProduct(Vector3d a, Vector3d b)
        {
            return ((a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z));
        }
    }
}