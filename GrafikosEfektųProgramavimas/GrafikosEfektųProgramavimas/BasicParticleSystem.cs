using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GrafikosEfektųProgramavimas
{
    class BasicParticleSystem : IRenderable
    {
        static private Random rng = new Random();
        static private float AgeLimit = 500;
        class Particle
        {
            public bool Alive;
            public RenderableObject obj;
            public Vector3 Position;
            public Vector3 Speed;

            public Particle(Model m)
            {
                Alive = true;
                var RandomVector = new Vector3(
                    (float)(rng.NextDouble() - 0.5),
                    (float)(rng.NextDouble() - 0.5),
                    (float)(rng.NextDouble() - 0.5));
                Speed = Vector3.Normalize(RandomVector) / 10;
                Position = new Vector3(0, 0, 0);
                obj = new RenderableObject(m);
            }

            public void Update(GameTime time)
            {
                Position += Speed * (float)time.ElapsedGameTime.TotalMilliseconds;
                Speed.Y -= 0.005f;
            }
        }

        List<Particle> particles;

        private static int MaxParticles = 500;
        private static int BatchSize = 10;
        private float age;

        public Model ObjectModel { get; set; }
        public Vector3 Position { get; set; }
        private int cursor;
        public BasicParticleSystem(Model m)
        {
            ObjectModel = m;
            particles = new List<Particle>(MaxParticles);
            particles.AddRange(Enumerable.Repeat<Particle>(null, MaxParticles));
            cursor = 0;

        }


        private void AddParticles()
        {
            for (int i = 0; i < BatchSize; i++)
            {
                particles[i + cursor] = new Particle(ObjectModel);
            }
            cursor += BatchSize;
            cursor %= MaxParticles;
        }


        public void Update(GameTime time)
        {
            particles.ForEach(p =>
            {
                if (p != null)
                {
                    p.Update(time);
                }
            });
            AddParticles();
        }

        public void Render(Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition)
        {
            particles.ForEach(p =>
                {
                    if (p != null)
                    {
                        p.obj.Position = p.Position + Position;
                        p.obj.Render(View, Projection, world, CameraPosition);
                    }
                });
        }

        public void Render(Effect customShader, Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition)
        {
            if (age < AgeLimit)
            {
                particles.ForEach(p =>
                    {
                        if (p != null)
                        {
                            p.obj.Position = p.Position + Position;
                            p.obj.Render(customShader, View, Projection, world, CameraPosition);
                        }
                    });
            }
        }

        public void SetUpEffects(Action<Effect, ModelMesh> SetUpFunction)
        {
            if (age < AgeLimit)
            {
                particles.ForEach(p =>
                    {
                        if (p != null)
                        {
                            p.obj.SetUpEffects(SetUpFunction);
                        }
                    });
            }
        }
    }
}
