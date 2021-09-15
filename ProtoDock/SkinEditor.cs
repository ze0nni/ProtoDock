using ProtoDock.Core;
using System;
using System.Windows.Forms;

namespace ProtoDock
{
    public partial class SkinEditor : Form
    {
        private readonly DockGraphics _graphics;

        private readonly PaddingEditor _dockPadding;
        private readonly PaddingEditor _panelPadding;

        private readonly ImageEditor _dockImage;

        private readonly ImageEditor _panelImage;
        private readonly ImageEditor _selectedBgImage;
        private readonly ImageEditor _selectedFgImage;
        private readonly ImageEditor _highlightBgBgImage;
        private readonly ImageEditor _highlightFgImage;

        public SkinEditor(DockGraphics graphics)
        {
            InitializeComponent();

            _graphics = graphics;

            _dockPadding = new PaddingEditor(DockPaddingTop, DockPaddingRight, DockPaddingBottom, DockPaddingLeft, p =>
            {
                _graphics.SelectedSkin.Padding = p;
                _graphics.SetDirty();
            });
            _dockPadding.Set(_graphics.SelectedSkin.Padding);

            _panelPadding = new PaddingEditor(PanelPaddingTop, PanelPaddingRight, PanelPaddingBottom, PanelPaddingLeft, p =>
            {
                _graphics.SelectedSkin.PanelPadding = p;
                _graphics.SetDirty();
            });
            _panelPadding.Set(_graphics.SelectedSkin.PanelPadding);

            _dockImage = new ImageEditor(Dock, ChooseDock, _graphics.SelectedSkin.Dock, _graphics.SetDirty);
            _panelImage = new ImageEditor(Panel, ChoosePanel, _graphics.SelectedSkin.Panel, _graphics.SetDirty);
            _selectedBgImage = new ImageEditor(SelectedBg, ChooseSelectedBg, _graphics.SelectedSkin.SelectedBg, _graphics.SetDirty);
            _selectedFgImage = new ImageEditor(SelectedFg, ChooseSelectedFg, _graphics.SelectedSkin.SelectedFg, _graphics.SetDirty);
            _highlightBgBgImage = new ImageEditor(HighlightBg, ChooseHighlightBg, _graphics.SelectedSkin.HighlightBg, _graphics.SetDirty);
            _highlightFgImage = new ImageEditor(HighlightFg, ChooseHighlightFg, _graphics.SelectedSkin.HighlightFg, _graphics.SetDirty);
        }
    }

    public class PaddingEditor
    {
        private readonly NumericUpDown _up;
        private readonly NumericUpDown _right;
        private readonly NumericUpDown _down;
        private readonly NumericUpDown _left;
        private readonly Action<Padding> _update;

        public PaddingEditor(
            NumericUpDown up,
            NumericUpDown right,
            NumericUpDown down,
            NumericUpDown left,
            Action<Padding> update
        )
        {
            _up = up;
            _right = right;
            _down = down;
            _left = left;
            _update = update;

            up.ValueChanged += OnValueChanged;
            right.ValueChanged += OnValueChanged;
            down.ValueChanged += OnValueChanged;
            left.ValueChanged += OnValueChanged;
        }

        public void Set(Padding padding)
        {
            _up.Value = padding.Top;
            _right.Value = padding.Right;
            _down.Value = padding.Bottom;
            _left.Value = padding.Left;
        }

        private void OnValueChanged(object? sender, EventArgs e)
        {
            _update(new Padding(
                    (int)_left.Value,
                    (int)_up.Value,
                    (int)_right.Value,
                    (int)_down.Value
                ));
        }
    }

    public class ImageEditor
    {
        private readonly TextBox _text;
        private readonly Button _button;
        private readonly DockSkinImage _image;
        private readonly Action _setDirty;

        public ImageEditor(TextBox text, Button button, DockSkinImage image, Action setDirty)
        {
            _text = text;
            _button = button;
            _image = image;
            _setDirty = setDirty;

            _text.Text = _image.BitmapSource;

            _button.Click += OnClick;
        }

        private void OnClick(object sender, EventArgs e)
        {
            new SkinEditorChooseImage(_image, _setDirty).ShowDialog();
            _text.Text = _image.BitmapSource;
        }
    }
}
