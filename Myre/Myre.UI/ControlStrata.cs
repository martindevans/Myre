using System.Collections.Generic;

namespace Myre.UI
{
    /// <summary>
    /// A struct which represents the depth of a control.
    /// </summary>
    public struct ControlStrata
    {
        /// <summary>
        /// Gets or sets the layer.
        /// </summary>
        /// <value>The layer.</value>
        public Layer Layer { get; set; }

        /// <summary>
        /// Gets or sets the offset within the layer.
        /// </summary>
        /// <value>The offset.</value>
        public int Offset { get; set; }
    }

    /// <summary>
    /// An enum which represents the dpeth layer of a control.
    /// </summary>
    public enum Layer
    {
        /// <summary>
        /// The layer the controls' parent exists in.
        /// </summary>
        Parent,

        /// <summary>
        /// The background layer. This is the lowest layer.
        /// </summary>
        Background,

        /// <summary>
        /// The low layer. This is above background.
        /// </summary>
        Low,

        /// <summary>
        /// The medium layer. This is above low.
        /// </summary>
        Medium,

        /// <summary>
        /// The high layer. This is above medium.
        /// </summary>
        High,

        /// <summary>
        /// The overlay layer. This is the highest layer.
        /// </summary>
        Overlay
    }

    static class ControlStrataComparer
    {
        public static readonly IComparer<Control> BottomToTop = new BottomToTopComparer();
        public static readonly IComparer<Control> TopToBottom = new TopToBottomComparer();

        class BottomToTopComparer
            : IComparer<Control>
        {
            public int Compare(Control x, Control y)
            {
                var xLayer = FindLayer(x);
                var yLayer = FindLayer(y);

                if (xLayer != yLayer)
                    return (int)xLayer - (int)yLayer;
                else
                    return x.Strata.Offset - y.Strata.Offset;
            }
        }

        class TopToBottomComparer
            : IComparer<Control>
        {
            public int Compare(Control x, Control y)
            {
                var xLayer = FindLayer(x);
                var yLayer = FindLayer(y);

                if (xLayer != yLayer)
                    return (int)yLayer - (int)xLayer;
                else
                    return y.Strata.Offset - x.Strata.Offset;
            }
        }

        private static Layer FindLayer(Control c)
        {
            if (c.Strata.Layer != Layer.Parent)
                return c.Strata.Layer;

            do
            {
                c = c.Parent;
            } while (c != null && c.Strata.Layer == Layer.Parent);

            if (c == null)
                return Layer.Medium;
            else
                return c.Strata.Layer;
        }
    }
}
