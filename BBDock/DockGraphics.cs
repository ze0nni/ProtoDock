using BBDock.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;

namespace BBDock
{
    public sealed class DockGraphics
    {
        public Bitmap Bitmap { get; private set; }
        private Graphics _graphics;

        private readonly int _iconSize;
        private readonly int _iconSpace;
        private readonly Padding _paddings;

        private readonly Bitmap _skin;
        private readonly Padding _skin9Scale;


        public DockGraphics(
            int iconSize,
            int iconSpace,
            Padding paddings,
            Bitmap skin,
            Padding skin9Scale
        )
        {
            _iconSize = iconSize;
            _iconSpace = iconSpace;
            _paddings = paddings;

            _skin = skin;
            _skin9Scale = skin9Scale;
        }

        private Size CalculateSize()
        {
            var iconsCount = 5;
            return new Size(
                _paddings.Left + iconsCount * _iconSize + (iconsCount - 1) * _iconSpace + _paddings.Right,
                _paddings.Top + _iconSize + _paddings.Bottom
            );
        }

        internal void Render(Dock dock)
        {
            var size = CalculateSize();
            if (Bitmap == null || Bitmap.Width != size.Width || Bitmap.Height != size.Height)
            {
                _graphics?.Dispose();
                Bitmap?.Dispose();

                Bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
                _graphics = Graphics.FromImage(Bitmap);
            }

            _graphics.Clear(Color.Transparent);
            RenderSkin(size);
        }

        private void RenderSkin(Size size)
        {
            var centerWidth = size.Width - _skin9Scale.Left - _skin9Scale.Right;
            var centerHeight = size.Height - _skin9Scale.Top - _skin9Scale.Bottom;

            var skinCenterWidth = _skin.Width - _skin9Scale.Left - _skin9Scale.Right;
            var skinCenterHeight = _skin.Height - _skin9Scale.Top - _skin9Scale.Bottom;

            //Top Left
            _graphics.DrawImage(_skin,
                new Rectangle(0, 0, _skin9Scale.Left, _skin9Scale.Top),
                new Rectangle(0, 0, _skin9Scale.Left, _skin9Scale.Top),
                GraphicsUnit.Pixel
             );

            //Top Middle
            _graphics.DrawImage(_skin,
                new Rectangle(_skin9Scale.Left, 0, centerWidth, _skin9Scale.Top),
                new Rectangle(_skin9Scale.Left, 0, skinCenterWidth, _skin9Scale.Top),
                GraphicsUnit.Pixel
             );

            //Top Right
            _graphics.DrawImage(_skin,
                new Rectangle(size.Width - _skin9Scale.Right, 0, _skin9Scale.Right, _skin9Scale.Top),
                new Rectangle(_skin.Width - _skin9Scale.Right, 0, _skin9Scale.Right, _skin9Scale.Top),
                GraphicsUnit.Pixel
             );

            //Center left
            _graphics.DrawImage(_skin,
                new Rectangle(0, _skin9Scale.Top, _skin9Scale.Left, centerHeight),
                new Rectangle(0, _skin9Scale.Top, _skin9Scale.Left, skinCenterWidth),
                GraphicsUnit.Pixel
             );

            //Center Middle
            _graphics.DrawImage(_skin,
                new Rectangle(_skin9Scale.Left, _skin9Scale.Top, centerWidth, centerHeight),
                new Rectangle(_skin9Scale.Left, _skin9Scale.Top, skinCenterWidth, skinCenterHeight),
                GraphicsUnit.Pixel
             );

            //Center Right
            _graphics.DrawImage(_skin,
                new Rectangle(size.Width - _skin9Scale.Right, _skin9Scale.Top, _skin9Scale.Right, centerHeight),
                new Rectangle(_skin.Width - _skin9Scale.Right, _skin9Scale.Top, _skin9Scale.Right, skinCenterHeight),
                GraphicsUnit.Pixel
             );

            //Bottom Left
            _graphics.DrawImage(_skin,
                new Rectangle(0, size.Height - _skin9Scale.Bottom, _skin9Scale.Left, _skin9Scale.Bottom),
                new Rectangle(0, _skin.Height - _skin9Scale.Bottom, _skin9Scale.Left, _skin9Scale.Bottom),
                GraphicsUnit.Pixel
             );

            //Bottom Middle
            _graphics.DrawImage(_skin,
                new Rectangle(_skin9Scale.Left, size.Height - _skin9Scale.Bottom, centerWidth, _skin9Scale.Bottom),
                new Rectangle(_skin9Scale.Left, _skin.Height - _skin9Scale.Bottom, skinCenterWidth, _skin9Scale.Bottom),
                GraphicsUnit.Pixel
             );

            //Bottom Right
            _graphics.DrawImage(_skin,
                new Rectangle(size.Width - _skin9Scale.Right, size.Height - _skin9Scale.Bottom, _skin9Scale.Right, _skin9Scale.Bottom),
                new Rectangle(_skin.Width - _skin9Scale.Right, _skin.Height - _skin9Scale.Bottom, _skin9Scale.Right, _skin9Scale.Bottom),
                GraphicsUnit.Pixel
             );
        }
    }
}
