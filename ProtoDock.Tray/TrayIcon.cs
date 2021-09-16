using System;
using System.ComponentModel;
using System.Diagnostics;
using ProtoDock.Api;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Input;
using ManagedShell.WindowsTray;

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
            
            // _icon.PropertyChanged += OnPropertyChanged;
            // _icon.Icon.Changed += OnIconChanged;
        }

        public void Dispose() {
            _iconBitmap.Dispose();
            // _icon.PropertyChanged -= OnPropertyChanged;
            // _icon.Icon.Changed -= OnIconChanged;
        }

        public void Update()
        {

        }

        public void Click()
        {
            _icon.IconMouseDown(MouseButton.Left, 0, 0);
            _icon.IconMouseUp(MouseButton.Left, 0, 0);
        }

        public bool ContextClick()
        {
            return false;
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
            }
            
            _mediator.Api.Dock.SetDirty();
        }
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            UpdateView();
        }

        private void OnIconChanged(object sender, EventArgs e) {
            UpdateView();
        }
    }
}
