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

            this.TextAlign = ContentAlignment.MiddleLeft;
            this.BackColor = SystemColors.ActiveCaption;
            this.ForeColor = SystemColors.ActiveCaptionText;
        }

        public void Dispose()
        {
            base.Dispose();
        }
    }
}
