using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.StateManagement;
using Myre.UI;
using Microsoft.Xna.Framework;
using Myre.UI.Controls;
using Microsoft.Xna.Framework.Content;
using Ninject;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Text;
using Myre.UI.Gestures;
using Microsoft.Xna.Framework.Input;
using Myre.UI.InputDevices;

namespace GraphicsTests
{
    abstract class TestScreen
        : Screen
    {
        private TestGame game;
        private ContentManager content;
        private InputActor actor;

        public string Name { get; private set; }
        public UserInterface UI { get; private set; }

        public TestScreen(string name, IKernel kernel)
        {
            Name = name;
            game = kernel.Get<TestGame>();

            content = kernel.Get<ContentManager>();
            content.RootDirectory = "Content";

            UI = kernel.Get<UserInterface>();
            UI.Root.Gestures.Bind((gesture, time, device) => Manager.Pop(), new KeyReleased(Keys.Escape));

            actor = game.Player;
            UI.Actors.Add(actor);

            var title = new Label(UI.Root, content.Load<SpriteFont>("Consolas"));
            title.Text = Name;
            title.Justification = Justification.Centre;
            title.SetPoint(Points.Top, Int2D.Zero);
        }

        protected override void BeginTransitionOn()
        {
            actor.Focus(UI.Root);

            //game.IsFixedTimeStep = false;
            game.DisplayUI = true;

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
            if (game.DisplayUI)
                UI.Draw(gameTime);

            base.Draw(gameTime);
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            content.Unload();
        }
    }
}
