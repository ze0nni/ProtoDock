using BBDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BBDock
{
    public enum Position
    {
        Top,
        Bottom
    }

    public sealed class DockGraphics : IDisposable
    {
        public Position Position => Position.Bottom;

        public bool IsDirty { get; private set; }
        
        public bool IsMouseOver { get; private set; }

        public Bitmap Bitmap { get; private set; }
        private Graphics _graphics;

        public readonly int IconSize;
        public readonly int IconSpace;
        public readonly Padding Paddings;

        public float ActiveIconScale = 0.5f;
        public float ActiveIconScaleDistance => IconSize * 3;
        public float IconScaleSpeed => 400;

        private readonly Bitmap _skin;
        private readonly Padding _skin9Scale;

        private readonly List<DockIconGraphics> _icons = new List<DockIconGraphics>();
        private DockIconGraphics _selectedIcon;

        public DockGraphics(
            int iconSize,
            int iconSpace,
            Padding paddings,
            Bitmap skin,
            Padding skin9Scale
        )
        {
            IconSize = iconSize;
            IconSpace = iconSpace;
            Paddings = paddings;

            _skin = skin;
            _skin9Scale = skin9Scale;
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            Bitmap?.Dispose();
        }

        internal void AddIcon(IDockIcon model)
        {
            var icon = new DockIconGraphics(this, model);
            _icons.Add(icon);

            SetDirty();
        }

        internal void Update(float dt)
        {
            for (var i = _icons.Count - 1; i >= 0; i--)
            {
                var icon = _icons[i];
                icon.Update(dt);
            }
        }

        public void MouseMove(float x, float y)
        {
            IsMouseOver = true;

            var left = (float)Paddings.Left;
            var maxDistance = ActiveIconScaleDistance;

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];
                var center = left + (icon.Width + IconSpace) * 0.5f;
                var distance = Math.Abs(center - x);

                var ratio = distance > maxDistance
                    ? 0f
                    : 1f - (distance / maxDistance);

                icon.SetDistanceToCursor(ratio);

                left += icon.Width + IconSpace;
            }

            var selectedIcon = IconFromPosition(x);
            if (selectedIcon != _selectedIcon)
            {
                UpdateSelectedIcon(selectedIcon);
            }
        }

        public void MouseLeave()
        {
            IsMouseOver = false;

            for (var i = 0; i < _icons.Count; i++)
            {
                _icons[i].SetDistanceToCursor(0f);
            }

            UpdateSelectedIcon(null);

            SetDirty();
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        private void UpdateSelectedIcon(DockIconGraphics icon)
        {
            _selectedIcon?.MouseLeave();

            _selectedIcon = icon;
            _selectedIcon?.MouseEnter();
        }

        private void CalculateSize(out SizeF dockSize, out Size drawSize)
        {
            var iconsCount = _icons.Count;
            var iconsWidthSum = 0f;
            var maxIconHeight = 0f;

            for (var i = 0; i < iconsCount; i++)
            {
                var icon = _icons[i];
                iconsWidthSum += icon.Width;
                maxIconHeight = MathF.Max(maxIconHeight, icon.Height);
            }

            var dockWidth = Paddings.Left + Math.Max(IconSize, iconsWidthSum) + Math.Max(0, iconsCount - 1) * IconSpace + Paddings.Right;
            var dockHeight = Paddings.Top + IconSize + Paddings.Bottom;
            dockSize = new SizeF(
                dockWidth,
                dockHeight
            );

            drawSize = new Size(
                (int)MathF.Ceiling(dockWidth),
                (int)MathF.Ceiling(dockHeight + MathF.Max(0, maxIconHeight - IconSize))
            ); ;
        }

        private DockIconGraphics IconFromPosition(float x)
        {
            var left = (float)Paddings.Left;

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                if (x > left - IconSpace * 0.5f && x < left + icon.Width + IconSpace * 0.5f)
                {
                    return icon;
                }


                left += icon.Width + IconSpace;
            }

            return null;
        }

        internal void Render()
        {
            CalculateSize(out var dockSize, out var drawSize);

            if (Bitmap == null || Bitmap.Width < drawSize.Width || Bitmap.Height < drawSize.Height)
            {
                _graphics?.Dispose();
                Bitmap?.Dispose();

                Bitmap = new Bitmap(drawSize.Width, drawSize.Height, PixelFormat.Format32bppArgb);
                _graphics = Graphics.FromImage(Bitmap);
            }
            _graphics.Clear(Color.Transparent);

            var state = _graphics.Save();


            switch (Position)
            {
                case Position.Top:
                    _graphics.TranslateTransform(
                        (Bitmap.Width - dockSize.Width) * 0.5f,
                        0
                    );
                    break;

                case Position.Bottom:
                    _graphics.TranslateTransform(
                        (Bitmap.Width - dockSize.Width) * 0.5f,
                        (Bitmap.Height - dockSize.Height)
                    );
                    break;

                default:
                    throw new ArgumentException(Position.ToString());
            }

                    RenderSkin(dockSize);
            RenderIcons();

            _graphics.Restore(state);

            IsDirty = false;
        }

        private void RenderSkin(SizeF size)
        {
            var centerWidth = size.Width - _skin9Scale.Left - _skin9Scale.Right;
            var centerHeight = size.Height - _skin9Scale.Top - _skin9Scale.Bottom;

            var skinCenterWidth = _skin.Width - _skin9Scale.Left - _skin9Scale.Right;
            var skinCenterHeight = _skin.Height - _skin9Scale.Top - _skin9Scale.Bottom;

            //Top Left
            _graphics.DrawImage(_skin,
                new RectangleF(0, 0, _skin9Scale.Left, _skin9Scale.Top),
                new RectangleF(0, 0, _skin9Scale.Left, _skin9Scale.Top),
                GraphicsUnit.Pixel
             );

            //Top Middle
            if (centerWidth > 0)
            {
                _graphics.DrawImage(_skin,
                    new RectangleF(_skin9Scale.Left, 0, centerWidth, _skin9Scale.Top),
                    new RectangleF(_skin9Scale.Left, 0, skinCenterWidth, _skin9Scale.Top),
                    GraphicsUnit.Pixel
                 );
            }

            //Top Right
            _graphics.DrawImage(_skin,
                new RectangleF(size.Width - _skin9Scale.Right, 0, _skin9Scale.Right, _skin9Scale.Top),
                new RectangleF(_skin.Width - _skin9Scale.Right, 0, _skin9Scale.Right, _skin9Scale.Top),
                GraphicsUnit.Pixel
             );

            //Center left
            _graphics.DrawImage(_skin,
                new RectangleF(0, _skin9Scale.Top, _skin9Scale.Left, centerHeight),
                new RectangleF(0, _skin9Scale.Top, _skin9Scale.Left, skinCenterWidth),
                GraphicsUnit.Pixel
             );

            //Center Middle
            if (centerWidth > 0)
            {
                _graphics.DrawImage(_skin,
                    new RectangleF(_skin9Scale.Left, _skin9Scale.Top, centerWidth, centerHeight),
                    new RectangleF(_skin9Scale.Left, _skin9Scale.Top, skinCenterWidth, skinCenterHeight),
                    GraphicsUnit.Pixel
                 );
            }

            //Center Right
            _graphics.DrawImage(_skin,
                new RectangleF(size.Width - _skin9Scale.Right, _skin9Scale.Top, _skin9Scale.Right, centerHeight),
                new RectangleF(_skin.Width - _skin9Scale.Right, _skin9Scale.Top, _skin9Scale.Right, skinCenterHeight),
                GraphicsUnit.Pixel
             );

            //Bottom Left
            _graphics.DrawImage(_skin,
                new RectangleF(0, size.Height - _skin9Scale.Bottom, _skin9Scale.Left, _skin9Scale.Bottom),
                new RectangleF(0, _skin.Height - _skin9Scale.Bottom, _skin9Scale.Left, _skin9Scale.Bottom),
                GraphicsUnit.Pixel
             );

            //Bottom Middle
            if (centerWidth > 0)
            {
                _graphics.DrawImage(_skin,
                    new RectangleF(_skin9Scale.Left, size.Height - _skin9Scale.Bottom, centerWidth, _skin9Scale.Bottom),
                    new RectangleF(_skin9Scale.Left, _skin.Height - _skin9Scale.Bottom, skinCenterWidth, _skin9Scale.Bottom),
                    GraphicsUnit.Pixel
                 );
            }

            //Bottom Right
            _graphics.DrawImage(_skin,
                new RectangleF(size.Width - _skin9Scale.Right, size.Height - _skin9Scale.Bottom, _skin9Scale.Right, _skin9Scale.Bottom),
                new RectangleF(_skin.Width - _skin9Scale.Right, _skin.Height - _skin9Scale.Bottom, _skin9Scale.Right, _skin9Scale.Bottom),
                GraphicsUnit.Pixel
             );
        }


        private void RenderIcons()
        {
            var state = _graphics.Save();
            _graphics.TranslateTransform(Paddings.Left, Paddings.Top);

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                switch (Position)
                {
                    case Position.Top:
                        icon.Render(_graphics);
                        break;

                    case Position.Bottom:
                        var vOffset = icon.Height - IconSize;
                        _graphics.TranslateTransform(0, -vOffset);
                        icon.Render(_graphics);
                        _graphics.TranslateTransform(0, vOffset);
                        break;

                    default:
                        throw new ArgumentException(Position.ToString());
                }
                _graphics.TranslateTransform(IconSpace + icon.Width, 0);
            }

            _graphics.Restore(state);
        }

    }
}
