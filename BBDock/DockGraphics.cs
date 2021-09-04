using BBDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

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

        public float ActiveIconScale = 0.5f;
        public float ActiveIconScaleDistance => IconSize * 3;
        public float IconScaleSpeed => 400;

        public readonly IReadOnlyCollection<DockSkin> Skins;
        public DockSkin SelectedSkin { get; private set; }


        private readonly List<DockIconGraphics> _icons = new List<DockIconGraphics>();
        private DockIconGraphics _selectedIcon;

        public DockGraphics(
            int iconSize,
            int iconSpace,
            List<DockSkin> skins
        )
        {
            IconSize = iconSize;
            IconSpace = iconSpace;
            Skins = skins;

            UpdateSkin(Skins.First());
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

            var left = (float)SelectedSkin.Padding.Left;
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

        public void UpdateSkin(DockSkin skin)
        {
            SelectedSkin?.Unload();
            SelectedSkin = skin;
            SelectedSkin.Load();
            SetDirty();
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

            var dockWidth = MathF.Max(
                SelectedSkin.Padding.Left + iconsWidthSum + Math.Max(01, iconsCount - 1) * IconSpace + SelectedSkin.Padding.Right,
                SelectedSkin.Scale9.Left + SelectedSkin.Scale9.Right);
            var dockHeight = SelectedSkin.Padding.Top + IconSize + SelectedSkin.Padding.Bottom;
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
            var left = (float)SelectedSkin.Padding.Left;

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
            var s9 = SelectedSkin.Scale9;
            var bmp = SelectedSkin.Bitmap;

            var centerWidth = size.Width - s9.Left - s9.Right;
            var centerHeight = size.Height - s9.Top - s9.Bottom;

            var skinCenterWidth = bmp.Width - s9.Left - s9.Right;
            var skinCenterHeight = bmp.Height - s9.Top - s9.Bottom;

            //Top Left
            _graphics.DrawImage(bmp,
                new RectangleF(0, 0, s9.Left, s9.Top),
                new RectangleF(0, 0, s9.Left, s9.Top),
                GraphicsUnit.Pixel
             );

            //Top Middle
            if (centerWidth > 0)
            {
                _graphics.DrawImage(bmp,
                    new RectangleF(s9.Left, 0, centerWidth, s9.Top),
                    new RectangleF(s9.Left, 0, skinCenterWidth, s9.Top),
                    GraphicsUnit.Pixel
                 );
            }

            //Top Right
            _graphics.DrawImage(bmp,
                new RectangleF(size.Width - s9.Right, 0, s9.Right, s9.Top),
                new RectangleF(bmp.Width - s9.Right, 0, s9.Right, s9.Top),
                GraphicsUnit.Pixel
             );

            //Center left
            _graphics.DrawImage(bmp,
                new RectangleF(0, s9.Top, s9.Left, centerHeight),
                new RectangleF(0, s9.Top, s9.Left, skinCenterHeight),
                GraphicsUnit.Pixel
             );

            //Center Middle
            if (centerWidth > 0)
            {
                _graphics.DrawImage(bmp,
                    new RectangleF(s9.Left, s9.Top, centerWidth, centerHeight),
                    new RectangleF(s9.Left, s9.Top, skinCenterWidth, skinCenterHeight),
                    GraphicsUnit.Pixel
                 );
            }

            //Center Right
            _graphics.DrawImage(bmp,
                new RectangleF(size.Width - s9.Right, s9.Top, s9.Right, centerHeight),
                new RectangleF(bmp.Width - s9.Right, s9.Top, s9.Right, skinCenterHeight),
                GraphicsUnit.Pixel
             );

            //Bottom Left
            _graphics.DrawImage(bmp,
                new RectangleF(0, size.Height - s9.Bottom, s9.Left, s9.Bottom),
                new RectangleF(0, bmp.Height - s9.Bottom, s9.Left, s9.Bottom),
                GraphicsUnit.Pixel
             );

            //Bottom Middle
            if (centerWidth > 0)
            {
                _graphics.DrawImage(bmp,
                    new RectangleF(s9.Left, size.Height - s9.Bottom, centerWidth, s9.Bottom),
                    new RectangleF(s9.Left, bmp.Height - s9.Bottom, skinCenterWidth, s9.Bottom),
                    GraphicsUnit.Pixel
                 );
            }

            //Bottom Right
            _graphics.DrawImage(bmp,
                new RectangleF(size.Width - s9.Right, size.Height - s9.Bottom, s9.Right, s9.Bottom),
                new RectangleF(bmp.Width - s9.Right, bmp.Height - s9.Bottom, s9.Right, s9.Bottom),
                GraphicsUnit.Pixel
             );
        }


        private void RenderIcons()
        {
            var state = _graphics.Save();
            _graphics.TranslateTransform(SelectedSkin.Padding.Left, SelectedSkin.Padding.Top);

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
