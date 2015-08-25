using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game = Microsoft.Xna.Framework.Game;

namespace Myre.UI.Text
{
    public class ContentParser<T>
    {
        private readonly ContentManager _content;
        private readonly Dictionary<StringPart, T> _items;

        public Dictionary<StringPart, T> Items
        {
            get { return _items; }
        }

        public ContentParser(Game game, string contentDirectory)
            : this(new ContentManager(game.Services, contentDirectory))
        {
        }

        public ContentParser(ContentManager content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            _content = content;
            _items = new Dictionary<StringPart, T>();
        }

        public bool TryParse(StringPart name, out T item)
        {
            if (_items.TryGetValue(name, out item))
                return true;

            if (TryLoad(name, out item))
                return true;

            return false;
        }

        private bool TryLoad(StringPart name, out T item)
        {
            try
            {
                item = _content.Load<T>(name.ToString());
                _items.Add(name, item);
                return true;
            }
            catch
            {
                item = default(T);
                return false;
            }
        }
    }
}
