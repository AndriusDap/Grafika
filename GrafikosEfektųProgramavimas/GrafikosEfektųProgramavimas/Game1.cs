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
        Model skybox;
        float aspectRatio;

        Vector3 SunPosition;
        Matrix SunLookAt;
        Matrix SunProjection;
        
        RasterizerState invertedCulling;
        RasterizerState normalCulling;

        RenderTarget2D ShadowRenderTarget;
        
        RenderTarget2D[] RenderPasses;

        Effect shadowShader;
        Effect SumShader;
        int ShadowMapSize = 2048;
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
            cameraPosition = new Vector3(-200, -50, -500);
            lookAt = cameraPosition + Vector3.UnitX;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.10f, 100000.0f);
            SunPosition = new Vector3(-200, -50, -500);

           
            SunLookAt = Matrix.CreateLookAt(SunPosition, SunPosition + Vector3.Left, Vector3.Up);
            SunProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90.0f), 1.0f, 1.0f, 500.0f);
            var pp = graphics.GraphicsDevice.PresentationParameters;
            var height = pp.BackBufferHeight;
            var width = pp.BackBufferWidth;
            ShadowRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, ShadowMapSize, ShadowMapSize, false, pp.BackBufferFormat, DepthFormat.Depth24Stencil8);
            for (int i = 0; i < RenderPasses.Length; i++)
            {
                RenderPasses[i] = new RenderTarget2D(graphics.GraphicsDevice, width, height, false, graphics.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            }
        }
        Effect depthshader;
        Effect Red;
        protected override void LoadContent()
        {
            SumShader = Content.Load<Effect>("SumShader");
            shadowShader = Content.Load<Effect>("ShadowShader");
            Red = Content.Load<Effect>("red");
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

            Func<Effect, ModelMesh, Object> func = (effect, mesh) =>
                    {
                        effect.Parameters["DiffuseIntensity"].SetValue(0.50f);

                        String parsedMeshName = ParseMeshName(mesh.Name);
                        effect.Parameters["DiffuseTexture"].SetValue(textures[parsedMeshName]);

                        // Key light.
                        effect.Parameters["Light0Direction"].SetValue(new Vector3(-0.5265408f, -0.5735765f, -0.6275069f));
                        effect.Parameters["Light0DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));
                        effect.Parameters["Light0SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));

                        // Fill light.
                        effect.Parameters["Light1Direction"].SetValue(new Vector3(0.7198464f, 0.3420201f, 0.6040227f));
                        effect.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.9647059f, 0.7607844f, 0.4078432f));
                        effect.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(Vector3.Zero);

                        // Back light.
                        effect.Parameters["Light1Direction"].SetValue(new Vector3(0.4545195f, -0.7660444f, 0.4545195f));
                        effect.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));
                        effect.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));

                        // Ambient light.
                        effect.Parameters["AmbientLightColor"].SetValue(Color.White.ToVector3());//
                        effect.Parameters["AmbientIntensity"].SetValue(0.30f);

                        // Could not find out how to return nothing with lambda
                        return null;
                    };

            // Set up shaders for created models:
            foreach (var model in models)
            {
                model.SetUpEffects((effect, mesh) =>
                    {
                        effect.Parameters["DiffuseIntensity"].SetValue(0.50f);

                        String parsedMeshName = ParseMeshName(mesh.Name);
                        effect.Parameters["DiffuseTexture"].SetValue(textures[parsedMeshName]);

                        // Key light.
                        effect.Parameters["Light0Direction"].SetValue(new Vector3(-0.5265408f, -0.5735765f, -0.6275069f));
                        effect.Parameters["Light0DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));
                        effect.Parameters["Light0SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));

                        // Fill light.
                        effect.Parameters["Light1Direction"].SetValue(new Vector3(0.7198464f, 0.3420201f, 0.6040227f));
                        effect.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.9647059f, 0.7607844f, 0.4078432f));
                        effect.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(Vector3.Zero);

                        // Back light.
                        effect.Parameters["Light1Direction"].SetValue(new Vector3(0.4545195f, -0.7660444f, 0.4545195f));
                        effect.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));
                        effect.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));

                        // Ambient light.
                        effect.Parameters["AmbientLightColor"].SetValue(Color.White.ToVector3());//
                        effect.Parameters["AmbientIntensity"].SetValue(0.30f);

                        shadowShader.Parameters["DiffuseTexture"].SetValue(textures[parsedMeshName]);

                        // Key light.
                        shadowShader.Parameters["Light0Direction"].SetValue(new Vector3(-0.5265408f, -0.5735765f, -0.6275069f));
                        shadowShader.Parameters["Light0DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));
                        shadowShader.Parameters["Light0SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(1, 0.9607844f, 0.8078432f));

                        // Fill light.
                        shadowShader.Parameters["Light1Direction"].SetValue(new Vector3(0.7198464f, 0.3420201f, 0.6040227f));
                        shadowShader.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.9647059f, 0.7607844f, 0.4078432f));
                        shadowShader.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(Vector3.Zero);

                        // Back light.
                        shadowShader.Parameters["Light1Direction"].SetValue(new Vector3(0.4545195f, -0.7660444f, 0.4545195f));
                        shadowShader.Parameters["Light1DiffuseColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));
                        shadowShader.Parameters["Light1SpecularColor"].SetValue(Color.White.ToVector3());//SetValue(new Vector3(0.3231373f, 0.3607844f, 0.3937255f));

                        // Ambient light.
                        shadowShader.Parameters["AmbientLightColor"].SetValue(Color.White.ToVector3());//
                        shadowShader.Parameters["AmbientIntensity"].SetValue(0.30f);

                        shadowShader.Parameters["DiffuseIntensity"].SetValue(0.50f);
               
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
            float speed = 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                speed *= 10f;
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
            var world = Matrix.Identity;
            var view = Matrix.CreateLookAt(cameraPosition, lookAt, Vector3.Up);
                        
            // ShadowMap pass:
            GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
            GraphicsDevice.Clear(Color.Black);
            foreach (var model in models)
            {
                model.Render(depthshader, SunLookAt, SunProjection, Matrix.Identity, SunPosition);
            }

            cameraPosition += CameraControl.CameraResult;
            lookAt += CameraControl.LookAtResult + CameraControl.CameraResult;

            int currentPass = 0;
            GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.SetRenderTarget(RenderPasses[currentPass++]);
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // Shadow pass:
            shadowShader.Parameters["LightView"].SetValue(SunLookAt);
            shadowShader.Parameters["LightProjection"].SetValue(SunProjection);
            shadowShader.Parameters["ShadowTexture"].SetValue((Texture2D) ShadowRenderTarget);
            foreach (var model in models)
            {
                model.Render(shadowShader, view, projection, world, cameraPosition);
            }

            base.Draw(gameTime);
            return;            
            GraphicsDevice.SetRenderTarget(RenderPasses[currentPass++]);
            GraphicsDevice.Clear(Color.CornflowerBlue);
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

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            
            SumShader.Parameters["Pass0"].SetValue(RenderPasses[0]);
            SumShader.Parameters["Pass1"].SetValue(RenderPasses[1]);
            using (SpriteBatch sprite = new SpriteBatch(GraphicsDevice))
            {
                sprite.Begin(0, BlendState.AlphaBlend);//, null, null, null, SumShader);
                sprite.Draw(ShadowRenderTarget, new Vector2(0, 0), Color.White);
                sprite.End();
            }
            base.Draw(gameTime);
        }
        #endregion
    }
       
}
