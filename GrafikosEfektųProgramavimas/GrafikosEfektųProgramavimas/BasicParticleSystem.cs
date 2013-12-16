using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GrafikosEfektųProgramavimas
{
    class BasicParticleSystem
    {
        struct Particle
        {
            public bool Alive;
            public RenderableObject obj;
            public Vector3 Target;
            public Vector3 Source;
            public Double Date;
        }
        Particle[] particles;

        int first;
        int last;
        private static int MaxParticles = 500;

        Model model;
        Effect effect;

        private Random rng;

        public BasicParticleSystem(Model m)
        {
            model = m;
            particles = new Particle[MaxParticles];
            for (int i = 0; i < MaxParticles; i++)
            {
                particles[i] = new Particle();
                particles[i].Alive = false;
            }
            first = 0;
            last = 0;
            rng = new Random();
        }

        public void SpawnParticles(Vector3 startingPoint, Vector3 targetPoint, float spread, int count, GameTime time)
        {
            double startingTime = time.TotalGameTime.TotalMilliseconds;
            for (int i = 0; i < count; i++)
            {
                Particle current = new Particle();
                current.Alive = true;
                current.obj = new RenderableObject(model);
                current.obj.Scale = 1f;
                current.Date = startingTime;
                current.Source = startingPoint;
                current.Target = targetPoint + new Vector3((float)rng.NextDouble() * 2, (float)rng.NextDouble() * 2, (float)rng.NextDouble() * 2);
                particles[i % MaxParticles] = current;
            }
        }

        public void Update(GameTime time)
        {
            double now = time.TotalGameTime.TotalMilliseconds;
            double minDate = time.TotalGameTime.TotalMilliseconds - 5000;
            for (int i = 0; i < MaxParticles; i++)
            {
                if (particles[i].Alive)
                {
                    if (particles[i].Date < minDate)
                    {
                        particles[i].Alive = false;
                    }
                    else
                    {
                        particles[i].obj.Position = particles[i].Source;// +(particles[i].Target - particles[i].Source) * (float)(now - particles[i].Date) / 1000f; 
                    }
                }
            }
        }

        public void Render(GraphicsDevice device, Matrix World, Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            for (int i = 0; i < MaxParticles; i++)
            {
                if (particles[i].Alive)
                {
                    particles[i].obj.Render(View, Projection, World, CameraPosition);
                }
            }
        }
    }
}
