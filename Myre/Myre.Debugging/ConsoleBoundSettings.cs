using System.Collections.Generic;
using Myre.Collections;

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
// ReSharper disable UnusedMember.Local
            public T Value
// ReSharper restore UnusedMember.Local
            {
                get { return Target.Value; }
                set { Target.Value = value; }
            }

            public string Name { get; set; }
            public string Description { get; set; }
            public Box<T> Target { private get; set; }
        }

        private readonly BoxedValueStore<string> _data;
        private CommandEngine _engine;
        private readonly List<ISetting> _settings;

        /// <summary>
        /// The command engine bound with this Settings instance
        /// </summary>
        public CommandEngine Engine
        {
            get { return _engine; }
        }

        /// <summary>
        /// Construct a new Settings instance
        /// </summary>
        /// <param name="data">Where this instance reads and writes its value to</param>
        public ConsoleBoundSettings(BoxedValueStore<string> data)
        {
            _data = data;
            _settings = new List<ISetting>();
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
            var box = _data.Get(name, defaultValue);
            var setting = new Setting<T>()
            {
                Name = name,
                Description = description,
                Target = box,
            };

            _settings.Add(setting);

            if (_engine != null)
            {
                _engine.RemoveOption(name);
                _engine.AddOption(setting, "Value", name, description);
            }

            return box;
        }

        /// <summary>
        /// Bind a new command engine to this settings instance
        /// </summary>
        /// <param name="engine"></param>
        public void BindCommandEngine(CommandEngine engine)
        {
            if (_engine == engine)
                return;

            if (_engine != null)
            {
                foreach (var item in _settings)
                    _engine.RemoveCommand(item.Name);
            }

            _engine = engine;
            if (engine != null)
            {
                foreach (var item in _settings)
                {
                    engine.RemoveOption(item.Name);
                    engine.AddOption(item, "Value", item.Name, item.Description);
                }
            }
        }
    }
}
