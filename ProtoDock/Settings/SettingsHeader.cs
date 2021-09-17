using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.Settings
{
    class SettingsHeader : Label, ISettingsLine
    {
        public Control Control => this;

        public SettingsHeader(string text)
        {
            this.Text = text;

            this.BackColor = SystemColors.ActiveCaption;
            this.ForeColor = SystemColors.ActiveCaptionText;
            this.BorderStyle = BorderStyle.Fixed3D;
        }

        public void Dispose()
        {
            base.Dispose();
        }
    }
}
