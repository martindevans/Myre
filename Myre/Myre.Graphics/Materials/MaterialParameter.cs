using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using System.Diagnostics;
using Myre.Extensions;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Materials
{
    struct ParameterType
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
            return base.Equals(obj);
        }

        public bool Equals(ParameterType obj)
        {
            return Type == obj.Type
                && Rows == obj.Rows
                && Columns == obj.Columns;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Type.GetHashCode() ^ Rows.GetHashCode() ^ Columns.GetHashCode();
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
            { typeof(Matrix).Name,    typeof(Matrix4X4MaterialParameterSetter)    },
            { typeof(String).Name,    typeof(StringMaterialParameterSetter)       },
            { typeof(Boolean[]).Name, typeof(BooleanArrayMaterialParameterSetter) },
            { typeof(Int32[]).Name,   typeof(Int32ArrayMaterialParameterSetter)   },
            { typeof(Single[]).Name,  typeof(SingleArrayMaterialParameterSetter)  },
            { typeof(Vector2[]).Name, typeof(Vector2ArrayMaterialParameterSetter) },
            { typeof(Vector3[]).Name, typeof(Vector3ArrayMaterialParameterSetter) },
            { typeof(Vector4[]).Name, typeof(Vector4ArrayMaterialParameterSetter) },
            { typeof(Matrix[]).Name,  typeof(Matrix4X4ArrayMaterialParameterSetter)  },
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
            { new ParameterType(EffectParameterType.Single, 4, 4), typeof(Matrix)    },
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
            {
#if WINDOWS
                Trace.TraceWarning("An automatic setter could not be created for the Material parameter \"{0}\", with semantic \"{1}\".", parameter.Name, parameter.Semantic);
#endif
            }

            Debug.Assert(type != null, "type != null");
            var typeName = type.Name;

            if (parameter.Elements.Count > 0)
                typeName += "[]";

            Type setterType;
            if (_setterTypeMappings.TryGetValue(typeName, out setterType))
                return Activator.CreateInstance(setterType, parameter) as MaterialParameterSetter;
            else
                return null;
        }
    }

    abstract class MaterialParameterSetter
    {
        protected readonly EffectParameter Parameter;
        protected readonly string Semantic;

        protected MaterialParameterSetter(EffectParameter parameter)
        {
            Parameter = parameter;
            Semantic = parameter.Semantic.ToLower();
        }

        //Why does this take a BoxedValueStore<string> and not a NamedValueCollection?
        //Because I don't *want* named values, I want boxed values!

        public abstract void Apply(BoxedValueStore<string> globals);
    }

    #region SetterGenerator
    static class ParameterSetterGenerator
    {
        private const string CLASS_TEMPLATE = @"
    class [ClassPrefix]MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<[Type]> value;
        private NamedBoxCollection previousGlobals;

        public [ClassPrefix]MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }";

        private const string INITIALISATION_TEMPLATE = "{ typeof([Type]).Name, typeof([ClassPrefix]MaterialParameterSetter) },";

        public static string Generate()
        {
            StringBuilder output = new StringBuilder();
            StringBuilder initialisation = new StringBuilder();

            var effectParameterType = typeof(EffectParameter);
            var types = from type in MaterialParameter.ParameterTypeMappings.Values
                        select type.Name;

            var parameters = (from method in effectParameterType.GetMethods()
                              where method.Name == "SetValue"
                              select (from parameter in method.GetParameters()
                                      select parameter.ParameterType.Name))
                              .Flatten();

            var arrays = from type in types
                         let typeArray = type + "[]"
                         where parameters.Contains(typeArray)
                         select typeArray;

            foreach (var type in types.Union(arrays))
            {
                var classDefinition = CLASS_TEMPLATE.Replace("[ClassPrefix]", type.Replace("[]", "Array")).Replace("[Type]", type);
                output.AppendLine(classDefinition);

                var initialisationLine = INITIALISATION_TEMPLATE.Replace("[ClassPrefix]", type.Replace("[]", "Array")).Replace("[Type]", type);
                initialisation.AppendLine(initialisationLine);
            }

            output.AppendLine(initialisation.ToString());

            return output.ToString();
        }
    }
    #endregion

    #region Setters
    class BooleanMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Boolean> _value;
        private BoxedValueStore<string> _previousGlobals;

        public BooleanMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Texture2DMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Texture2D> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Texture2DMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Int32MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Int32> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Int32MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class SingleMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Single> _value;
        private BoxedValueStore<string> _previousGlobals;

        public SingleMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Vector2MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector2> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Vector2MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Vector3MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector3> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Vector3MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Vector4MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector4> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Vector4MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Matrix4X4MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Matrix> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Matrix4X4MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class StringMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<String> _value;
        private BoxedValueStore<string> _previousGlobals;

        public StringMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class BooleanArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Boolean[]> _value;
        private BoxedValueStore<string> _previousGlobals;

        public BooleanArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Int32ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Int32[]> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Int32ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class SingleArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Single[]> _value;
        private BoxedValueStore<string> _previousGlobals;

        public SingleArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Vector2ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector2[]> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Vector2ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Vector3ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector3[]> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Vector3ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Vector4ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector4[]> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Vector4ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }

    class Matrix4X4ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Matrix[]> _value;
        private BoxedValueStore<string> _previousGlobals;

        public Matrix4X4ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (_value == null || _previousGlobals != globals)
            {
                globals.TryGet(Semantic, out _value);
                _previousGlobals = globals;
            }

            if (_value != null)
                Parameter.SetValue(_value.Value);
        }
    }
#endregion
}
