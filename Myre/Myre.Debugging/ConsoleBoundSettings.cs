using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Debugging;
using System.Reflection;
using Myre.Collections;
using System.Diagnostics;

namespace Myre.Debugging
{
    /// <summary>
    /// A class which mediates configuration settings between a named value store and a command engine.
    /// </summary>
    public class ConsoleBoundSettings
        :ISettingsCollection
    {
        /// <summary>
        /// A named setting
        /// </summary>
        interface ISetting
        {
            string Name { get; }
            string Description { get; }
        }

        class Setting<T>
            : ISetting
        {
            public T Value
            {
                get { return Target.Value; }
                set { Target.Value = value; }
            }

            public string Name { get; set; }
            public string Description { get; set; }
            public Box<T> Target { get; set; }
        }

        private BoxedValueStore<string> data;
        private CommandEngine engine;
        private List<ISetting> settings;

        /// <summary>
        /// The command engine bound with this Settings instance
        /// </summary>
        public CommandEngine Engine
        {
            get { return engine; }
        }

        /// <summary>
        /// Construct a new Settings instance
        /// </summary>
        /// <param name="data">Where this instance reads and writes its value to</param>
        public ConsoleBoundSettings(BoxedValueStore<string> data)
        {
            this.data = data;
            this.settings = new List<ISetting>();
        }

        /// <summary>
        /// Add a new setting
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="name">The name of the setting (what is typed into the console)</param>
        /// <param name="description">The description of the setting</param>
        /// <param name="defaultValue">The initial value this setting will contain</param>
        /// <returns>The box which this setting references</returns>
        public Box<T> Add<T>(string name, string description = null, T defaultValue = default(T))
        {
            var box = data.Get(name, defaultValue);
            var setting = new Setting<T>()
            {
                Name = name,
                Description = description,
                Target = box,
            };

            settings.Add(setting);

            if (engine != null)
            {
                engine.RemoveOption(name);
                engine.AddOption(setting, "Value", name, description);
            }

            return box;
        }

        /// <summary>
        /// Bind a new command engine to this settings instance
        /// </summary>
        /// <param name="engine"></param>
        public void BindCommandEngine(CommandEngine engine)
        {
            if (this.engine == engine)
                return;

            if (this.engine != null)
            {
                foreach (var item in settings)
                    this.engine.RemoveCommand(item.Name);
            }

            this.engine = engine;
            if (engine != null)
            {
                foreach (var item in settings)
                {
                    engine.RemoveOption(item.Name);
                    engine.AddOption(item, "Value", item.Name, item.Description);
                }
            }
        }
    }
}
