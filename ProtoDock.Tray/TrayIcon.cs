using ProtoDock.Api;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;

namespace ProtoDock.Tray
{
    internal class TrayIcon : IDockIcon
    {
        public IDockPanelMediator Mediator { get; }

        private Icon _icon;

        public TrayIcon(IDockPanelMediator mediator, Icon icon)
        {
            Mediator = mediator;
            _icon = icon;
        }

        public void Update()
        {

        }

        public void Click()
        {

        }

        public bool ContextClick()
        {
            return false;
        }

        public string Title => "";
        public void Render(
            Graphics graphics,
            float width,
            float height,
            bool isSelected
        ) {

            if (_icon != null)
            {
                graphics.DrawIcon(
                    _icon,
                    new Rectangle(0, 0, (int)width, (int)height)
                );
            }

                
        }

        public void UpdateIcon(Icon icon)
        {
            _icon = icon;
        }

        public bool Store(out string data)
        {
            data = default;
            return false;
        }
    }
}
