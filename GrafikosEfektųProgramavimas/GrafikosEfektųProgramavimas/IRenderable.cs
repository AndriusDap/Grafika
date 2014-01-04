using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrafikosEfektųProgramavimas
{
    interface IRenderable
    {
        void Update(GameTime time);
        void Render(Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition);
        void Render(Effect customShader, Matrix View, Matrix Projection, Matrix world, Vector3 CameraPosition);
        void SetUpEffects(Action<Effect, ModelMesh> SetUpFunction);
        Model ObjectModel { get; set; }
        Vector3 Position { get; set; }
    }
}
