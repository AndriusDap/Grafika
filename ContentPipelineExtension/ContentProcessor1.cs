using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;

// TODO: replace these with the processor input and output types.
using TInput = System.String;
using TOutput = System.String;

namespace ContentPipelineExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Model (Custom) - Select effect file")]
    public class ContentProcessor1 : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return base.Process(input, context);
        }

        [Browsable(false)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get { return base.DefaultEffect; }
            set { base.DefaultEffect = value; }
        }

        [Browsable(true)]
        public string Effect
        {
            get { return effectName; }
            set { effectName = value; }
        }
        private string effectName;

        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            EffectMaterialContent myMaterial = new EffectMaterialContent();

            if (!String.IsNullOrWhiteSpace(effectName))
            {
                myMaterial.Effect = new ExternalReference<EffectContent>(effectName);
            }
            else
            {
                throw new ArgumentNullException("Effect file not specified!");
            }

            foreach (var texture in material.Textures)
            {
                myMaterial.Textures.Add(texture.Key, texture.Value);
            }
            return context.Convert<MaterialContent, MaterialContent>(myMaterial, typeof(MaterialProcessor).Name);
        }
    }
}