using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock
{
    public partial class SkinEditorChooseImage : Form
    {
        private readonly DockSkinImage _image;
        private readonly Action _setDirty;
        
        private readonly PaddingEditor _padding;

        public SkinEditorChooseImage(DockSkinImage image, Action setDirty)
        {
            _image = image;
            _setDirty = setDirty;

            InitializeComponent();

            _padding = new PaddingEditor(Top, Right, Bottom, Left, p =>
            {
                _image.Scale9 = p;
                _setDirty();
            });
            _padding.Set(_image.Scale9);

            Images.Items.Add("");
            foreach (var file in Directory.GetFiles("Skins", "*.png", SearchOption.AllDirectories))
            {
                Images.Items.Add(file);
            }
            if (String.IsNullOrEmpty(_image.BitmapSource))
            {
                Images.SelectedItem = "";
            } else
            {
                Images.SelectedItem = _image.BitmapSource;
            }

            Images.SelectedValueChanged += OnSelectedValueChanged;
        }

        private void OnSelectedValueChanged(object sender, EventArgs e)
        {
            _image.Unload();
            _image.BitmapSource = (string)Images.SelectedItem;            
            _image.Load();
            _setDirty();
        }
    }
}
