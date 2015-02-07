using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace Myre.Graphics.Pipeline.Fonts
{
    [ContentProcessor(DisplayName = "Myre Vertex File Font Processor")]
    public class VertexFileFontProcessor
         : BaseVertexFontProcessor<KeyValuePair<string, byte[]>>
    {
        private IntPtr _unmanagedMemory;

        private string _directory;
        protected override string Directory
        {
            get
            {
                return _directory;
            }
        }

        protected override VertexFontContent Process(KeyValuePair<string, byte[]> input)
        {
            _directory = input.Key;

            return base.Process(input);
        }

        protected override FontFamily Font(KeyValuePair<string, byte[]> input)
        {
            var fc = new PrivateFontCollection();

            var unmanaged = Marshal.AllocHGlobal(input.Value.Length);
            Marshal.Copy(input.Value, 0, unmanaged, input.Value.Length);
            fc.AddMemoryFont(unmanaged, input.Value.Length);

            return fc.Families[0];
        }

        protected override void Processed()
        {
            Marshal.FreeHGlobal(_unmanagedMemory);
            _unmanagedMemory = IntPtr.Zero;
        }
    }
}
