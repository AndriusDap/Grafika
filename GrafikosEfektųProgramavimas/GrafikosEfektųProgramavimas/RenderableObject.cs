using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrafikosEfektųProgramavimas
{
    class RenderableObject : IRenderable
    {
        private Matrix ModelMatrix;
        public Model ObjectModel { get; set; }


        private Vector3 position;
        private float scale;
        private Boolean dirty;

        public float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                dirty = true;
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                dirty = true;
            }
        }

        public RenderableObject()
        {
            ModelMatrix = Matrix.Identity;
            ObjectModel = null;
            position = Vector3.Zero;
            scale = 1.0f;
            dirty = true;
        }

        public RenderableObject(Model model)
            : this()
        {
            ObjectModel = model;
        }

        private void UpdateMatrix()
        {
            if (dirty)
            {
                ModelMatrix = Matrix.CreateTranslation(position) * Matrix.CreateScale(scale);
                dirty = false;
            }
        }
        public void Update(GameTime t)
        {
            UpdateMatrix();
        }

        public void Render(Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition)
        {
            UpdateMatrix();
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
        Effect DefaultEffect;
        public void Render(Effect customShader, Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition)
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

        public void SetUpEffects(Action<Effect, ModelMesh> SetUpFunction)
        {
            foreach (ModelMesh mesh in ObjectModel.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    SetUpFunction(effect, mesh);
                }
            }
        }

    }
}
