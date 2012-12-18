// TODO: CommandConsole cannot be closed when there are no other controls

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using Myre.UI;
using Myre.UI.Gestures;
using Myre.UI.Controls;
using Myre.Debugging;
using System.Reflection;
using Myre.UI.Text;
using Myre.UI.InputDevices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Myre.Debugging
{
    public class CommandConsole
        : Control
    {
        CommandEngine engine;
        TextLog log;
        TextBox textBox;
        Label tabCompletion;
        Label infoBox;
        CommandHelp? help;
        Texture2D background;
        CommandStack commandStack;

#if WINDOWS
        public TraceListener Listener;
#endif

        /// <summary>
        /// Gets the command engine.
        /// </summary>
        /// <value>The engine.</value>
        public CommandEngine Engine { get { return engine; } }

        /// <summary>
        /// Gets or sets the key used to toggle this <see cref="CommandConsole"/>.
        /// </summary>
        public Keys ToggleKey { get; set; }

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
        /// <param name="parent">The parent.</param>
        /// <param name="assemblies">The assemblies containing commands and options to add to this <see cref="CommandConsole"/> instance.</param>
        public CommandConsole(Game game, SpriteFont font, Control parent, params Assembly[] assemblies)
            : base(parent)
        {
            engine = new CommandEngine(assemblies);

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
            background = new Texture2D(game.GraphicsDevice, 1, 1);
            background.SetData(new Color[] { Color.Black });

            textBox = new TextBox(this, game, font, "Command Console", "Enter your command");
            textBox.SetPoint(Points.Bottom, 0, -3);
            textBox.SetPoint(Points.Left, 3, 0);
            textBox.SetPoint(Points.Right, -3, 0);
            textBox.FocusPriority = 1;
            textBox.FocusedChanged += c => { if (c.IsFocused) textBox.BeginTyping(PlayerIndex.One); };
            textBox.IgnoredCharacters.Add('`');

            log = new TextLog(this, font, (int)(3 * Area.Height / (float)font.LineSpacing));
            log.SetPoint(Points.TopLeft, 3, 3);
            log.SetPoint(Points.TopRight, -3, 3);
            log.SetPoint(Points.Bottom, 0, 0, textBox, Points.Top);
            log.WriteLine("Hello world");

            tabCompletion = new Label(this, font);
            tabCompletion.SetSize(300, 0);
            tabCompletion.SetPoint(Points.TopLeft, 3, 6, this, Points.BottomLeft);

            infoBox = new Label(this, font);
            infoBox.SetPoint(Points.TopRight, -3, 6, this, Points.BottomRight);

            AreaChanged += delegate(Frame c)
            {
                infoBox.SetSize(Math.Max(0, c.Area.Width - 311), 0);
            };

            commandStack = new CommandStack(textBox, Gestures);

#if WINDOWS
            ConsoleTraceListener cts = new ConsoleTraceListener(this);
            Listener = cts;
            Trace.Listeners.Add(cts);

            Engine.AddCommand(cts.RegexFilter, "AddFilter", "Console.Trace.AddFilter");
            Engine.AddCommand(cts.RegexFilter, "ListFilters", "Console.Trace.ListFilters");
            Engine.AddCommand(cts.RegexFilter, "RemoveAt", "Console.Trace.RemoveFilter");
#endif

            BindGestures();

            Gestures.BlockedDevices.Add(typeof(KeyboardDevice));
        }
        #endregion

        private static Control CreateUserInterface(Game game)
        {
            UserInterface ui = new UserInterface(game.GraphicsDevice);
            ui.DrawOrder = int.MaxValue;
            game.Components.Add(ui);

            var player = new InputActor(1);
            player.Add(new KeyboardDevice(PlayerIndex.One));
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
            log.WriteLine((item ?? "null").ToString());
        }

        /// <summary>
        /// Appends the object to the end of the last line.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Write(object item)
        {
            log.Write((item ?? "null").ToString());
        }

        protected void BindGestures()
        {
            Gestures.Bind(delegate(IGesture gesture, GameTime gameTime, IInputDevice device)
            {
                if (textBox.Text.Length > 0)
                {
                    commandStack.PushCommand(textBox.Text);

                    log.WriteLine(">" + textBox.Text);
                    var result = engine.Execute(textBox.Text);
                    if (result.Result != null)
                        log.WriteLine(result.Result.ToString());
                    else if (result.Error != null)
                        log.WriteLine(result.Error);
                }
                textBox.Text = "";
            }, new KeyPressed(Keys.Enter));

            Gestures.Bind(OnAutocomplete, new KeyPressed(Keys.Tab));
        }

        private void OnAutocomplete(IGesture gesture, GameTime gameTime, IInputDevice device)
        {
            if (help.HasValue && help.Value.PossibleCommands.Length > 0)
            {
                var h = help.Value;
                string similarity = "";

                int letter = 0;
                bool b = true;
                while (b)
                {
                    char c = ' ';
                    for (int i = 0; i < help.Value.PossibleCommands.Length; i++)
                    {
                        if (letter >= help.Value.PossibleCommands[i].Length)
                        {
                            b = false;
                            break;
                        }
                        else if (c == ' ')
                            c = help.Value.PossibleCommands[i][letter];
                        else
                        {
                            if (c != help.Value.PossibleCommands[i][letter])
                            {
                                b = false;
                                break;
                            }
                        }
                    }
                    if (b)
                    {
                        similarity += c;
                        letter++;
                    }
                    else
                        break;
                }

                textBox.Text = h.Command.Substring(0, h.TabStart) + similarity;
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            //skin.Draw(batch, Area, Color.White);
            var colour = new Color(0.75f,0.75f, 0.75f, 0.75f);
            batch.Draw(background, Area, colour);

            if (tabCompletion.IsVisible)
            {
                batch.Draw(
                    background,
                    new Rectangle(
                        tabCompletion.Area.X - 3, tabCompletion.Area.Y - 3,
                        tabCompletion.Area.Width + 6, tabCompletion.Area.Height + 6),
                    colour);
                //skin.Draw(batch, new Rectangle(
                //    tabCompletion.Area.X - 3, tabCompletion.Area.Y - 3,
                //    tabCompletion.Area.Width + 6, tabCompletion.Area.Height + 6),
                //    Color.White);
            }

            if (infoBox.IsVisible)
            {
                batch.Draw(
                    background,
                    new Rectangle(
                        infoBox.Area.X - 3, infoBox.Area.Y - 3,
                        infoBox.Area.Width + 6, infoBox.Area.Height + 6),
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

            if (!help.HasValue || textBox.Text != help.Value.Command)
            {
                help = engine.GetHelp(textBox.Text);
                StringBuilder tab = new StringBuilder();
                foreach (var item in help.Value.PossibleCommands.OrderBy(s => s))
                    tab.AppendLine(item);
                tabCompletion.Text = tab.ToString(0, Math.Max(0, tab.Length - 1));
                tabCompletion.IsVisible = !tabCompletion.Text.Equals((StringPart)"");

                infoBox.Text = string.Format("[c:200:200:200]{0}[/c]{1}",
                    help.Value.Definitions,
                    help.Value.Description);
                infoBox.IsVisible = !string.IsNullOrEmpty(help.Value.Definitions) || !string.IsNullOrEmpty(help.Value.Description);
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
            LinkedList<String> previousCommands = new LinkedList<string>();
            LinkedListNode<String> commandScrollPointer = null;

            private TextBox textBox;

            public CommandStack(TextBox textBox, GestureGroup gestures)
            {
                if (textBox == null)
                    throw new NullReferenceException("textBox");

                this.textBox = textBox;

                gestures.Bind(OnPreviousCommand, new KeyPressed(Keys.Up));
                gestures.Bind(OnNextCommand, new KeyPressed(Keys.Down));
            }

            private void OnPreviousCommand(IGesture gesture, GameTime gameTime, IInputDevice device)
            {
                if (commandScrollPointer == null)
                {
                    commandScrollPointer = previousCommands.Last;
                    WritePreviousCommand();
                }
                else if (commandScrollPointer.Previous != null)
                {
                    commandScrollPointer = commandScrollPointer.Previous;
                    WritePreviousCommand();
                }
            }

            private void OnNextCommand(IGesture gesture, GameTime gameTime, IInputDevice device)
            {
                if (commandScrollPointer != null)
                {
                    if (commandScrollPointer.Next != null)
                    {
                        commandScrollPointer = commandScrollPointer.Next;
                        WritePreviousCommand();
                    }
                    else
                    {
                        textBox.Text = "";
                        commandScrollPointer = null;
                    }
                }
            }

            public void PushCommand(String s)
            {
                previousCommands.AddLast(textBox.Text);
                commandScrollPointer = null;
            }

            private void WritePreviousCommand()
            {
                if (commandScrollPointer != null)
                    textBox.Text = commandScrollPointer.Value;
            }
        }

#if WINDOWS
        private class ConsoleTraceListener
            : TraceListener
        {
            public readonly CommandConsole console;

#if WINDOWS
            public readonly RegexTraceFilter RegexFilter;
#endif

            public ConsoleTraceListener(CommandConsole console)
            {
                this.console = console;

#if WINDOWS
                this.RegexFilter = new RegexTraceFilter(console);
                base.Filter = RegexFilter;
#endif
            }

            /// <summary>
            /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
            /// </summary>
            /// <param name="message">A message to write.</param>
            public override void Write(string message)
            {
                console.WriteLine(message);
            }

            /// <summary>
            /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
            /// </summary>
            /// <param name="message">A message to write.</param>
            public override void WriteLine(string message)
            {
                console.WriteLine(message);
            }

#if WINDOWS
            public class RegexTraceFilter
                : TraceFilter
            {
                private List<Regex> filters = new List<Regex>();
                private CommandConsole console;

                internal RegexTraceFilter(CommandConsole console)
                {
                    this.console = console;
                }

                /// <summary>
                /// Adds the a regular expression which will filter out any messages it matches
                /// </summary>
                /// <param name="s">The s.</param>
                public void AddFilter(string s)
                {
                    filters.Add(new Regex(s));
                }

                /// <summary>
                /// Lists the filters registered to the console
                /// </summary>
                public void ListFilters()
                {
                    console.WriteLine("Filters:");
                    for (int i = 0; i < filters.Count; i++)
                        console.WriteLine("\t " + i + " :> " + filters[i]);
                }

                /// <summary>
                /// Removes the filter at the specified index
                /// </summary>
                /// <param name="index">The index.</param>
                /// <returns></returns>
                public Regex RemoveAt(int index)
                {
                    Regex r = filters[index];
                    filters.RemoveAt(index);
                    return r;
                }

                /// <summary>
                /// When overridden in a derived class, determines whether the trace listener should trace the event.
                /// </summary>
                /// <param name="cache">The <see cref="T:System.Diagnostics.TraceEventCache"/> that contains information for the trace event.</param>
                /// <param name="source">The name of the source.</param>
                /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
                /// <param name="id">A trace identifier number.</param>
                /// <param name="formatOrMessage">Either the format to use for writing an array of arguments specified by the <paramref name="args"/> parameter, or a message to write.</param>
                /// <param name="args">An array of argument objects.</param>
                /// <param name="data1">A trace data object.</param>
                /// <param name="data">An array of trace data objects.</param>
                /// <returns>
                /// true to trace the specified event; otherwise, false.
                /// </returns>
                public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
                {
                    for (int i = 0; i < filters.Count; i++)
                    {
                        if (filters[i].IsMatch(formatOrMessage))
                            return false;
                    }
                    return true;
                }
            }
#endif
        }
#endif
    }
}