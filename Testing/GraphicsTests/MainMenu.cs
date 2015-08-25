using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.Debugging.UI;
using Myre.StateManagement;
using Myre.UI;
using Myre.UI.Controls;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;
using Myre.UI.Text;
using Ninject;
using System;
using System.Linq;
using System.Reflection;

using Color = Microsoft.Xna.Framework.Color;
using Game = Microsoft.Xna.Framework.Game;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests
{
    class MainMenu
        : Screen
    {
        private readonly UserInterface _ui;
        private readonly InputActor _player;
        private readonly Menu _menu;
        private readonly TestGame _game;

        public MainMenu(TestGame game, CommandConsole console, GraphicsDevice device, ContentManager content, IServiceProvider services)
        {
            _game = game;
            _player = game.Player;

            _ui = new UserInterface(device);
            _ui.Actors.Add(_player);

            var tests = from type in Assembly.GetExecutingAssembly().GetTypes()
                        where typeof(TestScreen).IsAssignableFrom(type)
                        where !type.IsAbstract
                        select type;

            _menu = new Menu(_ui.Root);
            _menu.SetPoint(Points.BottomLeft, 50, -50);

            foreach (var test in tests)
            {
                var testKernel = new StandardKernel();
                testKernel.Bind<GraphicsDevice>().ToConstant(device);
                testKernel.Bind<ContentManager>().ToConstant(new ContentManager(services));
                testKernel.Bind<Game>().ToConstant(game);
                testKernel.Bind<TestGame>().ToConstant(game);
                testKernel.Bind<CommandConsole>().ToConstant(console);
                testKernel.Bind<IServiceProvider>().ToConstant(game.Services);
                //testKernel.Bind<InputActor>().ToConstant(player);

                var instance = (TestScreen)testKernel.Get(test);

                var menuOption = new TextButton(_menu, content.Load<SpriteFont>("Consolas"), instance.Name) {
                    Highlight = Color.Red
                };
                menuOption.Gestures.Bind((gesture, time, input) => Manager.Push(instance), new MouseReleased(MouseButtons.Left), new KeyReleased(Keys.Enter));
            }

            var quit = new TextButton(_menu, content.Load<SpriteFont>("Consolas"), "Exit") {
                Highlight = Color.Red
            };
            quit.Gestures.Bind((gesture, time, input) => game.Exit(), new MouseReleased(MouseButtons.Left), new KeyReleased(Keys.Enter));

            _menu.Arrange(Justification.Left);
        }

        protected override void BeginTransitionOn()
        {
            _game.DisplayUI = true;
            _player.Focus(_menu);
            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            _ui.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_game.DisplayUI)
                _ui.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
