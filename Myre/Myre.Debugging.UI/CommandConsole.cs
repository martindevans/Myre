// TODO: CommandConsole cannot be closed when there are no other controls

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.UI;
using Myre.UI.Controls;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;
using Myre.UI.Text;

namespace Myre.Debugging.UI
{
    public class CommandConsole
        : Control
    {
        readonly CommandEngine _engine;
        readonly TextLog _log;
        readonly TextBox _textBox;
        readonly Label _tabCompletion;
        readonly Label _infoBox;
        CommandHelp? _help;
        readonly Texture2D _background;
        readonly CommandStack _commandStack;

        /// <summary>
        /// Gets the command engine.
        /// </summary>
        /// <value>The engine.</value>
        public CommandEngine Engine { get { return _engine; } }

        /// <summary>
        /// Gets or sets the key used to toggle this <see cref="CommandConsole"/>.
        /// </summary>
        public Keys ToggleKey { get; set; }

        private readonly TextWriter _writer;
        public TextWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConsole"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="font">The font.</param>
        public CommandConsole(Game game, SpriteFont font)
            : this(game, font, Assembly.GetCallingAssembly())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConsole"/> class.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="parent"></param>
        /// <param name="game"></param>
        public CommandConsole(Game game, SpriteFont font, Control parent)
            : this(game, font, parent, Assembly.GetCallingAssembly())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConsole"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="font"></param>
        /// <param name="assemblies">The assemblies containing commands and options to add to this <see cref="CommandConsole"/> instance.</param>
        public CommandConsole(Game game, SpriteFont font, params Assembly[] assemblies)
            : this(game, font, CreateUserInterface(game), assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConsole"/> class.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="parent">The parent.</param>
        /// <param name="assemblies">The assemblies containing commands and options to add to this <see cref="CommandConsole"/> instance.</param>
        /// <param name="game"></param>
        public CommandConsole(Game game, SpriteFont font, Control parent, params Assembly[] assemblies)
            : base(parent)
        {
            _engine = new CommandEngine(assemblies);
            _writer = new ConsoleWriter(this);

            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;
            SetSize(0, pp.BackBufferHeight / 3);
            SetPoint(Points.Top, 0, 5);
            SetPoint(Points.Left, 5, 0);
            SetPoint(Points.Right, -5, 0);
            Strata = new ControlStrata() { Layer = Layer.Overlay, Offset = 100 };
            FocusPriority = int.MaxValue;
            LikesHavingFocus = false;
            IsVisible = false;
            RespectSafeArea = true;
            ToggleKey = Keys.Oem8;

            //var font = Content.Load<SpriteFont>(game, "Consolas");
            //skin = Content.Load<Skin>(game, "Console");
            //skin.BackgroundColour = new Color(1f, 1f, 1f, 0.8f);
            _background = new Texture2D(game.GraphicsDevice, 1, 1);
            _background.SetData(new Color[] { Color.Black });

            _textBox = new TextBox(this, game, font, "Command Console", "Enter your command");
            _textBox.SetPoint(Points.Bottom, 0, -3);
            _textBox.SetPoint(Points.Left, 3, 0);
            _textBox.SetPoint(Points.Right, -3, 0);
            _textBox.FocusPriority = 1;
            _textBox.FocusedChanged += c => { if (c.IsFocused) _textBox.BeginTyping(PlayerIndex.One); };
            _textBox.IgnoredCharacters.Add('`');

            _log = new TextLog(this, font, (int)(3 * Area.Height / (float)font.LineSpacing));
            _log.SetPoint(Points.TopLeft, 3, 3);
            _log.SetPoint(Points.TopRight, -3, 3);
            _log.SetPoint(Points.Bottom, 0, 0, _textBox, Points.Top);
            _log.WriteLine("Hello world");

            _tabCompletion = new Label(this, font);
            _tabCompletion.SetSize(300, 0);
            _tabCompletion.SetPoint(Points.TopLeft, 3, 6, this, Points.BottomLeft);

            _infoBox = new Label(this, font);
            _infoBox.SetPoint(Points.TopRight, -3, 6, this, Points.BottomRight);

            AreaChanged += c => _infoBox.SetSize(Math.Max(0, c.Area.Width - 311), 0);

            _commandStack = new CommandStack(_textBox, Gestures);

            BindGestures();

            Gestures.BlockedDevices.Add(typeof(KeyboardDevice));
        }
        #endregion

        private static Control CreateUserInterface(Game game)
        {
            UserInterface ui = new UserInterface(game.GraphicsDevice) { DrawOrder = int.MaxValue };
            game.Components.Add(ui);

            var player = new InputActor(1) { new KeyboardDevice(PlayerIndex.One) };
            game.Components.Add(player);
            ui.Actors.Add(player);

            return ui.Root;
        }

        /// <summary>
        /// Writes a line to the consosle.
        /// </summary>
        /// <param name="item">The item.</param>
        public void WriteLine(object item)
        {
            _log.WriteLine((item ?? "null").ToString());
        }

        /// <summary>
        /// Appends the object to the end of the last line.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Write(object item)
        {
            _log.Write((item ?? "null").ToString());
        }

        protected void BindGestures()
        {
            Gestures.Bind(delegate(IGesture gesture, GameTime gameTime, IInputDevice device)
            {
                if (_textBox.Text.Length > 0)
                {
                    _commandStack.PushCommand(_textBox.Text);

                    _log.WriteLine(">" + _textBox.Text);
                    var result = _engine.Execute(_textBox.Text);
                    if (result.Result != null)
                        _log.WriteLine(result.Result.ToString());
                    else if (result.Error != null)
                        _log.WriteLine(result.Error);
                }
                _textBox.Text = "";
            }, new KeyPressed(Keys.Enter));

            Gestures.Bind((GestureHandler<IGesture>)OnAutocomplete, new KeyPressed(Keys.Tab));
        }

        private void OnAutocomplete(IGesture gesture, GameTime gameTime, IInputDevice device)
        {
            if (_help.HasValue && _help.Value.PossibleCommands.Length > 0)
            {
                var h = _help.Value;
                string similarity = "";

                int letter = 0;
                bool b = true;
                while (true)
                {
                    char c = ' ';
                    for (int i = 0; i < _help.Value.PossibleCommands.Length; i++)
                    {
                        if (letter >= _help.Value.PossibleCommands[i].Length)
                        {
                            b = false;
                            break;
                        }

                        if (c == ' ')
                            c = _help.Value.PossibleCommands[i][letter];
                        else
                        {
                            if (c != _help.Value.PossibleCommands[i][letter])
                            {
                                b = false;
                                break;
                            }
                        }
                    }
                    if (!b)
                        break;

                    similarity += c;
                    letter++;
                }

                _textBox.Text = h.Command.Substring(0, h.TabStart) + similarity;
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            //skin.Draw(batch, Area, Color.White);
            var colour = new Color(0.75f,0.75f, 0.75f, 0.75f);
            batch.Draw(_background, Area, colour);

            if (_tabCompletion.IsVisible)
            {
                batch.Draw(
                    _background,
                    new Rectangle(
                        _tabCompletion.Area.X - 3, _tabCompletion.Area.Y - 3,
                        _tabCompletion.Area.Width + 6, _tabCompletion.Area.Height + 6),
                    colour);
                //skin.Draw(batch, new Rectangle(
                //    tabCompletion.Area.X - 3, tabCompletion.Area.Y - 3,
                //    tabCompletion.Area.Width + 6, tabCompletion.Area.Height + 6),
                //    Color.White);
            }

            if (_infoBox.IsVisible)
            {
                batch.Draw(
                    _background,
                    new Rectangle(
                        _infoBox.Area.X - 3, _infoBox.Area.Y - 3,
                        _infoBox.Area.Width + 6, _infoBox.Area.Height + 6),
                    colour);
                //skin.Draw(batch, new Rectangle(
                //    infoBox.Area.X - 3, infoBox.Area.Y - 3,
                //    infoBox.Area.Width + 6, infoBox.Area.Height + 6),
                //    Color.White);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardDevice keyboard = null;
            foreach (var actor in UserInterface.Actors)
            {
                keyboard = actor.FindDevice<KeyboardDevice>();
                if (keyboard != null)
                    break;
            }

            if (keyboard != null && keyboard.IsKeyNewlyDown(ToggleKey))
            {
                if (IsVisible)
                {
                    RestorePreviousFocus();
                    //try { RestorePreviousFocus(); }
                    //catch { if (Parent != null) Parent.Focus(); }
                }
                else Focus();
            }

            if (!IsFocused)
            {
                IsVisible = false;
                return;
            }

            if (!_help.HasValue || _textBox.Text != _help.Value.Command)
            {
                _help = _engine.GetHelp(_textBox.Text);
                StringBuilder tab = new StringBuilder();
                foreach (var item in _help.Value.PossibleCommands.OrderBy(s => s))
                    tab.AppendLine(item);
                _tabCompletion.Text = tab.ToString(0, Math.Max(0, tab.Length - 1));
                _tabCompletion.IsVisible = !_tabCompletion.Text.Equals((StringPart)"");

                _infoBox.Text = string.Format("[c:200:200:200]{0}[/c]{1}",
                    _help.Value.Definitions,
                    _help.Value.Description);
                _infoBox.IsVisible = !string.IsNullOrEmpty(_help.Value.Definitions) || !string.IsNullOrEmpty(_help.Value.Description);
            }
        }

        private void RestorePreviousFocus()
        {
            List<ActorFocus> buffer = new List<ActorFocus>(FocusedBy);
            foreach (var record in buffer)
                record.Restore();
        }

        private class CommandStack
        {
            readonly LinkedList<String> _previousCommands = new LinkedList<string>();
            LinkedListNode<String> _commandScrollPointer = null;

            private readonly TextBox _textBox;

            public CommandStack(TextBox textBox, GestureGroup gestures)
            {
                if (textBox == null)
                    throw new NullReferenceException("textBox");

                _textBox = textBox;

                gestures.Bind((GestureHandler<IGesture>) OnPreviousCommand, new KeyPressed(Keys.Up));
                gestures.Bind((GestureHandler<IGesture>) OnNextCommand, new KeyPressed(Keys.Down));
            }

            private void OnPreviousCommand(IGesture gesture, GameTime gameTime, IInputDevice device)
            {
                if (_commandScrollPointer == null)
                {
                    _commandScrollPointer = _previousCommands.Last;
                    WritePreviousCommand();
                }
                else if (_commandScrollPointer.Previous != null)
                {
                    _commandScrollPointer = _commandScrollPointer.Previous;
                    WritePreviousCommand();
                }
            }

            private void OnNextCommand(IGesture gesture, GameTime gameTime, IInputDevice device)
            {
                if (_commandScrollPointer != null)
                {
                    if (_commandScrollPointer.Next != null)
                    {
                        _commandScrollPointer = _commandScrollPointer.Next;
                        WritePreviousCommand();
                    }
                    else
                    {
                        _textBox.Text = "";
                        _commandScrollPointer = null;
                    }
                }
            }

            public void PushCommand(String s)
            {
                _previousCommands.AddLast(_textBox.Text);
                _commandScrollPointer = null;
            }

            private void WritePreviousCommand()
            {
                if (_commandScrollPointer != null)
                    _textBox.Text = _commandScrollPointer.Value;
            }
        }
    }
}