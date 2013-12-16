using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GrafikosEfektųProgramavimas
{
    class ParticleSystem
    {
        private static int MaxlastParticle = 5000;
        private int lastParticle;
        private int firstParticle;
        private VertexBuffer singleParticle;
        private VertexBuffer startingPoints;
        private VertexBuffer startingTimes;
        private VertexBuffer targetPoints;
        private Effect shader;
        private float[] times;
        private float CurrentTime;
        private Random rng;
        private IndexBuffer indexBuffer;
        public ParticleSystem(GraphicsDevice device)
        {
            lastParticle = 0;
            firstParticle = 0;

            VertexDeclaration lineVertex = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Position, 1));
            VertexDeclaration Point = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0));
            VertexDeclaration Time = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0));

            indexBuffer = new IndexBuffer(device, typeof(int), 2, BufferUsage.WriteOnly);
            int[] indices = {0, 1};
            indexBuffer.SetData(indices);

            singleParticle = new VertexBuffer(device, lineVertex, 2, BufferUsage.WriteOnly);
            Vector3[] line = {Vector3.Zero, new Vector3(1, 1, 1)};
            singleParticle.SetData(line);

            startingPoints = new DynamicVertexBuffer(device, Point, MaxlastParticle, BufferUsage.WriteOnly);
            targetPoints = new DynamicVertexBuffer(device, Point, MaxlastParticle, BufferUsage.WriteOnly);
            startingTimes = new DynamicVertexBuffer(device, Time, MaxlastParticle, BufferUsage.WriteOnly);

            times = new float[MaxlastParticle];
            rng = new Random();
        }

        public void SpawnParticles(Vector3 startingPoint, Vector3 targetPoint, float spread, int count, GameTime time)
        {
            float startingTime = (float) time.TotalGameTime.TotalMilliseconds;
            
            Vector3[] spawnerLocation = new Vector3[count];
            Vector3[] targets = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                spawnerLocation[i] = startingPoint;
                targets[i] = targetPoint + new Vector3((float)rng.NextDouble() * 2, (float)rng.NextDouble() * 2, (float)rng.NextDouble() * 2);
            }

            if (lastParticle + count > MaxlastParticle)
            {
                lastParticle = 0;
                firstParticle = 0;
            }

            for (int i = lastParticle; i < lastParticle + count; i++)
            {
                times[i] = startingTime;
            }
            startingPoints.SetData<Vector3>(lastParticle * 12, spawnerLocation, 0, count, 12);
            targetPoints.SetData<Vector3>(lastParticle * 12, targets, 0, count, 12);
            startingTimes.SetData<float>(lastParticle * 4, times, lastParticle, count, 4);
            lastParticle += count;
        }

        public void Update(GameTime time)
        {
            float minBirthDate = (float) (time.TotalGameTime.TotalMilliseconds - 500000); // Living longer than 5s is prohibited
            if (lastParticle <= firstParticle)
            {
                lastParticle = 0;
                firstParticle = 0;
            }
            else
            {
                for (int i = firstParticle; i < lastParticle; i++)
                {
                    if (times[i] < minBirthDate)
                    {
                        firstParticle = i;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            CurrentTime = (float)(time.TotalGameTime.TotalMilliseconds);
        }

        public void Render(GraphicsDevice device, Matrix World, Matrix View, Matrix Projection)
        {
            if (lastParticle - firstParticle == 0)
            {
                return;
            }
            Matrix MVP = Matrix.Multiply(Matrix.Multiply(World, View), Projection);
            shader.Parameters["MVP"].SetValue(MVP);
            shader.Parameters["CurrentTime"].SetValue(CurrentTime);
            
            device.SetVertexBuffers(
                new VertexBufferBinding(singleParticle, 0, 0),
               new VertexBufferBinding(startingPoints, firstParticle, 200),
               new VertexBufferBinding(targetPoints, firstParticle, 200),
               new VertexBufferBinding(startingTimes, firstParticle, 200));
            device.Indices = indexBuffer;
            
            shader.CurrentTechnique.Passes[0].Apply();
            
            device.DrawInstancedPrimitives(PrimitiveType.LineList, 0, 0, (lastParticle - firstParticle) * 2, firstParticle, lastParticle - firstParticle, 50);
            //device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, (lastParticle - firstParticle) * 2, firstParticle, lastParticle - firstParticle);
            device.SetVertexBuffers(null);
        }

        public void Load(ContentManager Content)
        {
            shader = Content.Load<Effect>("ParticleShader");
        }
    }
}
