using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;

namespace Myre.Graphics.Materials
{
    public class Material
    {
        private readonly Effect _effect;
        private readonly MaterialParameter[] _parameters;

        public EffectParameterCollection Parameters
        {
            get { return _effect.Parameters; }
        }

        public EffectTechniqueCollection Techniques
        {
            get { return _effect.Techniques; }
        }

        public EffectTechnique CurrentTechnique
        {
            get { return _effect.CurrentTechnique; }
            set { _effect.CurrentTechnique = value; }
        }

        public Material(Effect effect, string techniqueName = null)
        {
            _effect = effect;
            effect.CurrentTechnique = effect.Techniques[techniqueName] ?? effect.Techniques[0];
            _parameters = (from p in effect.Parameters
                           where !string.IsNullOrEmpty(p.Semantic) //&& technique.IsParameterUsed(p) <-- why did xna 4.0 remove this?!
                           select new MaterialParameter(p)).ToArray();
        }

        public IEnumerable<EffectPass> Begin(BoxedValueStore<string> parameterValues)
        {
            for (int i = 0; i < _parameters.Length; i++)
                _parameters[i].Apply(parameterValues);

            return _effect.CurrentTechnique.Passes;
        }
    }
}
