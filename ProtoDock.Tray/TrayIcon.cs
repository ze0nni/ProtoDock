using ProtoDock.Api;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;

namespace ProtoDock.Tray
{
    internal class TrayIcon : IDockIcon
    {
        private Icon _icon;

        public TrayIcon(Icon icon)
        {
            _icon = icon;
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
