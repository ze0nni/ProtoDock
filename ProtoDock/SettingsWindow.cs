using ProtoDock.Core;
using System.Windows.Forms;
using System.Linq;

namespace ProtoDock
{
    public partial class SettingsWindow : Form
    {
        private readonly Dock _dock;
        private readonly DockGraphics _graphics;

        internal SettingsWindow(Dock dock, DockGraphics graphics): base()
        {
            this.InitializeComponent();

            _dock = dock;
            _graphics = graphics;

            foreach (var skin in _graphics.Skins)
            {
                SkinCombo.Items.Add(skin);
            }
            SkinCombo.SelectedItem = _graphics.SelectedSkin;
        }

        private void SkinCombo_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            _graphics.UpdateSkin((DockSkin)SkinCombo.SelectedItem);
        }
    }
}
