using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;
using Myre.Graphics.Pipeline.Models;

namespace Myre.Graphics.Pipeline.Fonts
{
    [ContentSerializerRuntimeType("Myre.Graphics.Geometry.Text.VertexFont, Myre.Graphics")]
    public class VertexFontContent
    {
        public IDictionary<char, VertexCharacterContent> Characters { get; private set; }

        public char DefaultCharacter { get; private set; }

        public VertexFontContent(IDictionary<char, VertexCharacterContent> characters, char defaultCharacter)
        {
            if (!characters.ContainsKey(defaultCharacter))
                throw new ArgumentException("Defualt character of a font must be a character in the font");

            DefaultCharacter = defaultCharacter;
            Characters = characters;
        }
    }

    [ContentSerializerRuntimeType("Myre.Graphics.Geometry.Text.VertexCharacter, Myre.Graphics")]
    public class VertexCharacterContent
    {
        /// <summary>
        /// Kerning to use to offset the next character in the string.
        /// i.e.
        /// Draw(ThisCharacter);
        /// MovePen(HorizontalCharacterKerning[nextCharacter] + nextCharacter.Width)
        /// Draw(nextCharacter);
        /// </summary>
        public Dictionary<char, float> HorizontalCharacterKerning { get; private set; }

        public MyreMeshContent Mesh { get; private set; }

        public float Width { get; private set; }

        public char Character { get; private set; }

        public VertexCharacterContent(char c, Dictionary<char, float> horizontalCharacterKerning, MyreMeshContent mesh, float width)
        {
            Character = c;
            Width = width;
            Mesh = mesh;
            HorizontalCharacterKerning = horizontalCharacterKerning;
        }
    }

    [ContentTypeWriter]
    public class VertexCharacterContentWriter : ContentTypeWriter<VertexCharacterContent>
    {
        protected override void Write(ContentWriter output, VertexCharacterContent value)
        {
            //Write character
            output.Write(value.Character);

            //Write out kerning data
            output.Write(value.HorizontalCharacterKerning.Count);
            foreach (var kern in value.HorizontalCharacterKerning)
            {
                output.Write(kern.Key);
                output.Write(kern.Value);
            }

            //Write out mesh
            output.WriteObject(value.Mesh);

            //Write out width
            output.Write(value.Width);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.Text.VertexCharacterReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.Text.VertexCharacter, Myre.Graphics";
        }
    }

    [ContentTypeWriter]
    public class VertexFontContentWriter : ContentTypeWriter<VertexFontContent>
    {
        protected override void Write(ContentWriter output, VertexFontContent value)
        {
            //Write out character data
            output.Write(value.Characters.Count);
            foreach (var character in value.Characters)
                output.WriteObject(character.Value);

            //write out the default character
            output.Write(value.DefaultCharacter);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.Text.VertexFontReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.Text.VertexFont, Myre.Graphics";
        }
    }
}
