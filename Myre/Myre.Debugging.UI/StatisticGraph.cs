using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI;
using Myre.UI.Controls;
using Myre.UI.Text;

namespace Myre.Debugging.Statistics
{
    public class StatisticGraph
        : Control
    {
        StatisticTracker tracker;
        Graph graph;
        Label label;
        Label value;
        Texture2D texture;

        public StatisticGraph(Control parent, SpriteFont font, string statisticName, TimeSpan accessInterval)
            : this(parent, font, Statistic.Get(statisticName), accessInterval)
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
            tracker = new StatisticTracker(statistic, accessInterval);
            graph = new Graph(Device, (int)(15f / (float)accessInterval.TotalSeconds)); //(byte)MathHelper.Clamp(15f / (float)accessInterval.TotalSeconds, 15, 15 * 60));
            label = new Label(this, font);
            label.Text = statistic.Name;
            label.Justification = Justification.Centre;
            label.SetPoint(Points.TopLeft, 2, 2);
            label.SetPoint(Points.TopRight, -2, 2);

            value = new Label(this, font);
            value.Text = "0";
            value.Justification = Justification.Centre;
            value.SetPoint(Points.BottomLeft, 2, -2);
            value.SetPoint(Points.BottomRight, -2, -2);

            texture = new Texture2D(Device, 1, 1);
            texture.SetData<Color>(new Color[] { new Color(0, 0, 0, 0.8f) });

            SetSize(200, 120);
        }

        public override void Update(GameTime gameTime)
        {
            if (tracker.Statistic.IsDisposed)
            {
                Dispose();
                return;
            }

            bool read, changed;
            float value = tracker.GetValue(out read, out changed);
            if (read)
                graph.Add(value);
            if (changed)
                this.value.Text = value.ToString();
            base.Update(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {
            batch.Draw(texture, Area, Color.White);
            batch.End();
            graph.Draw(new Rectangle(
                Area.X + 2, 
                Area.Y + label.Area.Height + 2,
                Area.Width - 4, 
                Area.Height - label.Area.Height - value.Area.Height - 4));
            batch.Begin();
        }
    }
}
