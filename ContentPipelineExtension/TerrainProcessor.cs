using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

// TODO: replace these with the processor input and output types.
using TInput = System.String;
using TOutput = System.String;

namespace ContentPipelineExtension
{   
    /// <summary>
    /// Custom processor to generate terrain from heigthmap
    /// </summary>
    [ContentProcessor(DisplayName = "TerrainProcessor")]
    public class TerrainProcessor : ContentProcessor<Texture2DContent, ModelContent>
    {
        const float Scale = 4;
        const float Bumpiness = 128;
        const float CoordinateScale = 0.1f;
        const string Texture = "Terrain_DIFF.jpg";

        public override ModelContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            MeshBuilder builder = MeshBuilder.StartMesh("Terrain");

            // Keièiamas pradinio paveikslo pixeliø formatas ið sveikø skaièiø á slankaus kablelio
            input.ConvertBitmapType(typeof(PixelBitmapContent<float>));
            PixelBitmapContent<float> heightMap = input.Mipmaps[0] as PixelBitmapContent<float>;
            // Sukuriamos þemës koordinatës
            for (int y = 0; y < heightMap.Height; y++)
            {
                for (int x = 0; x < heightMap.Width; x++)
                {
                    Vector3 position;

                    position.X = (x - heightMap.Width / 2) * Scale;
                    position.Z = (y - heightMap.Height / 2) * Scale;

                    position.Y = (heightMap.GetPixel(x, y) - 1) * Bumpiness;

                    builder.CreatePosition(position);
                }
            }

            // Sukuriama medþiaga þemei
            var material = new BasicMaterialContent();
            
            // Nurodoma medþiagos tekstûra, imama ið konstantø
            var dir = Path.GetDirectoryName(input.Identity.SourceFilename);
            var filename = Path.Combine(dir, Texture);

            material.Texture = new ExternalReference<TextureContent>(filename);
            

            var shadedMaterial = new EffectMaterialContent();
            shadedMaterial.Effect = new ExternalReference<EffectContent>("TerrainToonShader.fx");
            foreach (var texture in material.Textures)
            {
                shadedMaterial.Textures.Add(texture.Key, texture.Value);
            }
            builder.SetMaterial(shadedMaterial);

            int texCoordId = builder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));

            // Sukuriami index ir vertex buferiai
            for (int y = 0; y < heightMap.Height - 1; y++)
            {
                for (int x = 0; x < heightMap.Width - 1; x++)
                {
                    AddVertex(builder, texCoordId, heightMap.Width, x, y);
                    AddVertex(builder, texCoordId, heightMap.Width, x + 1, y);
                    AddVertex(builder, texCoordId, heightMap.Width, x + 1, y + 1);

                    AddVertex(builder, texCoordId, heightMap.Width, x, y);
                    AddVertex(builder, texCoordId, heightMap.Width, x + 1, y + 1);
                    AddVertex(builder, texCoordId, heightMap.Width, x, y + 1);
                }
            }

            MeshContent terrainMesh = builder.FinishMesh();
            // Rolimesnis apdorojimas bus valdomas XNA.
            ModelProcessor mp = new ModelProcessor();
            mp.GenerateTangentFrames = true;
            return mp.Process(terrainMesh, context);
            
            //return context.Convert<MeshContent, ModelContent>(terrainMesh, "ModelProcessor");
        }

        static void AddVertex(MeshBuilder builder, int texCoordId, int w, int x, int y)
        {
            builder.SetVertexChannelData(texCoordId, new Vector2(x, y) * CoordinateScale);

            builder.AddTriangleVertex(x + y * w);
        }
    }
}