using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace ProtoDock
{
    [Serializable]
    public enum DockSkinImageAlign
    {
        Scale9,
        Stretch,
        Top,
        Right,
        Bottom,
        Left,
    }

    [Serializable]
    public class DockSkinImage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DockSkinImageAlign Align { get; set; }
        public Bitmap Bitmap { get; private set; }

        public string BitmapSource { get; set; }
        
        public Padding Scale9 { get; set; }

        public DockSkinImage(DockSkinImageAlign align, Bitmap bitmap, Padding scale9)
        {
            Align = align;
            Bitmap = bitmap;
            Scale9 = scale9;
        }

        public DockSkinImage()
        {
        }

        internal void Load()
        {
            if (BitmapSource != null)
            {
                Bitmap?.Dispose();
                Bitmap = new Bitmap(BitmapSource);
            }
        }

        internal void Unload()
        {
            if (BitmapSource != null)
            {
                Bitmap?.Dispose();
                Bitmap = null;
            }
        }

        public void Draw(Graphics g, SizeF size)
        {
            if (Bitmap == null)
            {
                return;
            }

            if (Align == DockSkinImageAlign.Scale9) {
                Draw9Scale(g, size);
            } else if (Align == DockSkinImageAlign.Stretch)
            {
                g.DrawImage(Bitmap, 0, 0, size.Width, size.Height);
            }
            else
            {
                var scale = MathF.Min(
                    size.Width / Bitmap.Width,
                    size.Height / Bitmap.Height
                );
                var bmpSize = new SizeF(Bitmap.Width * scale, Bitmap.Height * scale);
                float x;
                float y;

                switch (Align)
                {
                    case DockSkinImageAlign.Top:
                        x = (size.Width - bmpSize.Width) * 0.5f;
                        y = 0;
                        break;

                    case DockSkinImageAlign.Right:
                        x = size.Width - bmpSize.Width;
                        y = (size.Height - bmpSize.Height) * 0.5f;
                        break;

                    case DockSkinImageAlign.Bottom:
                        x = (size.Width - bmpSize.Width) * 0.5f;
                        y = size.Height - bmpSize.Height;
                        break;

                    case DockSkinImageAlign.Left:
                        x = 0;
                        y = (size.Height - bmpSize.Height) * 0.5f;
                        break;


                    default:
                        return;
                }

                g.DrawImage(Bitmap, x, y, bmpSize.Width, bmpSize.Height);
            }
                    
        }

        private void Draw9Scale(Graphics g, SizeF size)
        {
            var s9 = Scale9;
            var bmp = Bitmap;

            var centerWidth = size.Width - s9.Left - s9.Right;
            var centerHeight = size.Height - s9.Top - s9.Bottom;

            var skinCenterWidth = bmp.Width - s9.Left - s9.Right;
            var skinCenterHeight = bmp.Height - s9.Top - s9.Bottom;

            //Top Left
            g.DrawImage(bmp,
                new RectangleF(0, 0, s9.Left, s9.Top),
                new RectangleF(0, 0, s9.Left, s9.Top),
                GraphicsUnit.Pixel
             );

            //Top Middle
            if (centerWidth > 0)
            {
                g.DrawImage(bmp,
                    new RectangleF(s9.Left, 0, centerWidth, s9.Top),
                    new RectangleF(s9.Left, 0, skinCenterWidth, s9.Top),
                    GraphicsUnit.Pixel
                 );
            }

            //Top Right
            g.DrawImage(bmp,
                new RectangleF(size.Width - s9.Right, 0, s9.Right, s9.Top),
                new RectangleF(bmp.Width - s9.Right, 0, s9.Right, s9.Top),
                GraphicsUnit.Pixel
             );

            //Center left
            g.DrawImage(bmp,
                new RectangleF(0, s9.Top, s9.Left, centerHeight),
                new RectangleF(0, s9.Top, s9.Left, skinCenterHeight),
                GraphicsUnit.Pixel
             );

            //Center Middle
            if (centerWidth > 0)
            {
                g.DrawImage(bmp,
                    new RectangleF(s9.Left, s9.Top, centerWidth, centerHeight),
                    new RectangleF(s9.Left, s9.Top, skinCenterWidth, skinCenterHeight),
                    GraphicsUnit.Pixel
                 );
            }

            //Center Right
            g.DrawImage(bmp,
                new RectangleF(size.Width - s9.Right, s9.Top, s9.Right, centerHeight),
                new RectangleF(bmp.Width - s9.Right, s9.Top, s9.Right, skinCenterHeight),
                GraphicsUnit.Pixel
             );

            //Bottom Left
            g.DrawImage(bmp,
                new RectangleF(0, size.Height - s9.Bottom, s9.Left, s9.Bottom),
                new RectangleF(0, bmp.Height - s9.Bottom, s9.Left, s9.Bottom),
                GraphicsUnit.Pixel
             );

            //Bottom Middle
            if (centerWidth > 0)
            {
                g.DrawImage(bmp,
                    new RectangleF(s9.Left, size.Height - s9.Bottom, centerWidth, s9.Bottom),
                    new RectangleF(s9.Left, bmp.Height - s9.Bottom, skinCenterWidth, s9.Bottom),
                    GraphicsUnit.Pixel
                 );
            }

            //Bottom Right
            g.DrawImage(bmp,
                new RectangleF(size.Width - s9.Right, size.Height - s9.Bottom, s9.Right, s9.Bottom),
                new RectangleF(bmp.Width - s9.Right, bmp.Height - s9.Bottom, s9.Right, s9.Bottom),
                GraphicsUnit.Pixel
             );
        }
    }

    [Serializable]
    public class DockSkin
    {
        public string Name;

        public int VOffset { get; set; }
        public Padding Padding { get; set; }

        public DockSkinImage Dock { get; set; }
        
        public DockSkinImage SelectedBg { get; set; }

        public DockSkinImage SelectedFg { get; set; }

        public DockSkinImage HighlightBg { get; set; }

        public DockSkinImage HighlightFg { get; set; }

        public DockSkin(
            int vOffset,
            Padding padding,
            DockSkinImage dock,
            DockSkinImage selectedBg,
            DockSkinImage selectedFg,
            DockSkinImage highlightBg,
            DockSkinImage highlightFg
        )
        {
            this.VOffset = vOffset;
            this.Padding = padding;
            this.Dock = dock;
            this.SelectedBg = selectedBg;
            this.SelectedFg = selectedFg;
            this.HighlightFg = highlightFg;
            this.HighlightFg = highlightFg;
        }

        public DockSkin()
        {
        }

        public void Load()
        {
            Dock?.Load();
            SelectedBg?.Load();
            SelectedFg?.Load();
            HighlightBg?.Load();
            HighlightFg?.Load();
        }

        public void Unload()
        {
            Dock?.Unload();
            SelectedBg?.Unload();
            SelectedFg?.Unload();
            HighlightBg?.Unload();
            HighlightFg?.Unload();
        }

        public override string ToString()
        {
            if (Name == null)
            {
                return "Default";
            }
            return Name;
        }

        public void Draw(SkinElement element, Graphics g, float x, float y, float width, float height)
        {
            g.TranslateTransform(x, y);

            switch (element)
            {
                case SkinElement.Dock:
                    Dock?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.SelectedBg:
                    SelectedBg?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.SelectedFg:
                    SelectedFg?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.HighlightBg:
                    HighlightBg?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.HighlightFg:
                    HighlightFg?.Draw(g, new SizeF(width, height));
                    break;
            }

            g.TranslateTransform(-x, -y);
        }
    }
}
