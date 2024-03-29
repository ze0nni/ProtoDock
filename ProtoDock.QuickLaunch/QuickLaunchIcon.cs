﻿using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.QuickLaunch
{
    class QuickLaunchIcon : IDockIcon, IDisposable
    {
        public IDockPanelMediator Mediator { get; }
        
        public string Title => _path;
        public float Width => 1;
        public bool Hovered => true;
        
        private string _path;
        private Icon _icon;

        public QuickLaunchIcon(IDockPanelMediator mediator, string path)
        {
            Mediator = mediator;

            _path = path;
            _icon = Icon.ExtractAssociatedIcon(_path);
        }

        public void Dispose() {
            _icon.Dispose();
        }

        public void Update()
        {

        }
        public void MouseEnter() {
        }

        public void MouseLeave() {
        }

        public void MouseDown(int x, int y, MouseButtons button) {
        }

        public bool MouseUp(int x, int y, MouseButtons button) {
            return false;
        }
        
        public void MouseMove(int x, int y, MouseButtons button) {
        }

        public void Render(Graphics graphics, float width, float height, Rectangle content)
        {
            graphics.DrawIcon(_icon, content);
        }

        public bool Store(out string data)
        {
            data = _path;
            return true;
        }
    }
}
