using Microsoft.VisualBasic.CompilerServices;
using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using ProtoDock.Core;

namespace ProtoDock
{
    public interface IDropMediator
    {
        IEnumerable<IDockPanelMediator> Mediators { get; }
    }

    public sealed class DockGraphics : IDisposable
    {
        public Position Position => Position.Bottom;

        public bool IsDirty { get; private set; }

        public Bitmap Bitmap { get; private set; }
        private Graphics _graphics;
        private SizeF _dockSize;
        private Size _drawSize;

        public readonly int IconSize;
        public readonly int IconSpace;
        private readonly DockWindow _dockWindow;
        private readonly HintWindow _hintWindow;

        public float ActiveIconScale = 1f;
        public float ActiveIconScaleDistance => IconSize * 3;
        public float IconScaleSpeed => 400;

        public readonly IReadOnlyCollection<DockSkin> Skins;
        public DockSkin SelectedSkin { get; private set; }

        private readonly List<DockPanelGraphics> _panels = new List<DockPanelGraphics>();
        private DockPanelGraphics _selectedPanel;
        private DockIconGraphics _hoveredIcon;

        public bool IsMouseOver { get; private set; }
        
        public DockGraphics(
            int iconSize,
            int iconSpace,
            DockWindow dockWindow,
            HintWindow hintWindow,
            List<DockSkin> skins
        )
        {
            IconSize = iconSize;
            IconSpace = iconSpace;
            _dockWindow = dockWindow;
            _hintWindow = hintWindow;
            Skins = skins;

            UpdateSkin(Skins.First());
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            Bitmap?.Dispose();
        }

        internal void AddPanel(DockPanel model) {
            var panel = new DockPanelGraphics(this, model);
            _panels.Add(panel);
            SetDirty();
        }
        
        internal void RemovePanel(DockPanel panel) {
            throw new NotImplementedException();
        }
        
        internal void AddIcon(DockPanel panelModel, IDockIcon iconModel)
        {
            foreach (var panel in _panels) {
                if (panel.Model == panelModel) {
                    panel.AddIcon(iconModel);
                }
            }
        }
        
        internal void RemoveIcon(DockPanel panelModel, IDockIcon iconModel) {
            foreach (var panel in _panels) {
                if (panel.Model == panelModel) {
                    panel.RemoveIcon(iconModel);
                }
            }
        }

        internal void Update(float dt) {
            var panelLeft = 0f;
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                panel.Update(dt);

                panel.Move(panelLeft + SelectedSkin.Padding.Left, SelectedSkin.Padding.Top);
                
                panelLeft += panel.Width + IconSpace;
            }
            CalculateSize(out _dockSize, out _drawSize);
        }

        public void MouseDown(float x, float y, MouseButtons button)
        {
            PanelFromPosition(x, y, out _selectedPanel);
            if (_selectedPanel != null) {
                _selectedPanel.MouseDown(x - _selectedPanel.Left, y - _selectedPanel.Top, button);
            }
        }

        public bool MouseUp(float x, float y, MouseButtons button) {
            bool result;
            if (_selectedPanel != null) {
                result = _selectedPanel.MouseUp(x - _selectedPanel.Left, x - _selectedPanel.Top, button);
            }
            else {
                result = false;
            }
            
            _selectedPanel = null;
            return result;
        }            

        public void MouseMove(float x, float y)
        {
            if (_selectedPanel != null) {
                _selectedPanel?.MouseMove(x - _selectedPanel.Left, y - _selectedPanel.Top);
            }

            IconFromX(x, out var hoveredIcon, out _, out _);
            UpdateHoveredIcon(hoveredIcon);
            

            var screenPos = _dockWindow.PointToScreen(new Point((int)x, (int)0));
            switch (Position)
            {
                case Position.Top:
                    _hintWindow.SetPosition(
                        screenPos.X,
                        (int)(_dockSize.Height + IconSize * ActiveIconScale),
                        Position);
                    break;
                case Position.Bottom:
                    _hintWindow.SetPosition(
                        screenPos.X,
                        (int)(screenPos.Y + OffsetY - IconSize * ActiveIconScale),
                        Position
                    );
                    break;
            }
        }

        public void MouseLeave() {
            IsMouseOver = false;

            _selectedPanel = null;
            for (var i = 0; i < _panels.Count; i++) {
                //TODO: setIconDistance 0
            }

                _hintWindow.Hide();
            SetDirty();
        }
        
        public bool DragOver(float x, float y, IDropMediator mediator, IDataObject data) {
            if (_selectedPanel == null) {
                return false;
            }

            // _mousePosition = new PointF(x, y);
            //
            // SetState(State.DragData);
            //
            // SetDirty();
            //
            // foreach (var panel in mediator.Mediators)
            // {
            //     if (panel.DragCanAccept(data)) {
            //         return true;
            //     }
            // }
            //
            return false;
        }

        public void DragDrop(float x, float y, IDropMediator mediator, IDataObject data)
        {
            if (_selectedPanel == null) {
                return;
            }
            // SetState(State.Idle);
            //
            // foreach (var panel in mediator.Mediators)
            // {
            //     if (panel.DragCanAccept(data))
            //     {
            //         GetDropIndex(x, out var index, out _);
            //         panel.DragAccept(index, data);
            //         return;
            //     }
            // }
        }

        public void DragLeave()
        {
            SetDirty();
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        private void UpdateHoveredIcon(DockIconGraphics icon)
        {
            if (_hoveredIcon == icon)
                return;

            _hoveredIcon?.MouseLeave();

            _hoveredIcon = icon;
            _hoveredIcon?.MouseEnter();

            if (_hoveredIcon != null)
            {
                _hintWindow.SetText(icon.Model.Title);
            }
            else
            {
                _hintWindow.SetText("");
            }

            SetDirty();
        }

        public void UpdateSkin(DockSkin skin)
        {
            SelectedSkin?.Unload();
            SelectedSkin = skin;
            SelectedSkin.Load();
            SetDirty();
        }

        public float OffsetX
        {
            get
            {
                switch (Position)
                {
                    case Position.Top:
                    case Position.Bottom:
                        return (Bitmap.Width - _dockSize.Width) * 0.5f;
                }
                throw new ArgumentException(Position.ToString());
            }
        }

        public float OffsetY
        {
            get
            {
                switch (Position)
                {
                    case Position.Top:
                        return 0;
                    case Position.Bottom:
                        return (Bitmap.Height - _dockSize.Height);
                }
                throw new ArgumentException(Position.ToString());
            }
        }

        private void CalculateSize(out SizeF dockSize, out Size drawSize) {
            var panelsWidth = 0f;
            var panelsHeight = 0f;
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                panelsWidth = panel.Left + panel.Width;
                panelsHeight = MathF.Max(panelsHeight, panel.Height);
            }

            var width =
                MathF.Max(
                    SelectedSkin.Padding.Left + IconSize + SelectedSkin.Padding.Right,
                    SelectedSkin.Padding.Left + panelsWidth + SelectedSkin.Padding.Right);
            var height = MathF.Max(
                SelectedSkin.Padding.Top + IconSize + SelectedSkin.Padding.Bottom, 
                SelectedSkin.Padding.Top + panelsHeight + SelectedSkin.Padding.Bottom);
            
            dockSize = new SizeF(
                width,
                height
            );

            drawSize = new Size(
                (int) MathF.Max(1, width),
                (int) MathF.Max(1, height)
            );
        }

        private bool PanelFromPosition(float x, float y, out DockPanelGraphics outPanel) {
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                if (x < panel.Left || x > panel.Right ||
                    y < panel.Top || y  > panel.Bottom)
                    continue;
                
                //todo: check icons
                outPanel = panel;
                return true;
            }

            outPanel = default;
            return false;
        }

        private bool IconFromX(float x, out DockIconGraphics outIcon, out int outIndex, out float outLeft)
        {           
            var index = 0;
            var left = (float)SelectedSkin.Padding.Left;
            for (var pi = 0; pi < _panels.Count; pi++) {
                var panel = _panels[pi];
                if (panel.GetIconFromX(x - left, out var icon, out var iconIndex, out var iconLeft)) {
                    outIcon = icon;
                    outIndex = index + iconIndex;
                    outLeft = left + iconLeft;
                    return true;
                }
                left += panel.Width + IconSpace;
                index += panel.Icons.Count;
            }

            outIcon = default;
            outIndex = default;
            outLeft = default;

            return false;
        }


        internal void Render()
        {
            if (Bitmap == null || Bitmap.Width < _drawSize.Width || Bitmap.Height < _drawSize.Height)
            {
                _graphics?.Dispose();
                Bitmap?.Dispose();
            
                Bitmap = new Bitmap(_drawSize.Width, _drawSize.Height, PixelFormat.Format32bppArgb);
                _graphics = Graphics.FromImage(Bitmap);
            }
            _graphics.Clear(Color.Transparent);
            
            var state = _graphics.Save();
            
            _graphics.TranslateTransform(
                OffsetX,
                OffsetY
            );
            
            SelectedSkin.Dock.Draw(_graphics, _dockSize);
            
            _graphics.TranslateTransform(
                SelectedSkin.Padding.Left,
                SelectedSkin.Padding.Top
            );
            
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                panel.Render(_graphics);
                _graphics.TranslateTransform(panel.Width, 0);//TODO Width + Space
            }
            
            _graphics.Restore(state);

            IsDirty = false;
        }

    }
}
