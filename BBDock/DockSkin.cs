using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BBDock
{ 
    public class DockSkin
    {
        public string Name;

        public int VOffset { get; set; }
        public Padding Padding { get; set; }
        public Bitmap Bitmap { get; private set; }
        public Padding Scale9 { get; set; }
        public string BitmapSource { get; set; }

        public DockSkin(int vOffset, Padding padding, Bitmap bitmap, Padding scale9)
        {
            this.VOffset = vOffset;
            this.Padding = padding;
            this.Bitmap = bitmap;
            this.Scale9 = scale9;
        }

        public DockSkin()
        {
        }

        public void Load()
        {
            if (BitmapSource != null)
            {
                Bitmap?.Dispose();
                Bitmap = new Bitmap(BitmapSource);
            }
        }

        public void Unload()
        {
            if (BitmapSource != null)
            {
                Bitmap?.Dispose();
                Bitmap = null;
            }
        }

        public override string ToString()
        {
            if (Name == null)
            {
                return "Default";
            }
            return Name;
        }
    }
}
