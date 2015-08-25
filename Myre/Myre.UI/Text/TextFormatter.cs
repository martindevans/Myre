using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.UI.Text
{
    public static class TextFormatter
    {
        struct FontChange
        {
            public int Index;
            public SpriteFont Font;
        }

        enum TagType
        {
            None,
            FontStart,
            FontEnd,
            ColourStart,
            ColourEnd,
            NewLine
        }

        private static ContentParser<SpriteFont> _fonts;
        private static readonly Stack<SpriteFont> _fontHistory;
        private static readonly Stack<Color> _colourHistory;
        private static readonly StringBuilder _buffer;
        private static readonly StringBuilder _input;
        private static readonly List<FontChange> _fontChanges;
        private static readonly List<float> _lineWidths;

        private static SpriteFont _defaultFont;
        private static Color _defaultColour;

        static TextFormatter()// TextFormatter(ContentManager fontContent)
        {
            //fonts = new ContentParser<SpriteFont>(fontContent);
            _fontHistory = new Stack<SpriteFont>();
            _colourHistory = new Stack<Color>();
            _buffer = new StringBuilder();
            _input = new StringBuilder();
            _fontChanges = new List<FontChange>();
            _lineWidths = new List<float>();
        }

        public static void SetFontSource(ContentManager fontContent)
        {
            _fonts = new ContentParser<SpriteFont>(fontContent);
        }

        public static Vector2 MeasureParsedString(this SpriteFont font, StringPart text, int wrapWidth)
        {
            // bit of a hack..
            return font.MeasureParsedString(text, Vector2.One, wrapWidth);
        }

        public static Vector2 MeasureParsedString(this SpriteFont font, StringPart text, Vector2 scale, int wrapWidth)
        {
            // bit of a hack..
            return DrawParsedString(null, font, text, Vector2.Zero, Color.White, 0, Vector2.Zero, scale, wrapWidth, Justification.Left, false);
        }

        public static Vector2 DrawParsedString(this SpriteBatch spriteBatch, SpriteFont font, StringPart text, Vector2 position, Color colour)
        {
            return DrawParsedString(spriteBatch, font, text, position, colour, 0, Vector2.Zero, Vector2.One, 0, Justification.Left);
        }

        public static Vector2 DrawParsedString(this SpriteBatch spriteBatch, SpriteFont font, StringPart text, Vector2 position, Color colour, float rotation, Vector2 origin, Vector2 scale, int wrapWidth, Justification justification)
        {
            if (wrapWidth <= 0)
                throw new ArgumentOutOfRangeException("wrapWidth", "wrapWidth must be greater than 0.");

            return DrawParsedString(spriteBatch, font, text, position, colour, rotation, origin, scale, wrapWidth, justification, true);
        }

        private static Vector2 DrawParsedString(this SpriteBatch spriteBatch, SpriteFont font, StringPart text, Vector2 position, Color colour, float rotation, Vector2 origin, Vector2 scale, int wrapWidth, Justification justification, bool drawingEnabled)
        {
            _input.Clear();
            _input.AppendPart(text);

            _defaultFont = font;
            _defaultColour = colour;

            _lineWidths.Clear();
            if (wrapWidth > 0)
                Wrap(_input, scale, wrapWidth);

            _input.Replace("\n", "[\n]");

            _colourHistory.Clear();
            _fontHistory.Clear();

            var currentPositionOffset = Vector2.Zero;
            var lineSpacing = CurrentFont().LineSpacing;
            var width = 0f;

            int lineIndex = 0;
            Vector2 justificationOffset = new Vector2(Justify(_lineWidths.Count > 0 ? _lineWidths[0] : 0, wrapWidth, justification), 0);

            int i = 0;
            int tagStart;
            int tagEnd;
            Vector2 size;
            while (i < _input.Length
                && (tagStart = IndexOf(_input, "[", i)) != -1
                && (tagEnd = IndexOf(_input, "]", tagStart)) != -1)
            {
                if (tagStart > i)
                {
                    SetBuffer(new StringPart(_input, i, tagStart - i));

                    if (drawingEnabled)
                        DrawBuffer(spriteBatch, ref position, rotation, ref origin, ref scale, currentPositionOffset + justificationOffset);

                    size = CurrentFont().MeasureString(_buffer).FromXNA();
                    currentPositionOffset.X += size.X;
                    lineSpacing = Math.Max(lineSpacing, (int)size.Y);
                    width = Math.Max(width, currentPositionOffset.X);
                }

                i = tagStart;

                if (ParseTag(new StringPart(_input, tagStart + 1, tagEnd - tagStart - 1), ref currentPositionOffset, ref lineSpacing, ref lineIndex) != TagType.None)
                {
                    i = tagEnd;

                    if (_lineWidths.Count > lineIndex)
                        justificationOffset.X = Justify(_lineWidths[lineIndex], wrapWidth, justification);
                }
                else
                {
                    SetBuffer(new StringPart(_input, i, 1));

                    if (drawingEnabled)
                        DrawBuffer(spriteBatch, ref position, rotation, ref origin, ref scale, currentPositionOffset + justificationOffset);

                    size = CurrentFont().MeasureString(_buffer).FromXNA();
                    currentPositionOffset.X += size.X;
                    lineSpacing = Math.Max(lineSpacing, (int)size.Y);
                    width = Math.Max(width, currentPositionOffset.X);
                }

                i++;
            }

            if (i != _input.Length)
            {                
                SetBuffer(new StringPart(_input, i, _input.Length - i));
                
                if (drawingEnabled)
                    DrawBuffer(spriteBatch, ref position, rotation, ref origin, ref scale, currentPositionOffset + justificationOffset);

                size = CurrentFont().MeasureString(_buffer).FromXNA();
                currentPositionOffset.X += size.X;
                lineSpacing = Math.Max(lineSpacing, (int)size.Y);
                width = Math.Max(width, currentPositionOffset.X);
            }

            return new Vector2(width, currentPositionOffset.Y + lineSpacing) * scale;
        }

        private static void SetBuffer(StringPart text)
        {
            _buffer.Clear();
            _buffer.AppendPart(text);
        }

        private static void DrawBuffer(SpriteBatch spriteBatch, ref Vector2 position, float rotation, ref Vector2 origin, ref Vector2 scale, Vector2 currentPositionOffset)
        {
            spriteBatch.DrawString(
                CurrentFont(),
                _buffer,
                position.ToXNA(),
                CurrentColour(),
                rotation,
                (origin - currentPositionOffset).ToXNA(),
                scale.ToXNA(),
                SpriteEffects.None,
                0);
        }

        private static float Justify(float lineWidth, int wrapWidth, Justification justification)
        {
            switch (justification)
            {
                case Justification.Centre:
                    return (wrapWidth - lineWidth) / 2f;
                case Justification.Right:
                    return (wrapWidth - lineWidth);
                default:
                    return 0;
            }
        }

        private static int IndexOf(StringBuilder text, string value, int index)
        {
            int c = 0;
            for (int i = index; i < text.Length; i++)
            {
                if (text[i] == value[c])
                    c++;
                else
                    c = 0;

                if (c == value.Length)
                    return i;
            }

            return -1;
        }

        private static void Wrap(StringBuilder input, Vector2 scale, float allowedWidth)
        {
            RecordFontChanges(input);

            float lineWidth = 0;
            float wordWidth = 0;
            int fontIndex = 0;
            int wordStart = 0;
            float spaceSize = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '[')
                {
                    var closeBracket = IndexOf(input, "]", i);
                    if (closeBracket != -1)
                    {
                        Vector2 currentPositionOffset = Vector2.Zero;
                        int a = 0, b = 0;
                        if (ParseTag(new StringPart(input, i + 1, closeBracket - i - 1), ref currentPositionOffset, ref a, ref b) != TagType.None)
                        {
                            i = closeBracket;
                            continue;
                        }
                    }
                }

                if (input[i] == '\n')
                {
                    _lineWidths.Add((lineWidth + wordWidth) / scale.X);

                    lineWidth = 0;
                    wordWidth = 0;
                    wordStart = i + 1;
                    continue;
                }

                SpriteFont font = _defaultFont;
                while (fontIndex < _fontChanges.Count && _fontChanges[fontIndex].Index <= i)
                {
                    font = _fontChanges[fontIndex].Font;
                    fontIndex++;
                }
                
                if (input[i] == ' ')
                {
                    wordStart = i + 1;
                    lineWidth += wordWidth;
                    spaceSize = font.MeasureString(" ").X * scale.X;
                    lineWidth += spaceSize;
                    wordWidth = 0;
                    continue;
                }

                _buffer.Clear();
                _buffer.AppendPart(new StringPart(input, i, 1));
                var size = font.MeasureString(_buffer).FromXNA() * scale;
                wordWidth += size.X;

                if (wordWidth > allowedWidth)
                {
                    _lineWidths.Add((wordWidth - size.X) / scale.X);

                    input.Insert(i, "\n");
                    //i++;

                    lineWidth = 0;
                    wordWidth = 0;
                }
                else if (lineWidth + wordWidth > allowedWidth)
                {
                    if (lineWidth > 0)
                        lineWidth -= spaceSize;
                    _lineWidths.Add(lineWidth / scale.X);

                    input.Insert(wordStart, "\n");
                    i++;

                    lineWidth = 0;
                }
            }

            _lineWidths.Add(lineWidth + wordWidth);
        }

        private static void RecordFontChanges(StringBuilder input)
        {
            _fontChanges.Clear();

            int tagStart;
            int tagEnd = 0;
            while ((tagStart = IndexOf(input, "[", tagEnd)) != -1
                && (tagEnd = IndexOf(input, "]", tagStart)) != -1)
            {
                var tag = new StringPart(input, tagStart + 1, tagEnd - tagStart - 1);

                var fontChanged = false;
                if (tag.StartsWith("f:") && _fonts != null)
                {
                    var fontName = tag.Substring(2);
                    SpriteFont font;
                    if (_fonts.TryParse(fontName, out font))
                    {
                        PushFont(font);
                        fontChanged = true;
                    }
                }
                else if (tag.Equals("/f"))
                {
                    PopFont();
                    fontChanged = true;
                }

                if (fontChanged)
                    _fontChanges.Add(new FontChange() { Font = CurrentFont(), Index = tagStart });
            }
        }

        private static TagType ParseTag(StringPart text, ref Vector2 position, ref int lineSpacing, ref int lineIndex)
        {
            var type = TagType.None;

            if (text.Equals("\n"))
            {
                NewLine(ref position, ref lineSpacing, ref lineIndex);
                type = TagType.NewLine;
            }
            else if (text.StartsWith("f:") && _fonts != null)
            {
                var fontName = text.Substring(2);
                SpriteFont font;
                if (_fonts.TryParse(fontName, out font))
                {
                    PushFont(font);
                    type = TagType.FontStart;
                }                    
            }
            else if (text.Equals("/f"))
            {
                PopFont();
                type = TagType.FontEnd;
            }
            else if (text.StartsWith("c:"))
            {
                var colourName = text.Substring(2);
                Color colour;
                if (ColourParser.TryParse(colourName, out colour))
                {
                    PushColour(colour);
                    type = TagType.ColourStart;
                }
            }
            else if (text.Equals("/c"))
            {
                PopColour();
                type = TagType.ColourEnd;
            }

            return type;
        }

        private static SpriteFont CurrentFont()
        {
            if (_fontHistory.Count > 0)
                return _fontHistory.Peek();

            return _defaultFont;
        }

        private static void PushFont(SpriteFont font)
        {
            _fontHistory.Push(font);
        }

        private static void PopFont()
        {
            if (_fontHistory.Count > 0)
                _fontHistory.Pop();
        }

        private static Color CurrentColour()
        {
            if (_colourHistory.Count > 0)
            {
                var colour = _colourHistory.Peek().ToVector4();
                colour *= _defaultColour.ToVector4();

                return new Color(colour);
            }

            return _defaultColour;
        }

        private static void PushColour(Color colour)
        {
            _colourHistory.Push(colour);
        }

        private static void PopColour()
        {
            if (_colourHistory.Count > 0)
                _colourHistory.Pop();
        }

        private static void NewLine(ref Vector2 position, ref int lineSpacing, ref int lineIndex)
        {
            position.X = 0;
            position.Y += lineSpacing;
            lineSpacing = CurrentFont().LineSpacing;

            lineIndex++;
        }
    }
}
