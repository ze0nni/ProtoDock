using ProtoDock.Api;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Input;
using ManagedShell.WindowsTray;

namespace ProtoDock.Tray
{
    internal class TrayIcon : IDockIcon
    {
        public IDockPanelMediator Mediator { get; }

        public string Title => _icon.Title;
        public float Width => 1;
        public bool Hovered => true;

        private readonly NotifyIcon _icon;

        public TrayIcon(IDockPanelMediator mediator, NotifyIcon icon)
        {
            Mediator = mediator;
            _icon = icon;
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

            if (_icon.Icon != null)
            {
                // graphics.DrawIcon(
                //     _icon.Icon,
                //     new Rectangle(0, 0, (int)width, (int)height)
                // );
            }

                
        }
        
        public bool Store(out string data)
        {
            data = default;
            return false;
        }
    }
}
