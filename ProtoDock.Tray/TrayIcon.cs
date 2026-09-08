using System;
using ProtoDock.Api;
using System.Drawing;
using System.Windows.Forms;

namespace ProtoDock.Tray
{
    internal class TrayIcon : IDockIcon, IDisposable
    {
        public IDockPanelMediator Mediator => _mediator;
        private readonly TrayMediator _mediator;
        private readonly TrayNotifyIcon _icon;

        public string Title => _icon?.Title;
        public float Width => 1;
        public bool Hovered => true;

        public TrayIcon(TrayMediator mediator, TrayNotifyIcon icon)
        {
            _mediator = mediator;
            _icon = icon;
            _icon.Changed += OnIconChanged;
        }

        public void Dispose()
        {
            _icon.Changed -= OnIconChanged;
        }

        public void Update()
        {
        }

        public void MouseEnter()
        {
            _icon.MouseEnter();
        }

        public void MouseLeave()
        {
            _icon.MouseLeave();
        }

        public void MouseDown(int x, int y, MouseButtons button)
        {
            _icon.MouseDown(button);
        }

        public bool MouseUp(int x, int y, MouseButtons button)
        {
            _icon.MouseUp(button);
            return true;
        }

        public void MouseMove(int x, int y, MouseButtons button)
        {
            _icon.MouseMove();
        }

        public void Render(
            Graphics graphics,
            float width,
            float height,
            Rectangle content
        )
        {
            if (_icon.Image != null)
            {
                graphics.DrawImage(
                    _icon.Image,
                    new Rectangle(0, 0, (int)width, (int)height)
                );
            }

            if (_mediator.Api.ScreenRect(this, out var screenRect))
            {
                _icon.Placement = screenRect;
            }
        }

        public bool Store(out string data)
        {
            data = default;
            return false;
        }

        private void OnIconChanged()
        {
            _mediator.Api.Dock.SetDirty();
        }
    }
}
