using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.Settings
{
    public partial class PanelSettingsWindow : Form
    {
        public PanelSettingsWindow(
            IDockPanelMediator mediator,
            Action onFlush,
            Action setDirty,
            Action flashWindow
        )
        {
            InitializeComponent();
            this.Text = mediator.Plugin.Name;
            var display = new SettingsDisplay(Content, onFlush, setDirty, flashWindow);
            mediator.DisplaySettings(display);
        }
    }
}
