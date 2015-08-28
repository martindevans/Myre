using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.Extensions;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;

using GameTime = Microsoft.Xna.Framework.GameTime;
using Color = Microsoft.Xna.Framework.Color;
using Game = Microsoft.Xna.Framework.Game;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using PlayerIndex = Microsoft.Xna.Framework.PlayerIndex;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A text box control.
    /// </summary>
    public class TextBox
        : Control
    {
        private string _textString;
        private readonly StringBuilder _text;
        private bool _typing;
        private readonly string _title;
        private readonly string _description;

        private readonly StringBuilder _drawBuffer;
        private readonly SpriteFont _font;
        private readonly Color _colour;
        private int _drawStartIndex;
        private int _drawEndIndex;

        private readonly Pulser _blink;
        private readonly Pulser _keyRepeat;
        private int _selectionStartIndex;
        private int _selectionEndIndex;
        private int _selectionStartDrawPosition;
        private int _selectionEndDrawPosition;
        private readonly StringBuilder _measurementBuffer;

        private bool _dirty;
        private readonly Texture2D _blank;

        public string Text
        {
            get { return _textString; }
            set
            {
                if (value == null)
                    throw new ArgumentException("value");

                _text.Clear();
                _text.Append(value);
                _textString = value;
                _selectionStartIndex = value.Length;
                _selectionEndIndex = value.Length;
                Dirty();
            }
        }

        public bool TextFitsInSpace
        {
            get;
            private set;
        }

        public List<char> IgnoredCharacters
        {
            get;
            set;
        }

        public TextBox(Control parent, Game game, SpriteFont font, string title, string description)
            : base(parent)
        {
            _textString = string.Empty;
            _text = new StringBuilder();
            _textString = "";
            _typing = false;
            _title = title;
            _description = description;

            _drawBuffer = new StringBuilder();
            _font = font;
            _colour = Color.White;
            _drawStartIndex = 0;
            _drawEndIndex = 0;

            _blink = new Pulser(PulserType.SquareWave, TimeSpan.FromSeconds(0.5));
            _keyRepeat = new Pulser(PulserType.Simple, TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.5));
            _selectionStartIndex = 0;
            _selectionEndIndex = 0;
            _measurementBuffer = new StringBuilder();

            _blank = new Texture2D(game.GraphicsDevice, 1, 1);
            _blank.SetData(new Color[] { Color.White });

            IgnoredCharacters = new List<char>();

            SetSize(100, font.LineSpacing);

            Gestures.Bind((g, t, i) =>
                {
                    i.Owner.Focus(this);
                    BeginTyping(i.Owner.ID < 4 ? (PlayerIndex)i.Owner.ID : PlayerIndex.One);
                },
                new ButtonPressed(Buttons.A),
                new MousePressed(MouseButtons.Left));

            Gestures.Bind((g, t, i) => 
                {
                    var keyboard = (KeyboardDevice)i;
                    foreach (var character in keyboard.Characters)
                        Write(character.ToString(CultureInfo.InvariantCulture));
                },
                new CharactersEntered());

            Gestures.Bind((g, t, i) => Copy(),
                new KeyCombinationPressed(Keys.C, Keys.LeftControl),
                new KeyCombinationPressed(Keys.C, Keys.RightControl));

            Gestures.Bind((g, t, i) => Cut(),
                new KeyCombinationPressed(Keys.X, Keys.LeftControl),
                new KeyCombinationPressed(Keys.X, Keys.RightControl));

            Gestures.Bind((g, t, i) => Paste(),
                new KeyCombinationPressed(Keys.V, Keys.LeftControl),
                new KeyCombinationPressed(Keys.V, Keys.RightControl));

            FocusedChanged += control => { if (!IsFocused) _typing = false; };


        }

        public void BeginTyping(PlayerIndex player)
        {
            _typing = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (_typing)
            {
                _blink.Update();
                _keyRepeat.Update();

                ReadNavigationKeys();
            }

            if (_dirty)
            {
                UpdatePositions();
                _textString = _text.ToString();
            }
        }

        private void Copy()
        {
            Clipboard.Text = Text.Substring(Math.Min(_selectionStartIndex, _selectionEndIndex), Math.Abs(_selectionEndIndex - _selectionStartIndex));
        }

        private void Paste()
        {
            var clipboard = Clipboard.Text;
            if (!string.IsNullOrEmpty(clipboard))
                Write(clipboard);
        }

        private void Cut()
        {
            Copy();
            Write("");
        }

        private void ReadNavigationKeys()
        {
#if WINDOWS
            previousState = currentState;
            currentState = Keyboard.GetState();

            bool shift = currentState.IsKeyDown(Keys.LeftShift) || currentState.IsKeyDown(Keys.RightShift);

            if (NewOrRepeat(Keys.Left))
                Left(shift);
            if (NewOrRepeat(Keys.Right))
                Right(shift);
            if (NewOrRepeat(Keys.Home))
                Home(shift);
            if (NewOrRepeat(Keys.End))
                End(shift);
            if (NewOrRepeat(Keys.Delete))
                Delete();
            if (NewOrRepeat(Keys.Back))
                Backspace();
#endif
        }

#if WINDOWS
        private bool NewOrRepeat(Keys key)
        {
            if (!currentState.IsKeyDown(key))
                return false;

            if (!previousState.IsKeyDown(key))
            {
                _keyRepeat.Restart(false, true);
                return true;
            }

            return _keyRepeat.IsSignalled;
        }
#endif

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            spriteBatch.DrawString(_font, _drawBuffer, new Vector2(Area.X, Area.Y).ToXNA(), _colour);

            if (_typing)
            {
                spriteBatch.Draw(
                    _blank,
                    new Rectangle(
                        Area.X + Math.Min(_selectionStartDrawPosition, _selectionEndDrawPosition), Area.Y,
                        Math.Abs(_selectionEndDrawPosition - _selectionStartDrawPosition), _font.LineSpacing),
                    new Color(0.5f, 0.5f, 0.5f, 0.5f));

                if (_blink.IsSignalled)
                {
                    spriteBatch.Draw(
                        _blank,
                        new Rectangle(Area.X + _selectionEndDrawPosition, Area.Y, 1, _font.LineSpacing),
                        Color.White);
                }
            }
        }

        private void UpdatePositions()
        {
            _drawStartIndex = Math.Min(_drawStartIndex, _selectionEndIndex);

            bool success;
            do
            {
                _drawBuffer.Clear();
                _drawEndIndex = _drawStartIndex;

                while (true)
                {
                    bool fits = _font.MeasureString(_drawBuffer).X <= Area.Width;
                    if (!fits)
                    {
                        _drawBuffer.Remove(_drawBuffer.Length - 1, 1);
                        _drawEndIndex--;
                        break;
                    }
                    else
                    {
                        if (_drawEndIndex >= _text.Length)
                            break;

                        _drawBuffer.Append(_text[_drawEndIndex]);
                        _drawEndIndex++;
                    }
                }

                success = _drawEndIndex >= _selectionEndIndex;

                if (!success)
                    _drawStartIndex++;

            } while (!success);

            TextFitsInSpace = _font.MeasureString(_text).X <= Area.Width;

            _selectionStartDrawPosition = (int)MathHelper.Clamp(MeasureString(_text, _drawStartIndex, _selectionStartIndex - _drawStartIndex).X, 0, Area.Width - 1);
            _selectionEndDrawPosition = (int)MathHelper.Clamp(MeasureString(_text, _drawStartIndex, _selectionEndIndex - _drawStartIndex).X, 0, Area.Width - 1);

            _dirty = false;
        }

        private Vector2 MeasureString(StringBuilder sb, int startIndex, int length)
        {
            _measurementBuffer.Clear();
            _measurementBuffer.Append(sb, startIndex, length);
            return _font.MeasureString(_measurementBuffer).FromXNA();
        }

        private void Write(string characters)
        {
            var selectStart = Math.Min(_selectionStartIndex, _selectionEndIndex);
            var selectEnd = Math.Max(_selectionStartIndex, _selectionEndIndex);

            _text.Remove(selectStart, selectEnd - selectStart);

            int charactersAdded = 0;
            for (int i = characters.Length - 1; i >= 0; i--)
            {
                var c = characters[i];
                if ((_font.DefaultCharacter != null || _font.Characters.Contains(c))
                    && !IgnoredCharacters.Contains(c))
                {
                    _text.Insert(selectStart, c.ToString(CultureInfo.InvariantCulture));
                    charactersAdded++;
                }
            }

            _selectionEndIndex = selectStart + charactersAdded;
            _selectionStartIndex = _selectionEndIndex;

            Dirty();
        }

        private void Dirty()
        {
            _blink.Restart(true, true);
            _dirty = true;
        }

        private void Delete()
        {
            if (_selectionEndIndex != _selectionStartIndex)
                Write(string.Empty);
            else
            {
                if (_selectionEndIndex < _text.Length)
                {
                    _text.Remove(_selectionEndIndex, 1);
                    Dirty();
                }
            }
        }

        private void Backspace()
        {
            if (_selectionEndIndex != _selectionStartIndex)
                Write(string.Empty);
            else
            {
                Left(false);
                Delete();
            }
        }

        private void Left(bool shift)
        {
            _selectionEndIndex--;
            CompleteMove(shift);
        }

        private void Right(bool shift)
        {
            _selectionEndIndex++;
            CompleteMove(shift);
        }

        private void Home(bool shift)
        {
            _selectionEndIndex = 0;
            CompleteMove(shift);
        }

        private void End(bool shift)
        {
            _selectionEndIndex = _text.Length;
            CompleteMove(shift);
        }

        private void CompleteMove(bool shift)
        {
            _selectionEndIndex = (int)MathHelper.Clamp(_selectionEndIndex, 0, _text.Length);
            if (!shift)
                _selectionStartIndex = _selectionEndIndex;
            Dirty();
        }
    }
}
