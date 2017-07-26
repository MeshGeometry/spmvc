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

using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class Particle
    {
        public Basis Start { get; set; }
        public Basis Current { get; set; }
        public int LifeTime { get; set; }
        public int LifeLeft { get; set; }
        
        public Particle()
            : this (new Basis(), 0)
        {
        }

        public Particle(Particle pIn)
        {
            this.Start = new Basis(new GH_Point(pIn.Start.Point), new GH_Vector(pIn.Start.Vector));
            LifeTime = pIn.LifeTime;
            Reset();
        }

        public Particle(Basis start, int lifetime)
        {
            Start = start;
            Current = Start;
            LifeTime = lifetime;
            LifeLeft = LifeTime;
        }

        public Particle(GH_Point startP, GH_Vector startV, int lifetime)
            : this(new Basis(startP, startV), lifetime)
        {
        }

        public Particle(GH_Point startP, int lifetime)
            : this(new Basis(startP, new GH_Vector()), lifetime)
        {
        }

        public void Reset()
        {
            Current = new Basis(new GH_Point(Start.Point), new GH_Vector(Start.Vector));
            LifeLeft = LifeTime;
        }
    }

    public class Emitter
    {
        // stored as separate lists instead of a particle object for higher performance during the
        // dynamic application step (no need to gather lists of particle points and vectors to input)
        private List<Particle> particles;
        private List<Particle> startParticles;

        private int startLife; // step life each particle starts with
        private bool respawn;  // if true, respawn the particle when it runs out of life time
        
        private int emitDeviance;
        private int emitTime;
        private List<int> emitCountDown;

        private Random rnd;

        #region Properties

        public List<Particle> Particles
        {
            get { return particles; }
        }

        public List<GH_Point> Points
        {
            get
            {
                var ptList = new List<GH_Point>();               
                particles.ForEach(p => ptList.Add(p.Current.Point));
                return ptList;
            }
        }

        public List<GH_Vector> Vectors
        {
            get
            {
                var vecList = new List<GH_Vector>();
                particles.ForEach(p => vecList.Add(p.Current.Vector));
                return vecList;
            }
        }

        public int StartLife
        {
            get { return startLife; }
            set { startLife = value; }
        }

        public bool Respawn
        {
            get { return respawn; }
            set { respawn = value; }
        }

        #endregion

        #region Constructors

        protected Emitter(int startLife, DynamicSettings settings)
        {
            this.startLife = startLife;
            
            respawn = settings.respawn;
            emitTime = settings.newSpawn;
            emitDeviance = settings.rndSpawn;
            emitCountDown = new List<int>(); // parallel to startParticles

            particles = new List<Particle>();
            startParticles = new List<Particle>();

            rnd = new Random();
        }

        public Emitter(List<Basis> bases, int startLife, DynamicSettings settings)
            : this(startLife, settings)
        {
            bases.ForEach(b => startParticles.Add(new Particle(b, startLife)));
            Begin();
        }

        public Emitter(List<GH_Point> points, List<GH_Vector> vectors, int startLife, DynamicSettings settings)
            : this(startLife, settings)
        {
            if (points.Count != vectors.Count)
                throw new ArgumentException("Points and Vectors list must be parallel (same length)!");

            for (int i = 0; i < points.Count; i++)
                startParticles.Add(new Particle(points[i], vectors[i], startLife));

            Begin();
        }

        public Emitter(List<GH_Point> points, int startLife, DynamicSettings settings)
            : this(startLife, settings)
        {
            points.ForEach(p => startParticles.Add(new Particle(p, startLife)));
            Begin();
        }

        #endregion

        private void Begin()
        {
            foreach (var p in startParticles)
                emitCountDown.Add(GetNextEmitTime());

            if (emitTime == 0 || emitDeviance == 0)
                Emit();
        }

        private int GetNextEmitTime()
        {
            if (emitTime != 0 && emitDeviance != 0)
                return emitTime + rnd.Next(-emitDeviance, emitDeviance);
            else
                return emitTime;
        }

        public void Update(List<IDynamic> dynamics, List<GH_Point> points, List<GH_Vector> vectors, DynamicSettings settings)
        {
            UpdateWithDynamics(dynamics, settings);
            UpdateLifeTimes(dynamics);
            UpdateEmitCounter();
            UpdateIntegration(points, vectors, settings);
            CheckHaltingConditions(settings);
        }

        private void Emit()
        {
            startParticles.ForEach(p => particles.Add(new Particle(p)));
        }

        private void Emit(Particle p)
        {
            particles.Add(new Particle(p));
        }

        private void Reset()
        {
            particles.ForEach(p => p.Reset());
        }

        private void UpdateEmitCounter()
        {
            for (int i = 0; i < startParticles.Count; i++)
            {
                if (emitTime > 0)
                {
                    emitCountDown[i]--;
                    if (emitCountDown[i] < 0)
                    {
                        Emit(startParticles[i]);
                        emitCountDown[i] = GetNextEmitTime();
                    }
                }
            }
        }

        private void UpdateLifeTimes(List<IDynamic> dynamics)
        {
            // we have to keep track of particles to remove,
            // and their idx's for either clearing (resetting) or
            // deleting -- the accel vectors need to be key in sync
            var toRemove = new List<Particle>();
            var toClear = new List<int>();
            var toRemoveAV = new List<int>();

            var idx = 0;
            foreach (var p in particles)
            {
                p.LifeLeft--;

                if (p.LifeLeft < 0)
                {
                    if (respawn)
                        p.Reset();
                    else
                    {
                        toRemove.Add(p);
                        toRemoveAV.Add(idx);
                    }

                    toClear.Add(idx);
                }
                idx++;
            }

            // clear accel vectors from dynamics and remove others if necessary
            ClearAcceleration(dynamics, toClear, toRemoveAV);

            foreach (var p in toRemove)
                particles.Remove(p);
        }

        private void ClearAcceleration(List<IDynamic> dynamics, List<int> idxToClear, List<int> idxToRemove)
        {
            string accelKey = "accelerationVectors";
            foreach (var d in dynamics)
            {
                if (d.Accelerated)
                {
                    if (d.Param.Contains(accelKey))
                    {
                        // clear accel vector at idx's
                        var a = d.Param[accelKey] as List<GH_Vector>;
                        d.Param.Remove(accelKey);

                        // clear some vectors
                        foreach (var i in idxToClear)
                            a[i] = new GH_Vector(Vector3d.Unset);

                        // remove others for resync
                        for (int i = idxToRemove.Count - 1; i >= 0; i--)
                            a.RemoveAt(idxToRemove[i]);

                        d.Param[accelKey] = a;
                    }
                }
            }
        }

        private void UpdateWithDynamics(List<IDynamic> dynamics, DynamicSettings settings)
        {
            var points = new List<GH_Point>(particles.Count);
            var vectors = new List<GH_Vector>(particles.Count);

            for (int i = 0; i < particles.Count; i++)
            {
                points.Add(particles[i].Current.Point);
                vectors.Add(new GH_Vector());
            }

            foreach (var d in dynamics)
            {
                // we need a new list of vectors each way through, which we add seperately to an end result
                // otherwise acceleration can apply across dynamics in unintended ways
                var tempVectors = new List<GH_Vector>(particles.Count);
                for (int i = 0; i < particles.Count; i++)
                    tempVectors.Add(new GH_Vector());

                // post processes use the cumulative vector list
                // pre processes use the temporary empty vector list
                if (d.PostProcess)
                {
                    // vectors is modified inline so we don't need to update as below
                    Algos.ProcessDynamics(d, points, vectors, settings.surface);
                }
                else
                {
                    Algos.ProcessDynamics(d, points, tempVectors, settings.surface);

                    for (int i = 0; i < particles.Count; i++)
                        vectors[i].Value += tempVectors[i].Value;
                }
            }

            Algos.RealignAccelerationVectors(dynamics, vectors);

            for (int i = 0; i < points.Count; i++)
            {
                points[i].Value += vectors[i].Value;
                
                // update particles from resultant p/v's above
                particles[i].Current.Point = points[i];
                particles[i].Current.Vector = vectors[i];
            }
        }

        private void UpdateIntegration(List<GH_Point> points, List<GH_Vector> vectors, DynamicSettings settings)
        {
            // only integrate if we have a vector field to work with
            if (points.Count > 0)
            {
                // integration
                var bases = new List<Basis>();
                for (int i = 0; i < points.Count; i++)
                    bases.Add(new Basis(points[i], vectors[i]));

                var ptSampling = (points.Count != 0);

                var staticSettings = ConvertDynSettingsToStatic(settings);

                for (int i = 0; i < particles.Count; i++)
                {
                    var p = particles[i];

                    var traveller = p.Current.Point;
                    var startBasis = p.Start;
                    var vecLast = p.Current.Vector;

                    var outBasis = new Basis();

                    if (points.Count != 0 &&
                        !Algos.SampleForNextPoint(bases, traveller.Value, startBasis, vecLast, staticSettings, out outBasis))
                        break;

                    p.Current.Point = outBasis.Point;
                    p.Current.Vector = outBasis.Vector;
                }
            }
        }

        private StaticSettings ConvertDynSettingsToStatic(DynamicSettings dyn)
        {
            var sta = new StaticSettings();

            // 1:1 functionality
            sta.avgRadius = dyn.avgRadius;
            sta.bounds = dyn.bounds;
            sta.lineCont = dyn.lineCont;
            sta.snapAngle = dyn.snapAngle;
            sta.snapTol = dyn.snapTol;
            sta.surface = dyn.surface;

            // not used or used differently from regular static usage
            sta.stop = false; // never use the regular stop checks in dynamic
            sta.tolerance = double.MaxValue;

            return sta;
        }

        private void CheckHaltingConditions(DynamicSettings settings)
        {
            var staticSettings = ConvertDynSettingsToStatic(settings);
            var toRemove = new List<Particle>();

            for (int i = 0; i < particles.Count; i++)
            {
                var p = particles[i];

                var add = false;
                var outBasis = new Basis(p.Current.Point, p.Current.Vector);

                if (!Algos.CheckHaltingConditions(p.Current.Point, p.Start, outBasis, p.Current.Vector.Value, out add, staticSettings) || add == false)
                    toRemove.Add(p);
                else
                    p.Current.Point = outBasis.Point;
            }

            foreach (var remove in toRemove)
                particles.Remove(remove);
        }
    }
}
