using System;
using System.Collections.Generic;
using Myre.Debugging.Statistics;
using Myre.UI;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Controls;

using Color = Microsoft.Xna.Framework.Color;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace Myre.Debugging.UI
{
    /// <summary>
    /// Shows a text representation of statistics.
    /// </summary>
    public class StatisticTextLog                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        : Control
    {
        private readonly Dictionary<string, StatisticText> _stats;
        private readonly SpriteFont _font;
        private readonly bool _autoAdd;
        private Color _textColour;

        public Color TextColour
        {
            get { return _textColour; }
            set
            {
                _textColour = value;
                UpdatePositions();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticTextLog"/> class.
        /// </summary>
        public StatisticTextLog(Control parent, SpriteFont font, bool autoAdd = false)
            : base(parent)
        {
            _autoAdd = autoAdd;
            _font = font;
            _textColour = Color.White;
            RespectSafeArea = true;
            _stats = new Dictionary<string, StatisticText>();
            SetPoint(Points.TopLeft, 0, 0);
            SetPoint(Points.Right, 0, 0);

            //AreaChanged += delegate(Control c) { UpdatePositions(); };
        }

        /// <summary>
        /// Adds the statistic to the log.
        /// </summary>
        /// <param name="statisticName">Name of the statistic.</param>
        /// <param name="accessInterval">The time between readings of the statistic.</param>
        public void AddStatistic(string statisticName, TimeSpan accessInterval)
        {
            AddStatistic(Statistic.Create(statisticName), accessInterval);
        }

        /// <summary>
        /// Adds the statistic.
        /// </summary>
        /// <param name="statistic">The statistic.</param>
        /// <param name="accessInterval">The time between readings of the statistic.</param>
        public void AddStatistic(Statistic statistic, TimeSpan accessInterval)
        {
            if (statistic == null)
                throw new ArgumentNullException("statistic");
            if (!_stats.ContainsKey(statistic.Name))
                _stats.Add(statistic.Name, new StatisticText(this, statistic, accessInterval, _font));
            UpdatePositions();
        }

        /// <summary>
        /// Removes the statistic from the log.
        /// </summary>
        /// <param name="statisticName">Name of the statistic.</param>
        public void RemoveStatistic(string statisticName)
        {
            if (_stats.ContainsKey(statisticName))
            {
                _stats[statisticName].Label.Dispose();
                _stats.Remove(statisticName);
                UpdatePositions();
            }
        }

        /// <summary>
        /// Removes the statistic from the log.
        /// </summary>
        /// <param name="statistic">The statistic.</param>
        public void RemoveStatistic(Statistic statistic)
        {
            if (_stats.ContainsKey(statistic.Name) && _stats[statistic.Name].Statistic == statistic)
            {
                //Remove(stats[statistic.Name].Label);
                _stats[statistic.Name].Label.Dispose();
                _stats.Remove(statistic.Name);
                UpdatePositions();
            }
        }

        /// <summary>
        /// Updates the control and its' children.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        public override void Update(GameTime gameTime)
        {
            if (_autoAdd)
            {
                foreach (var item in Statistic.Statistics)
                {
                    if (!_stats.ContainsKey(item.Key))
                        AddStatistic(item.Key, TimeSpan.FromSeconds(0.5));
                }
            }

            RemoveDisposed();

            foreach (var stat in _stats.Values)
                stat.Update();

            base.Update(gameTime);
        }

        private void RemoveDisposed()
        {
            List<Statistic> disposed = null;
            foreach (var stat in _stats.Values)
            {
                if (stat.Statistic.IsDisposed)
                {
                    if (disposed == null)
                        disposed = new List<Statistic>();

                    disposed.Add(stat.Statistic);
                }
            }

            if (disposed != null)
            {
                foreach (var item in disposed)
                    RemoveStatistic(item);
            }
        }

        private void UpdatePositions()
        {
            int height = 0;
            Control previous = null;
            foreach (var stat in _stats.Values)
            {
                if (previous == null)
                    stat.Label.SetPoint(Points.Top, 0, 0);
                else
                    stat.Label.SetPoint(Points.Top, 0, 0, previous, Points.Bottom);

                stat.Label.SetPoint(Points.Left, 0, 0);
                stat.Label.SetPoint(Points.Right, 0, 0);
                stat.Label.Colour = _textColour;

                height += stat.Label.Area.Height;
                previous = stat.Label;
            }

            SetSize(Area.Width, height);
        }
    }

    class StatisticText
        : StatisticTracker
    {
        readonly Label _label;

        public Label Label { get { return _label; } }

        public StatisticText(StatisticTextLog log, Statistic statistic, TimeSpan accessInterval, SpriteFont font)
            : base(statistic, accessInterval)
        {
            _label = new Label(log, font)
            {
                Text = statistic.Name + ": " + string.Format(statistic.Format, statistic.Value)
            };
        }

        public void Update()
        {
            bool read, changed;
            float value = GetValue(out read, out changed);
            if (changed)
                _label.Text = Statistic.Name + ": " + string.Format(Statistic.Format, value);
        }
    }
}
