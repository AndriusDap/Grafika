using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GrafikosEfektųProgramavimas
{
    class Billboard : RenderableObject
    {

        public Billboard(Model m) : base(m)
        {            
        }

        public void UpdateMatrix()
        {
            // matrix will be recalculated on rendering
        }

        Effect DefaultEffect;
        public override void Render(Effect customShader, Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition)
        {
            foreach (ModelMesh mesh in ObjectModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    DefaultEffect = part.Effect;
                    part.Effect = customShader;
                }
            }

            Render(View, Projection, world, CameraPosition);

            foreach (ModelMesh mesh in ObjectModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = DefaultEffect;
                }
            }
        }

        public override void Render(Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition)
        {
            float angle = (float) Math.Atan2(- Position.X + CameraPosition.X, - Position.Z + CameraPosition.Z);
            Matrix ModelMatrix = Matrix.CreateRotationY(angle);
            ModelMatrix *= Matrix.CreateTranslation(Position);
            foreach (ModelMesh mesh in ObjectModel.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {

                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["Model"].SetValue(ModelMatrix);
                    effect.Parameters["View"].SetValue(View);
                    effect.Parameters["Projection"].SetValue(Projection);
                    effect.Parameters["CameraPosition"].SetValue(CameraPosition);
                }
                mesh.Draw();
            }
        }
    }
}
