using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ProtoDock.QuickLaunch
{
    class QuickLaunchIcon : IDockIcon
    {
        public IDockPanelMediator Mediator { get; }

        private string _path;
        private Icon _icon;

        public QuickLaunchIcon(IDockPanelMediator mediator, string path)
        {
            Mediator = mediator;

            _path = path;
            _icon = Icon.ExtractAssociatedIcon(_path);
        }

        public void Click()
        {
            
        }

        public bool ContextClick()
        {
            return false;
        }

        public void Render(Graphics graphics, float width, float height, bool isSelected)
        {
            graphics.DrawIcon(_icon, new Rectangle(0, 0, (int)width, (int)height));
        }

        public bool Store(out string data)
        {
            data = _path;
            return true;
        }
    }
}
