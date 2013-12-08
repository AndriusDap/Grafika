using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GrafikosEfektųProgramavimas
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Vector3 cameraPosition;
        Vector3 lookAt;
        Matrix projection;
        List<RenderableObject> models;
        Dictionary<String, Texture2D> textures;
        Dictionary<String, Texture2D> specularTextures;
        Dictionary<String, Texture2D> normalTextures;
        Dictionary<String, Texture2D> lightMapTextures;
        Model skybox;
        float aspectRatio;    
        
        RasterizerState invertedCulling;
        RasterizerState normalCulling;

        #region initialization
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1600;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            models = new List<RenderableObject>();
            textures = new Dictionary<String, Texture2D>();
            normalTextures = new Dictionary<String, Texture2D>();
                  
            invertedCulling = new RasterizerState();
            invertedCulling.CullMode = CullMode.CullClockwiseFace;
            
            normalCulling = new RasterizerState();
            normalCulling.CullMode = CullMode.CullCounterClockwiseFace;
        }

        public static String ParseMeshName(String name)
        {
            return name.Split('_')[0];
        }
       
        protected override void Initialize()
        {
            base.Initialize();
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            cameraPosition = new Vector3(-200, -50, -500);
            lookAt = cameraPosition + Vector3.UnitX;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.10f, 100000.0f);
           
        }

        protected override void LoadContent()
        {
           
            var terrain = Content.Load<Model>("large_heightmap");
            models.Add(new RenderableObject(terrain));
            skybox = Content.Load<Model>("skybox2");

            List<String> textureNames = new List<String>();
            foreach (var m in models)
            {
                foreach (var mesh in m.ObjectModel.Meshes)
                {
                    if (mesh.Name.Contains('_'))
                    {
                        continue;
                    }
                    if (!textureNames.Contains(mesh.Name))
                    {
                        textureNames.Add(mesh.Name);
                    }
                }
            }

            foreach (var textureName in textureNames)
            {
               textures.Add(textureName, Content.Load<Texture2D>(textureName+"_DIFF"));
               normalTextures.Add(textureName, Content.Load<Texture2D>(textureName + "_NORM"));
            }

            // Set up shaders for created models:
            foreach (var model in models)
            {
                model.SetUpEffects((effect, mesh) =>
                    {
                        effect.Parameters["DiffuseIntensity"].SetValue(0.50f);

                        String parsedMeshName = ParseMeshName(mesh.Name);
                        effect.Parameters["DiffuseTexture"].SetValue(textures[parsedMeshName]);
                        effect.Parameters["NormalMap"].SetValue(normalTextures[parsedMeshName]);

                        // Key light.
                        effect.Parameters["Light0Direction"].SetValue(new Vector3(-0.5265408f, -0.5735765f, -0.6275069f));
                        effect.Parameters["Light0DiffuseColor"].SetValue(new Vector3(1, 0.9607844f, 0.8078432f));
                        effect.Parameters["Light0SpecularColor"].SetValue(new Vector3(1, 0.9607844f, 0.8078432f));

                        // Fill light.
                        effect.Parameters["Light1Direction"].SetValue(new Vector3(0.7198464f, 0.3420201f, 0.6040227f));
                        effect.Parameters["Light1DiffuseColor"].SetValue(new Vector3(0.9647059f, 0.7607844f, 0.4078432f));
                        effect.Parameters["Light1SpecularColor"].SetValue(Vector3.Zero);

                        // Back light.
                        effect.Parameters["Light1Direction"].SetValue(new Vector3(0.4545195f, -0.7660444f, 0.4545195f));
                        effect.Parameters["Light1DiffuseColor"].SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));
                        effect.Parameters["Light1SpecularColor"].SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));

                        // Ambient light.
                        effect.Parameters["AmbientLightColor"].SetValue(Color.White.ToVector3());
                        effect.Parameters["AmbientIntensity"].SetValue(0.10f);
                  
                        // Could not find out how to return nothing with lambda
                        return null;
                    });
            }

        }



        protected override void UnloadContent()
        {
        }
        #endregion

        #region updates
        protected override void Update(GameTime gameTime)
        {

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            float speed = 0.001f;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                speed *= 100f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                speed = 1;
            }
            CameraControl.Hover(cameraPosition, lookAt, (float) (gameTime.ElapsedGameTime.TotalMilliseconds * speed));
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                var position = cameraPosition;
            }
            base.Update(gameTime);
        }
        #endregion

        #region rendering
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            cameraPosition += CameraControl.CameraResult;
            lookAt += CameraControl.LookAtResult + CameraControl.CameraResult;

            var world = Matrix.Identity;
            var view = Matrix.CreateLookAt(cameraPosition, lookAt, Vector3.Up);

            //Render skybox first:
            //Invert culling
            GraphicsDevice.RasterizerState = invertedCulling;
            var skyBoxWorld = Matrix.CreateScale(10000.0f) * Matrix.CreateTranslation(cameraPosition);
            foreach (ModelMesh mesh in skybox.Meshes)
            {

                foreach (BasicEffect effect in mesh.Effects)
                {
                   // effect.DisableDefaultLighting();
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = skyBoxWorld;
                }
                mesh.Draw();
            }
            //restore culling
            GraphicsDevice.RasterizerState = normalCulling;

            foreach (var model in models)
            {
                model.Render(view, projection, world, cameraPosition);
            }
            base.Draw(gameTime);
        }
        #endregion
    }
       
}
