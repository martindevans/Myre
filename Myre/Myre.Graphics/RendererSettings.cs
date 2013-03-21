using System.Collections.Generic;
using Myre.Collections;
using Myre.Debugging;

namespace Myre.Graphics
{
    public class RendererSettings
    {
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
            public Box<T> Target { get; set; }
        }

        private readonly Renderer _renderer;
        private CommandEngine _engine;
        private readonly List<ISetting> _settings;

        public CommandEngine Engine
        {
            get { return _engine; }
        }

        public RendererSettings(Renderer renderer)
        {
            _renderer = renderer;
            _settings = new List<ISetting>();
        }

// ReSharper disable UnusedMethodReturnValue.Global
        public Box<T> Add<T>(string name, string description = null, T defaultValue = default(T))
// ReSharper restore UnusedMethodReturnValue.Global
        {
            var box = _renderer.Data.Get(name, defaultValue);
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

        public void BindCommandEngine(CommandEngine commandEngine)
        {
            if (_engine == commandEngine)
                return;

            if (_engine != null)
            {
                foreach (var item in _settings)
                    _engine.RemoveCommand(item.Name);
            }

            _engine = commandEngine;
            if (commandEngine != null)
            {
                foreach (var item in _settings)
                {
                    commandEngine.RemoveOption(item.Name);
                    commandEngine.AddOption(item, "Value", item.Name, item.Description);
                }
            }
        }
    }
}
