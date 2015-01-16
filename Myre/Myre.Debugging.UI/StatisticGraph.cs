using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Debugging.Statistics;
using Myre.UI;
using Myre.UI.Controls;
using Myre.UI.Text;

namespace Myre.Debugging.UI
{
    public class StatisticGraph
        : Control
    {
        readonly StatisticTracker _tracker;
        readonly Graph _graph;
        readonly Label _label;
        readonly Label _value;
        readonly Texture2D _texture;

        public StatisticGraph(Control parent, SpriteFont font, string statisticName, TimeSpan accessInterval)
            : this(parent, font, Statistic.Create(statisticName), accessInterval)
        {
        }

        public StatisticGraph(Control parent, SpriteFont font, Statistic statistic, TimeSpan accessInterval)
            : base(parent)
        {
            if (statistic == null)
                throw new ArgumentNullException("statistic");

            if (accessInterval == TimeSpan.Zero)
                accessInterval = TimeSpan.FromMilliseconds(16);

            Strata = new ControlStrata() { Layer = Layer.Overlay };
            _tracker = new StatisticTracker(statistic, accessInterval);
            _graph = new Graph(Device, (int)(15f / (float)accessInterval.TotalSeconds)); //(byte)MathHelper.Clamp(15f / (float)accessInterval.TotalSeconds, 15, 15 * 60));
            _label = new Label(this, font);
            _label.Text = statistic.Name;
            _label.Justification = Justification.Centre;
            _label.SetPoint(Points.TopLeft, 2, 2);
            _label.SetPoint(Points.TopRight, -2, 2);

            _value = new Label(this, font);
            _value.Text = "0";
            _value.Justification = Justification.Centre;
            _value.SetPoint(Points.BottomLeft, 2, -2);
            _value.SetPoint(Points.BottomRight, -2, -2);

            _texture = new Texture2D(Device, 1, 1);
            _texture.SetData<Color>(new Color[] { new Color(0, 0, 0, 0.8f) });

            SetSize(200, 120);
        }

        public override void Update(GameTime gameTime)
        {
            if (_tracker.Statistic.IsDisposed)
            {
                Dispose();
                return;
            }

            bool read, changed;
            float value = _tracker.GetValue(out read, out changed);
            if (read)
                _graph.Add(value);
            if (changed)
                _value.Text = value.ToString(CultureInfo.InvariantCulture);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch batch)
        {
            batch.Draw(_texture, Area, Color.White);
            batch.End();
            _graph.Draw(new Rectangle(
                Area.X + 2, 
                Area.Y + _label.Area.Height + 2,
                Area.Width - 4, 
                Area.Height - _label.Area.Height - _value.Area.Height - 4));
            batch.Begin();
        }
    }
}
