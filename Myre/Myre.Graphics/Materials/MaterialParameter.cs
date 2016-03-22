using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Myre.Graphics.Materials
{
    internal struct ParameterType
    {
        public readonly EffectParameterType Type;
        public readonly int Rows;
        public readonly int Columns;

        public ParameterType(EffectParameterType type, int columns, int rows)
        {
            Type = type;
            Rows = rows;
            Columns = columns;
        }

        public override bool Equals(object obj)
        {
            return obj is ParameterType && Equals((ParameterType)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ Rows;
                hashCode = (hashCode * 397) ^ Columns;
                return hashCode;
            }
        }

        public bool Equals(ParameterType obj)
        {
            return Type == obj.Type
                && Rows == obj.Rows
                && Columns == obj.Columns;
        }
    }

    public class MaterialParameter
    {
        #region Type Mappings

        private static readonly Dictionary<string, Type> _setterTypeMappings = new Dictionary<string, Type>
        {
            { typeof(Boolean).Name,   typeof(BooleanMaterialParameterSetter)      },
            { typeof(Texture2D).Name, typeof(Texture2DMaterialParameterSetter)    },
            { typeof(Int32).Name,     typeof(Int32MaterialParameterSetter)        },
            { typeof(Single).Name,    typeof(SingleMaterialParameterSetter)       },
            { typeof(Vector2).Name,   typeof(Vector2MaterialParameterSetter)      },
            { typeof(Vector3).Name,   typeof(Vector3MaterialParameterSetter)      },
            { typeof(Vector4).Name,   typeof(Vector4MaterialParameterSetter)      },
            { typeof(Matrix4x4).Name, typeof(Matrix4X4MaterialParameterSetter)    },
            { typeof(String).Name,    typeof(StringMaterialParameterSetter)       },
            { typeof(Boolean[]).Name, typeof(BooleanArrayMaterialParameterSetter) },
            { typeof(Int32[]).Name,   typeof(Int32ArrayMaterialParameterSetter)   },
            { typeof(Single[]).Name,  typeof(SingleArrayMaterialParameterSetter)  },
            { typeof(Vector2[]).Name, typeof(Vector2ArrayMaterialParameterSetter) },
            { typeof(Vector3[]).Name, typeof(Vector3ArrayMaterialParameterSetter) },
            { typeof(Vector4[]).Name, typeof(Vector4ArrayMaterialParameterSetter) },
            { typeof(Matrix4x4[]).Name,  typeof(Matrix4X4ArrayMaterialParameterSetter)  },
        };

        internal static readonly Dictionary<ParameterType, Type> ParameterTypeMappings = new Dictionary<ParameterType, Type>()
        {
            { new ParameterType(EffectParameterType.Bool, 1, 1 ), typeof(Boolean)   },
            { new ParameterType(EffectParameterType.Texture, 0, 0), typeof(Texture2D) },
            { new ParameterType(EffectParameterType.Int32, 1, 1), typeof(Int32)     },
            { new ParameterType(EffectParameterType.Single, 1, 1), typeof(Single)    },
            { new ParameterType(EffectParameterType.Single, 2, 1), typeof(Vector2)   },
            { new ParameterType(EffectParameterType.Single, 3, 1), typeof(Vector3)   },
            { new ParameterType(EffectParameterType.Single, 4, 1), typeof(Vector4)   },
            { new ParameterType(EffectParameterType.Single, 4, 4), typeof(Matrix4x4)    },
            { new ParameterType(EffectParameterType.String, 0, 0), typeof(String)    }
        };
        #endregion

// ReSharper disable NotAccessedField.Local
        private EffectParameter _parameter;
// ReSharper restore NotAccessedField.Local
        private readonly MaterialParameterSetter _setter;

        public MaterialParameter(EffectParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            if (string.IsNullOrEmpty(parameter.Semantic))
                throw new ArgumentException("Material parameters must have a semantic");

            _parameter = parameter;
            _setter = CreateSetter(parameter);
        }

        public void Apply(NamedBoxCollection data)
        {
            if (_setter != null)
                _setter.Apply(data);
        }

        private static MaterialParameterSetter CreateSetter(EffectParameter parameter)
        {
            if (string.IsNullOrEmpty(parameter.Semantic))
                return null;

            var parameterType = new ParameterType(parameter.ParameterType, parameter.ColumnCount, parameter.RowCount);
            Type type;
            if (!ParameterTypeMappings.TryGetValue(parameterType, out type))
                throw new InvalidOperationException(string.Format("An automatic setter could not be created for the Material parameter \"{0}\", with semantic \"{1}\".", parameter.Name, parameter.Semantic));

            var typeName = type.Name;

            if (parameter.Elements.Count > 0)
                typeName += "[]";

            Type setterType;
            if (_setterTypeMappings.TryGetValue(typeName, out setterType))
                return (MaterialParameterSetter)Activator.CreateInstance(setterType, parameter);
            else
                return null;
        }
    }

    internal abstract class MaterialParameterSetter
    {
        public abstract void Apply(NamedBoxCollection globals);
    }

    internal abstract class BaseMaterialParameterSetter<T>
        : MaterialParameterSetter
    {
        private readonly EffectParameter _parameter;
        private readonly TypedName<T> _semanticName;

        private Box<T> _box;
        private NamedBoxCollection _previousGlobals;

        protected BaseMaterialParameterSetter(EffectParameter parameter)
        {
            Contract.Requires(parameter != null);

            _parameter = parameter;
            _semanticName = new TypedName<T>(parameter.Semantic.ToLower());
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_parameter != null);
        }

        public override void Apply(NamedBoxCollection globals)
        {
            //If the box is null, or the globals collection has changed, get the box
            if (_box == null || _previousGlobals != globals)
            {
                globals.TryGet(_semanticName, out _box);
                _previousGlobals = globals;
            }

            //Set the value into the box
            if (_box != null)
                Set(_parameter, _box);
        }

        protected abstract void Set(EffectParameter parameter, Box<T> value);
    }

    #region SetterGenerator
//    internal static class ParameterSetterGenerator
//    {
//        private const string CLASS_TEMPLATE = @"
//    class [ClassPrefix]MaterialParameterSetter
//        : MaterialParameterSetter
//    {
//        private Box<[Type]> value;
//        private NamedBoxCollection previousGlobals;
//
//        public [ClassPrefix]MaterialParameterSetter(EffectParameter parameter)
//            : base(parameter)
//        {
//        }
//
//        public override void Apply(NamedBoxCollection globals)
//        {
//            if (value == null || previousGlobals != globals)
//            {
//                globals.TryGet(Semantic, out value);
//                previousGlobals = globals;
//            }
//
//            if (value != null)
//                Parameter.SetValue(value.Value);
//        }
//    }";

//        private const string INITIALISATION_TEMPLATE = "{ typeof([Type]).Name, typeof([ClassPrefix]MaterialParameterSetter) },";

//        public static string Generate()
//        {
//            StringBuilder output = new StringBuilder();
//            StringBuilder initialisation = new StringBuilder();

//            var effectParameterType = typeof(EffectParameter);
//            var types = from type in MaterialParameter.ParameterTypeMappings.Values
//                        select type.Name;

//            var parameters = (from method in effectParameterType.GetMethods()
//                              where method.Name == "SetValue"
//                              select (from parameter in method.GetParameters()
//                                      select parameter.ParameterType.Name))
//                              .Flatten();

//            var arrays = from type in types
//                         let typeArray = type + "[]"
//                         where parameters.Contains(typeArray)
//                         select typeArray;

//            foreach (var type in types.Union(arrays))
//            {
//                var classDefinition = CLASS_TEMPLATE.Replace("[ClassPrefix]", type.Replace("[]", "Array")).Replace("[Type]", type);
//                output.AppendLine(classDefinition);

//                var initialisationLine = INITIALISATION_TEMPLATE.Replace("[ClassPrefix]", type.Replace("[]", "Array")).Replace("[Type]", type);
//                initialisation.AppendLine(initialisationLine);
//            }

//            output.AppendLine(initialisation.ToString());

//            return output.ToString();
//        }
//    }
    #endregion

    #region Setters
    internal class BooleanMaterialParameterSetter
        : BaseMaterialParameterSetter<bool>
    {
        public BooleanMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<bool> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Texture2DMaterialParameterSetter
        : BaseMaterialParameterSetter<Texture2D>
    {
        public Texture2DMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Texture2D> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Int32MaterialParameterSetter
        : BaseMaterialParameterSetter<int>
    {
        public Int32MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<int> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class SingleMaterialParameterSetter
        : BaseMaterialParameterSetter<float>
    {
        public SingleMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<float> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Vector2MaterialParameterSetter
        : BaseMaterialParameterSetter<Vector2>
    {
        public Vector2MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Vector2> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Vector3MaterialParameterSetter
        : BaseMaterialParameterSetter<Vector3>
    {
        public Vector3MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Vector3> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Vector4MaterialParameterSetter
        : BaseMaterialParameterSetter<Vector4>
    {
        public Vector4MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Vector4> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Matrix4X4MaterialParameterSetter
        : BaseMaterialParameterSetter<Matrix4x4>
    {
        public Matrix4X4MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Matrix4x4> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class StringMaterialParameterSetter
        : BaseMaterialParameterSetter<string>
    {
        public StringMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<string> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class BooleanArrayMaterialParameterSetter
        : BaseMaterialParameterSetter<bool[]>
    {
        public BooleanArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<bool[]> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Int32ArrayMaterialParameterSetter
        : BaseMaterialParameterSetter<int[]>
    {
        public Int32ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<int[]> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class SingleArrayMaterialParameterSetter
        : BaseMaterialParameterSetter<float[]>
    {
        public SingleArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<float[]> value)
        {
            parameter.SetValue(value.Value);
        }
    }

    internal class Vector2ArrayMaterialParameterSetter
        : BaseMaterialParameterSetter<Vector2[]>
    {
        private Microsoft.Xna.Framework.Vector2[] _conversion;

        public Vector2ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Vector2[]> value)
        {
            //Create an array to convert into (or reuse existing one)
            if (_conversion == null || _conversion.Length != value.Value.Length)
                _conversion = new Microsoft.Xna.Framework.Vector2[value.Value.Length];

            //Convert to XNA types
            for (int i = 0; i < value.Value.Length; i++)
                _conversion[i] = value.Value[i].ToXNA();

            parameter.SetValue(_conversion);
        }
    }

    internal class Vector3ArrayMaterialParameterSetter
        : BaseMaterialParameterSetter<Vector3[]>
    {
        private Microsoft.Xna.Framework.Vector3[] _conversion;

        public Vector3ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Vector3[]> value)
        {
            //Create an array to convert into (or reuse existing one)
            if (_conversion == null || _conversion.Length != value.Value.Length)
                _conversion = new Microsoft.Xna.Framework.Vector3[value.Value.Length];

            //Convert to XNA types
            for (int i = 0; i < value.Value.Length; i++)
                _conversion[i] = value.Value[i].ToXNA();

            parameter.SetValue(_conversion);
        }
    }

    internal class Vector4ArrayMaterialParameterSetter
        : BaseMaterialParameterSetter<Vector4[]>
    {
        private Microsoft.Xna.Framework.Vector4[] _conversion;

        public Vector4ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Vector4[]> value)
        {
            //Create an array to convert into (or reuse existing one)
            if (_conversion == null || _conversion.Length != value.Value.Length)
                _conversion = new Microsoft.Xna.Framework.Vector4[value.Value.Length];

            //Convert to XNA types
            for (int i = 0; i < value.Value.Length; i++)
                _conversion[i] = value.Value[i].ToXNA();

            parameter.SetValue(_conversion);
        }
    }

    internal class Matrix4X4ArrayMaterialParameterSetter
        : BaseMaterialParameterSetter<Matrix4x4[]>
    {
        private Microsoft.Xna.Framework.Matrix[] _conversion;

        public Matrix4X4ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        protected override void Set(EffectParameter parameter, Box<Matrix4x4[]> value)
        {
            //Create an array to convert into (or reuse existing one)
            if (_conversion == null || _conversion.Length != value.Value.Length)
                _conversion = new Microsoft.Xna.Framework.Matrix[value.Value.Length];

            //Convert to XNA types
            for (int i = 0; i < value.Value.Length; i++)
                _conversion[i] = value.Value[i].ToXNA();

            parameter.SetValue(_conversion);
        }
    }
#endregion
}
