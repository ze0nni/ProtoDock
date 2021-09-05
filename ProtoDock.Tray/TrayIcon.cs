using ProtoDock.Api;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;

namespace ProtoDock.Tray
{
    internal class TrayIcon : IDockIcon
    {
        public IDockPanel Panel { get; }

        private Icon _icon;

        public TrayIcon(IDockPanel panel, Icon icon)
        {
            Panel = panel;
            _icon = icon;
        }

        public void Click()
        {

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
    }
}
