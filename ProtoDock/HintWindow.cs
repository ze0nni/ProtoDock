using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace ProtoDock
{
    public class HintWindow: Form
    {
        public const int MIN_FONT_SIZE = 8;
        public const int MAX_FONT_SIZE = 128;

        private Bitmap _bitmap;
        private Graphics _graphics;
        public int FontSize { get; private set; }
        private Font _font;

        private Brush _bgBrush;
        private Brush _fgBrush;

        private string _text = "";
        private Size _textSize;
        private bool _isDirty = true;

        public HintWindow(): base() {
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;

            FontSize = 24;
            _font = new Font(FontFamily.GenericSansSerif, FontSize);

            _bgBrush = new SolidBrush(Color.Black);
            _fgBrush = new SolidBrush(Color.White);
        }

        protected override void DestroyHandle()
        {
            _graphics?.Dispose();
            _bgBrush?.Dispose();
            _fgBrush?.Dispose();
            _graphics?.Dispose();
            _bitmap?.Dispose();
            _font?.Dispose();

            base.DestroyHandle();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var p = base.CreateParams;
                p.Style = (int)User32.WindowStyles.WS_VISIBLE;
                p.ExStyle |= (int)User32.WindowStylesEx.WS_EX_TOOLWINDOW;
                p.ExStyle |= (int)User32.WindowStylesEx.WS_EX_LAYERED;
                return p;
            }
        }

        public void Update()
        {
            if (!_isDirty)
                return;
            _isDirty = false;

            Size size;
            if (_graphics != null)
            {
                var sizeF = _graphics.MeasureString(_text, _font);
                size = new Size((int)sizeF.Width, (int)sizeF.Height);
            } else
            {
                size = new Size(1, 1);
            }
            if (size.Width <= 0)
            {
                size.Width = 1;
            }
            if (size.Height <= 0)
            {
                size.Height = 1;
            }
            if (size.Width > _textSize.Width || size.Height > _textSize.Height)
            {
                _graphics?.Dispose();
                _bitmap?.Dispose();

                _bitmap = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _graphics = Graphics.FromImage(_bitmap);
            }
            _textSize = size;

            _graphics.Clear(Color.Transparent);
         
            _graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            _graphics.DrawString(_text, _font, Brushes.Black, new PointF(-1, -1));
            _graphics.DrawString(_text, _font, Brushes.Black, new PointF(-1, 1));
            _graphics.DrawString(_text, _font, Brushes.Black, new PointF(1, -1));
            _graphics.DrawString(_text, _font, Brushes.Black, new PointF(1, 1));
            _graphics.DrawString(_text, _font, Brushes.Black, new PointF(1, 2));
            _graphics.DrawString(_text, _font, Brushes.Black, new PointF(2, 1));
            _graphics.DrawString(_text, _font, Brushes.Black, new PointF(2, 2));
            _graphics.DrawString(_text, _font, Brushes.White, new PointF(0, 0));

            FormApi.SetImage(this, _bitmap);
        }

        public void SetText(string value)
        {
            if (_text == value)
            {
                return;
            }

            _text = value;

            _isDirty = true;
        }

        public void UpdateFontSize(int value)
        {
            FontSize = Math.Max(MIN_FONT_SIZE, Math.Min(MAX_FONT_SIZE, value));
            _font?.Dispose();
            _font = new Font(FontFamily.GenericSansSerif, FontSize);

            _isDirty = true;
        }

        public void SetPosition(int x, int y, Api.Position position)
        {
            Show();

            switch (position)
            {
                case Api.Position.Top:
                    Left = x - _textSize.Width / 2;
                    Top = y;
                    break;

                case Api.Position.Bottom:
                    Left = x - _textSize.Width / 2;
                    Top = y - _textSize.Height;
                    break;
            }
        }
    }
}
