using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Myre.Debugging
{
    /// <summary>
    /// An attribute which secifies that a static method may be accessed by a CommandEngine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class CommandAttribute
        : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public CommandAttribute()
        {
            Name = "";
            Description = "";
        }
    }

    struct CommandInfo
    {
        public CommandAttribute Attribute { get; set; }
        public MethodInfo Command { get; set; }
        public object Target { get; set; }
    }

    struct OptionInfo
    {
        public CommandAttribute Attribute { get; set; }
        public PropertyInfo Property { get; set; }
        public object Target { get; set; }
        public Action SetEvent;
    }
}
