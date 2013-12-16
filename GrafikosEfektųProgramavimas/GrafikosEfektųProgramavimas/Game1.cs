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

        List<RenderableObject> models;
        Dictionary<String, Texture2D> textures;
        Model skybox;
        float aspectRatio;
        Matrix World;
        Vector3 SunPosition;
        Matrix SunLookAt;
        Matrix SunProjection;
        
        RenderTarget2D ShadowRenderTarget;
        
        RenderTarget2D[] RenderPasses;

        Effect SumShader;
        int ShadowMapSize = 1024;
        Texture2D CellMap;
        RasterizerState invertedCulling;
        RasterizerState normalCulling;

        RenderTarget2D RenderTarget;
        Texture2D RenderBuffer;
        Effect depthshader;


        // Post effects
        Effect SobelShader;
        Effect ScharrShader;
        Effect ActivePostEffect = null;

        // Main shaders
        Effect ShadowShader;
        Effect ToonShader;
        Effect ActiveMainShader = null;
        bool BasicShaderEnabled = false;

        // Aditional effects
        bool SkyboxEnabled = true;
        bool FogEnabled = true;

        BasicParticleSystem Particles;
        #endregion

        #region initialization
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 1200;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            models = new List<RenderableObject>();
            textures = new Dictionary<String, Texture2D>();                  
            invertedCulling = new RasterizerState();
            invertedCulling.CullMode = CullMode.CullClockwiseFace;
            
            normalCulling = new RasterizerState();
            normalCulling.CullMode = CullMode.CullCounterClockwiseFace;

            
            RenderPasses = new RenderTarget2D[2];

        }

        public static String ParseMeshName(String name)
        {
            return name.Split('_')[0];
        }
       
        protected override void Initialize()
        {
            base.Initialize();
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            cameraPosition = new Vector3(0f, 0f, 0f);
            lookAt = cameraPosition + Vector3.UnitX;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.1f, 50000.0f);
            SunPosition = new Vector3(0, 0, 0);

            SunLookAt = Matrix.CreateLookAt(SunPosition, SunPosition + Vector3.UnitX, Vector3.Up);
            SunProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90.0f), 1.0f,  0.01f, 50000.0f);
            var pp = graphics.GraphicsDevice.PresentationParameters;
            var height = pp.BackBufferHeight;
            var width = pp.BackBufferWidth;
            ShadowRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, ShadowMapSize, ShadowMapSize, false, SurfaceFormat.Single, DepthFormat.Depth24);
            for (int i = 0; i < RenderPasses.Length; i++)
            {
                RenderPasses[i] = new RenderTarget2D(graphics.GraphicsDevice, width, height, false, graphics.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            }
            World = Matrix.Identity * Matrix.CreateScale(0.00001f);
                
        }


        #endregion

        #region content loading

        static void SetUpShader(Effect e)
        {
            // Key light.
            e.Parameters["Light0Direction"].SetValue(new Vector3(-0.5265408f, -0.5735765f, -0.6275069f));
            e.Parameters["Light0DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));
            e.Parameters["Light0SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));

            // Fill light.
            e.Parameters["Light1Direction"].SetValue(new Vector3(0.7198464f, 0.3420201f, 0.6040227f));
            e.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.9647059f, 0.7607844f, 0.4078432f));
            e.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(Vector3.Zero);

            // Back light.
            e.Parameters["Light1Direction"].SetValue(new Vector3(0.4545195f, -0.7660444f, 0.4545195f));
            e.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));
            e.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));

            // Ambient light.
            e.Parameters["AmbientLightColor"].SetValue(Color.White.ToVector3());//
            e.Parameters["AmbientIntensity"].SetValue(0.30f);

            e.Parameters["DiffuseIntensity"].SetValue(0.50f);
        }
        
        protected override void LoadContent()
        {
            SumShader = Content.Load<Effect>("SumShader");
            
            depthshader = Content.Load<Effect>("DepthMapShader");

            var terrain = Content.Load<Model>("large_heightmap");
            models.Add(new RenderableObject(terrain));

            skybox = Content.Load<Model>("skybox2");

            var cube = Content.Load<Model>("cube2");
            var renderable = new RenderableObject(cube);
            renderable.Position = new Vector3(-210, -50, -510);
            models.Add(renderable);

            renderable = new RenderableObject(cube);
            renderable.Position = new Vector3(-210, -56, -510);
            models.Add(renderable);
            
            renderable = new RenderableObject(cube);
            renderable.Position = new Vector3(-220, -56, -510);
            models.Add(renderable);
            
            renderable = new RenderableObject(cube);
            renderable.Position = new Vector3(-210, -56, -520);
            models.Add(renderable);

            renderable = new RenderableObject(cube);
            renderable.Position = new Vector3(-220, -56, -520);
            models.Add(renderable);
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
            }
            // Set up shaders for created models:
            foreach (var model in models)
            {
                model.SetUpEffects((effect, mesh) =>
                {
                    effect.Parameters["DiffuseIntensity"].SetValue(0.50f);

                    String parsedMeshName = ParseMeshName(mesh.Name);
                    effect.Parameters["DiffuseTexture"].SetValue(textures[parsedMeshName]);
                    SetUpShader(effect);
                    return null;
                });
            }

            CellMap = Content.Load<Texture2D>("celMap");


            ShadowShader = Content.Load<Effect>("ShadowShader");
            SetUpShader(ShadowShader);

            ToonShader = Content.Load<Effect>("TerrainToonShader");
            SetUpShader(ToonShader);
            ToonShader.Parameters["CellMap"].SetValue(CellMap);
            ActiveMainShader = ToonShader;


           PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
           RenderTarget = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, graphics.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);

           float width = 1.0f / (float)pp.BackBufferWidth;
           float height = 1.0f / (float)pp.BackBufferHeight;
           SobelShader = Content.Load<Effect>("SobelShader");
           ScharrShader = Content.Load<Effect>("ScharrShader");
           var pixelOffsetX = new Vector3(-width, 0, width);
           var pixelOffsetY = new Vector3(-height, 0, height);

           SobelShader.Parameters["pixelOffsetX"].SetValue(pixelOffsetX);
           SobelShader.Parameters["pixelOffsetY"].SetValue(pixelOffsetY);

           ScharrShader.Parameters["pixelOffsetX"].SetValue(pixelOffsetX);
           ScharrShader.Parameters["pixelOffsetY"].SetValue(pixelOffsetY);
           Particles = new BasicParticleSystem(cube);
        }



        protected override void UnloadContent()
        {
            // Or don't
        }
        #endregion

        KeyboardState oldKeyboardState = Keyboard.GetState();
        private bool IsKeyPressed(Keys k)
        {
            return (oldKeyboardState.IsKeyUp(k) && Keyboard.GetState().IsKeyDown(k));            
        }

        #region updates
        protected override void Update(GameTime gameTime)
        {

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            float speed = 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                speed *= 10f;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                         
                CameraControl.Hover(cameraPosition, lookAt, (float)(gameTime.ElapsedGameTime.TotalMilliseconds * speed));
                if (this.IsMouseVisible == true)
                {
                    CameraControl.CameraResult = Vector3.Zero;
                    CameraControl.LookAtResult = Vector3.Zero;
                }   
                this.IsMouseVisible = false;    
            }
            else
            {
                this.IsMouseVisible = true;
            }
            
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                var position = cameraPosition;
            }


            
            if (IsKeyPressed(Keys.NumPad1))
            {
                if (ActivePostEffect == SobelShader)
                {
                    ActivePostEffect = null;
                }
                else
                {
                    ActivePostEffect = SobelShader;
                }
            }
            if (IsKeyPressed(Keys.NumPad2))
            {
                if (ActivePostEffect == ScharrShader)
                {
                    ActivePostEffect = null;
                }
                else
                {
                    ActivePostEffect = ScharrShader;
                }
            }

            if(IsKeyPressed(Keys.NumPad3))
            {
                if (ActiveMainShader == ToonShader && !BasicShaderEnabled)
                {
                    ActiveMainShader.Parameters["IsThisToon"].SetValue(0f);
                    BasicShaderEnabled = true;
                }
                else
                {
                    BasicShaderEnabled = false;
                    ActiveMainShader = ToonShader;
                    ActiveMainShader.Parameters["IsThisToon"].SetValue(1f);
                }
            }

            if(IsKeyPressed(Keys.NumPad4))
            {
                if (ActiveMainShader == ShadowShader)
                {
                    ActiveMainShader = ToonShader;
                    ActiveMainShader.Parameters["IsThisToon"].SetValue(0f);
                }
                else
                {
                    ActiveMainShader = ShadowShader;
                }
            }

            if (IsKeyPressed(Keys.NumPad7))
            {
                FogEnabled = !FogEnabled;
                ShadowShader.Parameters["FogEnabled"].SetValue(FogEnabled? 1f : 0f);
                ToonShader.Parameters["FogEnabled"].SetValue(FogEnabled ? 1f : 0f);
            }


            if (IsKeyPressed(Keys.NumPad9))
            {
                SkyboxEnabled = !SkyboxEnabled;
            }

            if (IsKeyPressed(Keys.Space))
            {
                Particles.SpawnParticles(cameraPosition, lookAt, 1f, 100, gameTime);
            }

            oldKeyboardState = Keyboard.GetState();
            Particles.Update(gameTime);
            base.Update(gameTime);
        }
        #endregion

        #region rendering

        protected override void Draw(GameTime gameTime)
        {
            var view = Matrix.CreateLookAt(cameraPosition, lookAt, Vector3.Up);
    
            // ShadowMap pass:
            GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
            GraphicsDevice.Clear(Color.Black);
            foreach (var model in models)
            {
                model.Render(depthshader, SunLookAt, SunProjection, World, SunPosition);
            }
           
            cameraPosition += CameraControl.CameraResult;
            lookAt += CameraControl.LookAtResult + CameraControl.CameraResult;

            CameraControl.LookAtResult = Vector3.Zero;
            CameraControl.CameraResult = Vector3.Zero;

            GraphicsDevice.SetRenderTarget(RenderTarget);

            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            GraphicsDevice.Clear(Color.FromNonPremultiplied((int)(0.3 * 255), (int)(0.3 * 255), (int)(0.3 * 255), 255));

            if (SkyboxEnabled)
            {     
                // Skybox:
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
            }

            Particles.Render(graphics.GraphicsDevice, World, view, projection, cameraPosition);
            // Shadow pass:
            ShadowShader.Parameters["LightView"].SetValue(SunLookAt);
            ShadowShader.Parameters["LightProjection"].SetValue(SunProjection);
            ShadowShader.Parameters["ShadowTexture"].SetValue((Texture2D) ShadowRenderTarget);
            foreach (var model in models)
            {
                ActiveMainShader.Parameters["DiffuseTexture"].SetValue(textures[ParseMeshName(model.ObjectModel.Meshes[0].Name)]);
                model.Render(ActiveMainShader, view, projection, World, cameraPosition);
            }
       
            GraphicsDevice.SetRenderTarget(null);
            RenderBuffer = (Texture2D)RenderTarget;
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            
            using (SpriteBatch sprite = new SpriteBatch(GraphicsDevice))
            {
                sprite.Begin(0, BlendState.Opaque, null, null, null, ActivePostEffect);
                sprite.Draw(RenderBuffer, new Vector2(0, 0), Color.White);
                sprite.End();
            }
            base.Draw(gameTime);
            return;          
        }
        #endregion
    }
       
}
