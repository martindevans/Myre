using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.StateManagement;
using Myre.UI;
using Myre.UI.Controls;
using Myre.UI.Gestures;
using Myre.UI.Text;
using Ninject;

using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests
{
    public abstract class TestScreen
        : Screen
    {
        protected readonly TestGame Game;
        private readonly ContentManager _content;
        private readonly InputActor _actor;

        public string Name { get; private set; }
        public UserInterface UI { get; private set; }

        public TestScreen(string name, IKernel kernel)
        {
            Name = name;
            Game = kernel.Get<TestGame>();

            _content = kernel.Get<ContentManager>();
            _content.RootDirectory = "Content";

            UI = kernel.Get<UserInterface>();
            UI.Root.Gestures.Bind((gesture, time, device) => Manager.Pop(), new KeyReleased(Keys.Escape));

            _actor = Game.Player;
            UI.Actors.Add(_actor);

            var title = new Label(UI.Root, _content.Load<SpriteFont>("Consolas")) {
                Text = Name,
                Justification = Justification.Centre
            };
            title.SetPoint(Points.Top, Int2D.Zero);
        }

        protected override void BeginTransitionOn()
        {
            _actor.Focus(UI.Root);

            //game.IsFixedTimeStep = false;
            Game.DisplayUI = true;

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var actor in UI.Actors)
                actor.Update(gameTime);

            UI.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (Game.DisplayUI)
                UI.Draw(gameTime);

            base.Draw(gameTime);
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            _content.Unload();
        }
    }
}
