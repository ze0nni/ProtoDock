using ProtoDock.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;

namespace ProtoDock
{
    public partial class DockWindow : Form
    {

        public IntPtr HInstance => User32.HInstance;

        private readonly Timer _timer;

        private readonly DockGraphics _graphics;
        private readonly Dock _dock;
        private readonly HintWindow _hint;
        private long _lastUpdate;

        private readonly ContextMenuStrip _contextMenu = new ContextMenuStrip();

        private bool _doExit;

        public DockWindow(): base()
        {
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;

            this.AllowDrop = true;

            _lastUpdate = DateTime.Now.Ticks;
            _timer = new Timer();
            _timer.Interval = 1000 / 60;
            _timer.Start();
            _timer.Tick += OnTick;

            _hint = new HintWindow();

            _graphics = new DockGraphics(
                48,
                8,
                this,
                _hint,
                LoadSkins()
            );
            _dock = new Dock(HInstance, Handle, _graphics);

            this.FormClosing += (s, e) =>
            {
                e.Cancel = !_doExit;
            };

            this.FormClosed += (s, e) => {
                _timer.Dispose();
                _graphics.Dispose();
                _dock.Dispose();
            };

            this.MouseLeave += OnMouseLeave;
            this.MouseMove += OnMouseMove;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.DragOver += OnDragOver;
            this.DragDrop += OnDragDrop;
            this.DragLeave += OnDragLeave;

            CreateContextMenu();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var p = base.CreateParams;
                p.Style = (int)PInvoke.User32.WindowStyles.WS_VISIBLE;
                p.ExStyle |= (int)PInvoke.User32.WindowStylesEx.WS_EX_TOOLWINDOW;
                p.ExStyle |= (int)PInvoke.User32.WindowStylesEx.WS_EX_LAYERED;
                return p;
            }
        }

        private void CreateContextMenu()
        {
            _contextMenu.Items.Add(new ToolStripMenuItem("Settings", null, OnSettingsClick));
            
            _contextMenu.Items.Add(new ToolStripSeparator());

            var pluginsMenu = new ToolStripMenuItem("Add panel");
            _contextMenu.Items.Add(pluginsMenu);
            foreach (var p in _dock.Plugins)
            {
                pluginsMenu.DropDownItems.Add(new ToolStripMenuItem(p.Name, null, (s, e) =>
                {
                    _dock.AddPanel().AddMediator(p.Create());
                }));
            }

            _contextMenu.Items.Add(new ToolStripSeparator());

            _contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, OnExitClick));
        }

        private List<DockSkin> LoadSkins()
        {
            var list = new List<DockSkin>();

            foreach (var r  in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Debug.WriteLine(r);
            }

            list.Add(
                new DockSkin(
                    0,
                    new Padding(14, 14, 14, 14),
                    new Padding(2, 2, 2, 2),
                    new DockSkinImage(
                        DockSkinImageAlign.Scale9,
                        new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoDock.Embeded.Default.png")),
                        new Padding(32, 32, 32, 32)
                    ),
                    new DockSkinImage(
                        DockSkinImageAlign.Scale9,
                        new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoDock.Embeded.Default_panel.png")),
                        new Padding(8,8,8,8)
                    ),
                    new DockSkinImage(
                        DockSkinImageAlign.Bottom,
                        new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoDock.Embeded.Default_selected.png")),
                        new Padding()
                    ),
                    new DockSkinImage(
                        DockSkinImageAlign.Bottom,
                        new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoDock.Embeded.Default_selected_fg.png")),
                        new Padding()
                    ),
                    new DockSkinImage(
                        DockSkinImageAlign.Stretch,
                        new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoDock.Embeded.Default_highlight.png")),
                        new Padding(32, 32, 32, 32)
                    ),
                    null
                )
            );

            try
            {
                foreach (var file in Directory.GetFiles("./Skins/", "*.json"))
                {
                    try
                    {
                        var data = File.ReadAllText(file);
                        var skin = JsonSerializer.Deserialize<DockSkin>(data);
                        skin.Name = file;
                        list.Add(skin);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc);
                    }
                }
            } catch
            {
                //
            }

            return list;
        }

        public void Render()
        {
            var screen = Screen.AllScreens[0];
            var bounds = screen.Bounds;

            _graphics.Render();
            FormApi.SetImage(this, _graphics.Bitmap);

            this.Left = (bounds.Width - _graphics.Bitmap.Width) / 2;
            switch (_graphics.Position)
            {
                case Api.Position.Top:
                    this.Top = -_graphics.SelectedSkin.VOffset;
                    break;

                case Api.Position.Bottom:
                    this.Top = (bounds.Height - _graphics.Bitmap.Height) + _graphics.SelectedSkin.VOffset;
                    break;

                default:
                    throw new ArgumentException(_graphics.Position.ToString());
            }            
        }

        private void OnTick(Object sender, EventArgs e) {
            var ticks = DateTime.Now.Ticks;
            var delta = ticks - _lastUpdate;
            _lastUpdate = ticks;
            
            _graphics.Update(delta / (1000f * 10000f));

            if (_graphics.IsDirty)
                Render();

            _hint.Update();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _graphics.MouseMove(e.X - _graphics.OffsetX, e.Y - _graphics.OffsetY);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _graphics.MouseLeave();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            _graphics.MouseDown(e.X - _graphics.OffsetX, e.Y - _graphics.OffsetY, e.Button);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (_graphics.MouseUp(e.X - _graphics.OffsetX, e.Y - _graphics.OffsetY, e.Button))
                return;

            if (e.Button == MouseButtons.Right)
            {
                _contextMenu.Show(this, e.Location);
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            var p = this.PointToClient(new Point(e.X, e.Y));
            if (_graphics.DragOver(p.X - _graphics.OffsetX, p.Y - _graphics.OffsetY, _dock.GetDropMediator, e.Data)) {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            var p = this.PointToClient(new Point(e.X, e.Y));
            _graphics.DragDrop(p.X - _graphics.OffsetX, p.Y - _graphics.OffsetY, _dock.GetDropMediator, e.Data);
        }

        private void OnDragLeave(object sender, EventArgs e)
        {
            _graphics.DragLeave();
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            new SettingsWindow(_dock, _graphics).Show();
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            _doExit = true;
            Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DockWindow
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "DockWindow";
            this.Load += new System.EventHandler(this.DockWindow_Load);
            this.ResumeLayout(false);

        }

        private void DockWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
