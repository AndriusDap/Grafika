using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
/*using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;*/
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

// TODO: replace this with the type you want to import.
using TImport = Microsoft.Xna.Framework.Content.Pipeline.Graphics.MeshContent;
using Microsoft.Xna.Framework;

namespace TerrainContentPipelineExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>
    [ContentImporter(".*", DisplayName = "HeightMap importer", DefaultProcessor = "HeightMapProcessor")]
    public class HeightMapImporter : ContentImporter<TImport>
    {
        public override TImport Import(string filename, ContentImporterContext context)
        {
            var bmp = new Bitmap(filename);
            MeshBuilder mb = MeshBuilder.StartMesh(filename);
            // Rastertek
            var index = 0;
            Vector3[] heightMap = new Vector3[bmp.Height * bmp.Width];
            for(var j = 0; j < bmp.Height; j++)
            {
                for(var i = 0; i < bmp.Width; i++)
                {
                    var colour = bmp.GetPixel(j, i);
                    index = (bmp.Height * j) + i;
                    
                    heightMap[index].X = i;
                    heightMap[index].Y = colour.B;
                    heightMap[index].Z = j;
                }
            }
            int height = bmp.Height;
            int width = bmp.Width;
            // Load the vertex and index array with the terrain data.
            index = 0;
            for (var j = 0; j < height - 1; j++)
            {
                for (var i = 0; i < width - 1; i++)
                {
                    var index1 = (height * j) + i;          // Bottom left.
                    var index2 = (height * j) + (i + 1);      // Bottom right.
                    var index3 = (height * (j + 1)) + i;      // Upper left.
                    var index4 = (height * (j + 1)) + (i + 1);  // Upper right.

                    // Upper left.                    
                    index = mb.CreatePosition(heightMap[index3].X, heightMap[index3].Y, heightMap[index3].Z);
                    mb.AddTriangleVertex(index);
                    // Upper right.
                    index = mb.CreatePosition(heightMap[index4].X, heightMap[index4].Y, heightMap[index4].Z);
                    mb.AddTriangleVertex(index);
                    // Upper right.
                    index = mb.CreatePosition(heightMap[index4].X, heightMap[index4].Y, heightMap[index4].Z);
                    mb.AddTriangleVertex(index);
                    // Bottom left.
                    index = mb.CreatePosition(heightMap[index1].X, heightMap[index1].Y, heightMap[index1].Z);
                    mb.AddTriangleVertex(index);
                    // Bottom left.
                    index = mb.CreatePosition(heightMap[index1].X, heightMap[index1].Y, heightMap[index1].Z);
                    mb.AddTriangleVertex(index);
                    // Upper left.
                    index = mb.CreatePosition(heightMap[index3].X, heightMap[index3].Y, heightMap[index3].Z);
                    mb.AddTriangleVertex(index);
                    // Bottom left.
                    index = mb.CreatePosition(heightMap[index1].X, heightMap[index1].Y, heightMap[index1].Z);
                    mb.AddTriangleVertex(index);
                    // Upper right.
                    index = mb.CreatePosition(heightMap[index4].X, heightMap[index4].Y, heightMap[index4].Z);
                    mb.AddTriangleVertex(index);
                    // Upper right.
                    index = mb.CreatePosition(heightMap[index4].X, heightMap[index4].Y, heightMap[index4].Z);
                    mb.AddTriangleVertex(index);
                    // Bottom right.
                    index = mb.CreatePosition(heightMap[index2].X, heightMap[index2].Y, heightMap[index2].Z);
                    mb.AddTriangleVertex(index);
                    // Bottom right.
                    index = mb.CreatePosition(heightMap[index2].X, heightMap[index2].Y, heightMap[index2].Z);
                    mb.AddTriangleVertex(index);
                    // Bottom left.
                    index = mb.CreatePosition(heightMap[index1].X, heightMap[index1].Y, heightMap[index1].Z);
                    mb.AddTriangleVertex(index);   
                   
                }
            }
            return mb.FinishMesh();
        }
    }
}
