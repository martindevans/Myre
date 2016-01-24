using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;

namespace Myre.Graphics.Geometry.Text
{
    public class VertexFont
    {
        private readonly Dictionary<char, VertexCharacter> _characters;
        private readonly IReadOnlyDictionary<char, VertexCharacter> _readonlyCharacters;
        public IReadOnlyDictionary<char, VertexCharacter> Characters
        {
            get
            {
                return _readonlyCharacters;
            }
        }

        private readonly char _defaultCharacter;
        public char DefaultCharacter
        {
            get
            {
                return _defaultCharacter;
            }
        }

        public VertexFont(IEnumerable<VertexCharacter> characters, char defaultCharacter)
        {
            _characters = characters.ToDictionary(a => a.Character, a => a);
            _readonlyCharacters = new Dictionary<char, VertexCharacter>(_characters);

            _defaultCharacter = defaultCharacter;
        }

        public VertexCharacter GetCharacter(char c)
        {
            VertexCharacter vc;
            if (_characters.TryGetValue(c, out vc))
                return vc;

            return _characters[_defaultCharacter];
        }
    }

    public class VertexFontReader : ContentTypeReader<VertexFont>
    {
        protected override VertexFont Read(ContentReader input, VertexFont existingInstance)
        {
            var count = input.ReadInt32();

            var characters = new VertexCharacter[count];
            for (var i = 0; i < count; i++)
                characters[i] = input.ReadObject<VertexCharacter>();

            var defaultCharacter = input.ReadChar();

            return new VertexFont(characters, defaultCharacter);
        }
    }
}
