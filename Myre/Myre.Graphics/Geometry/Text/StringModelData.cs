using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;
using Myre.Graphics.Materials;
using System.Collections.Generic;
using System.Linq;

namespace Myre.Graphics.Geometry.Text
{
    public class StringModelData
        : Behaviour
    {
        public static readonly TypedName<string> StringName = new TypedName<string>("string");
        public static readonly TypedName<VertexFont> FontName = new TypedName<VertexFont>("font");
        public static readonly TypedName<float> ThicknessName = new TypedName<float>("thickness");

        private Property<string> _string;
        public string String
        {
            get
            {
                return _string.Value;
            }
            set
            {
                _string.Value = value;
            }
        }

        private Property<VertexFont> _font;
        public VertexFont Font
        {
            get
            {
                return _font.Value;
            }
            set
            {
                _font.Value = value;
            }
        }

        private Property<float> _thickness;
        public float Thickness
        {
            get
            {
                return _thickness.Value;
            }
            set
            {
                _thickness.Value = value;
            }
        }

        /// <summary>
        /// Cache of characters currently in use in the model
        /// </summary>
        private IDictionary<char, List<Mesh>> _scratchPad = new Dictionary<char, List<Mesh>>();

        /// <summary>
        /// Cache of characters currently in use in the model
        /// </summary>
        private IDictionary<char, List<Mesh>> _characterCache = new Dictionary<char, List<Mesh>>();

        private Property<ModelData> _model;

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            base.CreateProperties(context);

            _string = context.CreateProperty(StringName, "");
            _font = context.CreateProperty(FontName);
            _model = context.CreateProperty(ModelInstance.ModelName);
            _thickness = context.CreateProperty(ThicknessName);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(FontName, _font);
            initialisationData.TryCopyValue(StringName, _string);
            initialisationData.TryCopyValue(ThicknessName, _thickness);

            _string.PropertySet += StringChanged;
            _font.PropertySet += FontChanged;
        }

        protected override void Initialised()
        {
            base.Initialised();

            Rebuild();
        }

        private void FontChanged(Property<VertexFont> property, VertexFont oldvalue, VertexFont newvalue)
        {
            if (oldvalue == newvalue)
                return;

            Rebuild();
        }

        private void StringChanged(Property<string> property, string oldvalue, string newvalue)
        {
            if (oldvalue == newvalue)
                return;

            Rebuild();
        }

        private void Rebuild()
        {
            if (_model.Value == null)
                _model.Value = new ModelData(new Mesh[0]);

            BuildString(_string.Value);
        }

        private void BuildString(string str)
        {
            //Remove all character meshes from the model
            _model.Value.Clear();

            //Get the characters which make up this string
            var characters = GetCharacters(str);

            var pen = 0f;
            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters[i];

                //Get the cache of characters which we used last time
                List<Mesh> cache;
                _characterCache.TryGetValue(character.Character, out cache);

                //Create a new mesh, or use an existing one
                Mesh m;
                if (cache == null || cache.Count == 0)
                    m = CloneMesh(character.Mesh);
                else
                {
                    m = cache[cache.Count - 1];
                    cache.RemoveAt(cache.Count - 1);
                }

                //Add mesh into scratchpad cache
                List<Mesh> cache2;
                if (!_scratchPad.TryGetValue(character.Character, out cache2))
                {
                    cache2 = new List<Mesh>();
                    _scratchPad.Add(character.Character, cache2);
                }
                cache2.Add(m);

                //Move mesh into position
                m.MeshTransform = Matrix.CreateScale(1, _thickness.Value, 1) * Matrix.CreateTranslation(pen, 0, 0);
                _model.Value.Add(m);

                //Update pen position
                pen += character.Width;

                //If there is a following character move pen by kerning distance between this character pair
                if (i != str.Length - 1)
                    pen += character.GetKern(str[i + 1]);
            }

            //Swap scratchpad and character cache
            var tmp = _scratchPad;
            _scratchPad = _characterCache;
            _characterCache = tmp;

            //Clean up caches
            CleanCache(_scratchPad, true);
            CleanCache(_characterCache, false);
        }

        private readonly List<char> _cleanup = new List<char>();
        private void CleanCache(IDictionary<char, List<Mesh>> cache, bool empty)
        {
            if (empty)
            {
                //Discard everything in this cache
                foreach (var mesh in cache.SelectMany(item => item.Value))
                {
                    //We're sharing buffers, so disposing them now would be very bad! Set them to null to make sure they're not disposed
                    mesh.IndexBuffer = null;
                    mesh.VertexBuffer = null;

                    //Dispose the mesh
                    mesh.Dispose();
                }

                cache.Clear();
            }
            else
            {
                //Find all keys with an empty cache
                foreach (var item in cache)
                    if (item.Value.Count == 0)
                        _cleanup.Add(item.Key);

                //Remove all the useless keys
                foreach (var character in _cleanup)
                    cache.Remove(character);

                _cleanup.Clear();
            }
        }

        private Mesh CloneMesh(Mesh m)
        {
            return new Mesh 
            {
                BaseVertex = m.BaseVertex,
                BoundingSphere = m.BoundingSphere,
                IndexBuffer = m.IndexBuffer,
                Materials = m.Materials,  //todo: Do I need to clone the materials, or can they be shared?
                MeshTransform = m.MeshTransform,
                MinVertexIndex = m.MinVertexIndex,
                Name = m.Name,
                StartIndex = m.StartIndex,
                TriangleCount = m.TriangleCount,
                VertexBuffer = m.VertexBuffer,
                VertexCount = m.VertexCount
            };
        }

        private VertexCharacter[] GetCharacters(string str)
        {
            return str.Select(a => _font.Value.GetCharacter(a)).ToArray();
        }
    }
}
