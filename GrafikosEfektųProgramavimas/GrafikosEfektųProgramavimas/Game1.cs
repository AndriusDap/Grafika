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
        #region parameters
        GraphicsDeviceManager graphics;
        Vector3 cameraPosition;
        Vector3 lookAt;
        Matrix projection;
        List<Model> models;
        Dictionary<String, Texture2D> textures;
        Dictionary<String, Texture2D> specularTextures;
        Dictionary<String, Texture2D> normalTextures;
        Dictionary<String, Texture2D> lightMapTextures;
        Model skybox;
        Texture2D CellMap;
        float SpecularToggle;
        float NormalToggle;
        float LightMapToggle;
        float aspectRatio;
        float colorMultiplicationToggle;
        Effect SobelShader;
        RasterizerState invertedCulling;
        RasterizerState normalCulling;

        RenderTarget2D RenderTarget;
        Texture2D RenderBuffer;
        #endregion

        #region initialization
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 900;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            models = new List<Model>();
            textures = new Dictionary<String, Texture2D>();
            specularTextures = new Dictionary<String, Texture2D>();
            normalTextures = new Dictionary<String, Texture2D>();
            lightMapTextures = new Dictionary<String, Texture2D>();
            SpecularToggle = 1;
            NormalToggle = 1;
            LightMapToggle = 1;

           
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
            cameraPosition = new Vector3(3, 0, 0);
            lookAt = new Vector3(0, 0, 0);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.10f, 100000.0f);
           
        }
        #endregion

        #region content loading
        protected override void LoadContent()
        {
            var terrain = Content.Load<Model>("large_heightmap");
            models.Add(terrain);
            skybox = Content.Load<Model>("skybox2");

            List<String> textureNames = new List<String>();
            foreach (var m in models)
            {
                foreach (var mesh in m.Meshes)
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
               //specularTextures.Add(textureName, Content.Load<Texture2D>(textureName + "_SPEC"));
               normalTextures.Add(textureName, Content.Load<Texture2D>(textureName + "_NORM"));
            }

           CellMap = Content.Load<Texture2D>("celMap");
           PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
           RenderTarget = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, graphics.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);

           float width = 1.0f / (float)pp.BackBufferWidth;
           float height = 1.0f / (float)pp.BackBufferHeight;
           SobelShader = Content.Load<Effect>("SobelShader");
           var pixelOffsetX = new Vector3(-width, 0, width);
           var pixelOffsetY = new Vector3(-height, 0, height);

           SobelShader.Parameters["pixelOffsetX"].SetValue(pixelOffsetX);
           SobelShader.Parameters["pixelOffsetY"].SetValue(pixelOffsetY);
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
            SpecularToggle = Keyboard.GetState().IsKeyDown(Keys.NumPad1) ? 0 : 1;
            NormalToggle = Keyboard.GetState().IsKeyDown(Keys.NumPad2) ? 0 : 1;
            LightMapToggle = Keyboard.GetState().IsKeyDown(Keys.NumPad3) ? 0 : 1;
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
            base.Update(gameTime);
        }
        #endregion

        #region rendering

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(RenderTarget);
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
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
            
           /* foreach (ModelMesh mesh in terrain.Meshes)
            {

                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = world;
                }
                mesh.Draw();
            }*/

            foreach (var model in models)
            {
                foreach (var mesh in model.Meshes)
                {
                    var x = mesh.Name;
                    var a = x.Length;
                    foreach (Effect effect in mesh.Effects)
                    {
                        String parsedMeshName = ParseMeshName(mesh.Name);
                        effect.Parameters["SpecularToggle"].SetValue(SpecularToggle);
                        effect.Parameters["NormalToggle"].SetValue(NormalToggle);
                        effect.Parameters["LightMapToggle"].SetValue(LightMapToggle);

                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["DiffuseIntensity"].SetValue(0.50f);

                       
                        effect.Parameters["DiffuseTexture"].SetValue(textures[parsedMeshName]);
                      //  effect.Parameters["SpecularTexture"].SetValue(specularTextures[parsedMeshName]);
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
                        effect.Parameters["Light1DiffuseColor"].SetValue( new Vector3(0.3231373f, 0.3607844f, 0.3937255f));
                        effect.Parameters["Light1SpecularColor"].SetValue( new Vector3(0.3231373f, 0.3607844f, 0.3937255f));

                        // Ambient light.
                        effect.Parameters["AmbientLightColor"].SetValue(Color.White.ToVector3());//new Vector3(0.05333332f, 0.09882354f, 0.1819608f));
                        effect.Parameters["AmbientIntensity"].SetValue(0.10f);
                  
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["CellMap"].SetValue(CellMap);
                            
                    }
                    mesh.Draw();
                }
            }
            GraphicsDevice.SetRenderTarget(null);
            RenderBuffer = (Texture2D)RenderTarget;
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            
            using (SpriteBatch sprite = new SpriteBatch(GraphicsDevice))
            {
                sprite.Begin(0, BlendState.Opaque, null, null, null, SobelShader);
                sprite.Draw(RenderBuffer, new Vector2(0, 0), Color.White);
                sprite.End();
            }
            base.Draw(gameTime);
        }
        #endregion
    }
       
}
