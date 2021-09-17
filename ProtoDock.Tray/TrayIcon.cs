using System;
using System.ComponentModel;
using ProtoDock.Api;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ManagedShell.Common.Helpers;
using NotifyIcon = ManagedShell.WindowsTray.NotifyIcon;

namespace ProtoDock.Tray
{
    internal class TrayIcon : IDockIcon, IDisposable {
        public IDockPanelMediator Mediator => _mediator;
        private TrayMediator _mediator;

        public string Title => _icon.Title;
        public float Width => 1;
        public bool Hovered => true;

        private readonly NotifyIcon _icon;
        private Bitmap _iconBitmap;

        public TrayIcon(TrayMediator mediator, NotifyIcon icon)
        {
            _mediator = mediator;
            _icon = icon;

            UpdateView();
            
            _icon.PropertyChanged += OnPropertyChanged;
            // _icon.Icon.Changed += OnIconChanged;
        }

        public void Dispose() {
            _iconBitmap.Dispose();
            _icon.PropertyChanged -= OnPropertyChanged;
            // _icon.Icon.Changed -= OnIconChanged;
        }

        public void Update()
        {

        }

        public void MouseEnter() {
            _icon.IconMouseEnter(MouseHelper.GetCursorPositionParam());
        }

        public void MouseLeave() {
            _icon.IconMouseLeave(MouseHelper.GetCursorPositionParam());
        }

        public void MouseDown(int x, int y, MouseButtons button) {
            _icon.IconMouseDown(GeMouseButton(button), MouseHelper.GetCursorPositionParam(), System.Windows.Forms.SystemInformation.DoubleClickTime);
        }

        public bool MouseUp(int x, int y, MouseButtons button) {
            _icon.IconMouseUp(GeMouseButton(button), MouseHelper.GetCursorPositionParam(), System.Windows.Forms.SystemInformation.DoubleClickTime);
            return true;
        }

        private MouseButton GeMouseButton(MouseButtons buttons) {
            switch (buttons) {
                case MouseButtons.Left:
                    return MouseButton.Left;
                
                case MouseButtons.Right:
                    return MouseButton.Right;
                
                case MouseButtons.Middle:
                    return MouseButton.Middle;
            }

            throw new ArgumentOutOfRangeException();
        }
        
        public void MouseMove(int x, int y, MouseButtons button) {
            _icon.IconMouseMove(MouseHelper.GetCursorPositionParam());
        }
        
        public void Render(
            Graphics graphics,
            float width,
            float height,
            bool isSelected
        ) {

            if (_iconBitmap != null)
            {
                graphics.DrawImage(
                    _iconBitmap,
                    new Rectangle(0, 0, (int)width, (int)height)
                );
            }
            
            if (_mediator.Api.ScreenRect(this, out var screenRect))
            {
                _icon.Placement = new ManagedShell.Interop.NativeMethods.Rect(
                    screenRect.Left,
                    screenRect.Top,
                    screenRect.Right,
                    screenRect.Bottom);
            }
        }
        
        public bool Store(out string data)
        {
            data = default;
            return false;
        }

        private void UpdateView() {
            if (_iconBitmap == null  || _iconBitmap.Width != _icon.Icon.Width || _iconBitmap.Height != _icon.Icon.Height)
            _iconBitmap?.Dispose();
            _iconBitmap = new Bitmap((int)_icon.Icon.Width, (int)_icon.Icon.Height);
            using (var g = Graphics.FromImage(_iconBitmap)) {
                g.Clear(Color.Transparent);

                switch (_icon.Icon) {
                    case InteropBitmap iBitmap:
                    {
                        using var bitmap = GetBitmap(iBitmap);
                        g.DrawImage(bitmap, 0, 0, _iconBitmap.Width, _iconBitmap.Height);
                        break;
                    }
                }
            }
            
            _mediator.Api.Dock.SetDirty();
        }
        
        Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            UpdateView();
        }

        private void OnIconChanged(object sender, EventArgs e) {
            UpdateView();
        }
    }
}
