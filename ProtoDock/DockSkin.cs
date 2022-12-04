using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        TopRaw,
        RightRaw,
        BottomRaw,
        LeftRaw
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

        internal void Load(DockSkin skin, bool forced = false)
        {
            if (!string.IsNullOrEmpty(BitmapSource))
            {
                Bitmap?.Dispose();
                using (var stream = File.Open(BitmapSource, FileMode.Open, FileAccess.Read)) {
                    Bitmap = new Bitmap(stream);
                }
            }
            if (Bitmap == null)
            {
                Bitmap = new Bitmap(2, 2);
                Bitmap.SetPixel(0, 0, Color.Red);
                Bitmap.SetPixel(0, 1, Color.Red);
                Bitmap.SetPixel(1, 0, Color.Red);
                Bitmap.SetPixel(1, 1, Color.Red);
            }
        }

        internal void Unload()
        {
            if (!string.IsNullOrEmpty(BitmapSource))
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
                var bmpSizeRaw = new SizeF(Bitmap.Width, Bitmap.Height);
                float x;
                float y;
                float width;
                float height;

                switch (Align)
                {
                    // Common
                    case DockSkinImageAlign.Top:
                        x = (size.Width - bmpSize.Width) * 0.5f;
                        y = 0;
                        width = bmpSize.Width;
                        height = bmpSize.Height;
                        break;

                    case DockSkinImageAlign.Right:
                        x = size.Width - bmpSize.Width;
                        y = (size.Height - bmpSize.Height) * 0.5f;
                        width = bmpSize.Width;
                        height = bmpSize.Height;
                        break;

                    case DockSkinImageAlign.Bottom:
                        x = (size.Width - bmpSize.Width) * 0.5f;
                        y = size.Height - bmpSize.Height;
                        width = bmpSize.Width;
                        height = bmpSize.Height;
                        break;

                    case DockSkinImageAlign.Left:
                        x = 0;
                        y = (size.Height - bmpSize.Height) * 0.5f;
                        width = bmpSize.Width;
                        height = bmpSize.Height;
                        break;
                    // Raw
                    case DockSkinImageAlign.TopRaw:
                        x = (size.Width - bmpSizeRaw.Width) * 0.5f;
                        y = 0;
                        width = bmpSizeRaw.Width;
                        height = bmpSizeRaw.Height;
                        break;

                    case DockSkinImageAlign.RightRaw:
                        x = size.Width - bmpSizeRaw.Width;
                        y = (size.Height - bmpSizeRaw.Height) * 0.5f;
                        width = bmpSizeRaw.Width;
                        height = bmpSizeRaw.Height;
                        break;

                    case DockSkinImageAlign.BottomRaw:
                        x = (size.Width - bmpSizeRaw.Width) * 0.5f;
                        y = size.Height - bmpSizeRaw.Height;
                        width = bmpSizeRaw.Width;
                        height = bmpSizeRaw.Height;
                        break;

                    case DockSkinImageAlign.LeftRaw:
                        x = 0;
                        y = (size.Height - bmpSizeRaw.Height) * 0.5f;
                        width = bmpSizeRaw.Width;
                        height = bmpSizeRaw.Height;
                        break;
                    
                    default:
                        throw new ArgumentException();
                }

                g.DrawImage(Bitmap, x, y, width, height);
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
        public string Root;

        public int VOffset { get; set; }
        public Padding Padding { get; set; }
        public Padding PanelPadding { get; set; }

        public DockSkinImage Dock { get; set; }
        
        public DockSkinImage Panel { get; set; }
        
        public DockSkinImage SelectedBg { get; set; }

        public Point SelectedBgOffset { get; set; }
        
        public DockSkinImage SelectedFg { get; set; }
        
        public Point SelectedFgOffset { get; set; }

        public DockSkinImage HighlightBg { get; set; }

        public DockSkinImage HighlightFg { get; set; }

        public DockSkin(
            string name,
            int vOffset,
            Padding padding,
            Padding panelPadding,
            DockSkinImage dock,
            DockSkinImage panel,
            DockSkinImage selectedBg,
            DockSkinImage selectedFg,
            DockSkinImage highlightBg,
            DockSkinImage highlightFg
        )
        {
            this.Name = name;
            this.VOffset = vOffset;
            this.Padding = padding;
            this.PanelPadding = panelPadding;
            this.Dock = dock;
            this.Panel = panel;
            this.SelectedBg = selectedBg;
            this.SelectedFg = selectedFg;
            this.HighlightFg = highlightFg;
            this.HighlightFg = highlightFg;
        }

        public DockSkin()
        {
            this.Dock = new DockSkinImage();
            this.Panel = new DockSkinImage();
            this.SelectedBg = new DockSkinImage();
            this.SelectedFg = new DockSkinImage();
            this.HighlightFg = new DockSkinImage();
            this.HighlightFg = new DockSkinImage();
        }

        public void Load()
        {
            Dock?.Load(this, true);
            SelectedBg?.Load(this);
            SelectedFg?.Load(this);
            HighlightBg?.Load(this);
            HighlightFg?.Load(this);
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

            var ox = 0;
            var oy = 0;
            
            switch (element)
            {
                case SkinElement.Dock:
                    Dock?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.SelectedBg:
                    ox = SelectedBgOffset.X;
                    oy = SelectedBgOffset.Y;
                    g.TranslateTransform(ox, oy);
                    SelectedBg?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.SelectedFg:
                    ox = SelectedFgOffset.X;
                    oy = SelectedFgOffset.Y;
                    g.TranslateTransform(ox, oy);
                    SelectedFg?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.HighlightBg:
                    HighlightBg?.Draw(g, new SizeF(width, height));
                    break;

                case SkinElement.HighlightFg:
                    HighlightFg?.Draw(g, new SizeF(width, height));
                    break;
            }

            g.TranslateTransform(-(x + ox), -(y + oy));
        }
    }
}
