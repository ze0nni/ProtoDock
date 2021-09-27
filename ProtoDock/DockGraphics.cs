using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using ProtoDock.Core;
using System.Linq;

namespace ProtoDock
{
    public interface IDropMediator
    {
        IReadOnlyList<IDockPanelMediator> Mediators { get; }
    }

    public sealed class DockGraphics : IDisposable
    {
        public Position Position { get; private set; }

        public bool IsDirty { get; private set; }

        public Bitmap Bitmap { get; private set; }
        private Graphics _graphics;
        private SizeF _dockSize;
        private Size _drawSize;

        public readonly int IconSize;
        public readonly int IconSpace;
        public readonly DockWindow DockWindow;
        private readonly HintWindow _hintWindow;

        public float ActiveIconScale = 1f;
        public float ActiveIconScaleDistance => IconSize * 3;
        public float IconScaleSpeed => 400;

        public readonly IReadOnlyCollection<DockSkin> Skins;
        public DockSkin SelectedSkin { get; private set; }

        private string _screenName;
        public Screen ActiveScreen { get; private set;  }

        private readonly List<DockPanelGraphics> _panels = new List<DockPanelGraphics>();
        private DockPanelGraphics _selectedPanel;
        private DockIconGraphics _hoveredIcon;

        public bool IsMouseOver { get; private set; }

        public event Action Changed;

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
            DockWindow = dockWindow;
            _hintWindow = hintWindow;
            Skins = skins;
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            Bitmap?.Dispose();
        }

        internal void Store(Config.DockConfig config)
        {
            config.Skin = SelectedSkin.Name;
            config.ScreenName = _screenName;
            config.Position = Position;
        }

        internal void Restore(Config.DockConfig config)
        {
            var skin = Skins.FirstOrDefault(s => s.Name == config.Skin);
            UpdateSkin(skin);

            UpdateScreen(config.ScreenName);
            UpdatePosition(config.Position);

        }

        internal void AddPanel(DockPanel model) {
            var panel = new DockPanelGraphics(this, model);
            _panels.Add(panel);
            SetDirty();
        }
        
        internal void RemovePanel(DockPanel panel) {
            throw new NotImplementedException();
        }
        
        internal void AddIcon(DockPanel panelModel, IDockIcon iconModel, bool playAppear)
        {
            foreach (var panel in _panels) {
                if (panel.Model == panelModel) {
                    panel.AddIcon(iconModel, playAppear);
                    SetDirty();
                    return;
                }
            }

            throw new ArgumentException();
        }
        
        internal void RemoveIcon(DockPanel panelModel, IDockIcon iconModel, bool playDisappear) {
            foreach (var panel in _panels) {
                if (panel.Model == panelModel) {
                    panel.RemoveIcon(iconModel, playDisappear);
                    SetDirty();
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
            UpdateIconsDistance(x);

            var screenPos = DockWindow.PointToScreen(new Point((int)x, (int)0));
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

            UpdateHoveredIcon(null);
            ClearIconsDistance();
            _hintWindow.Hide();
            SetDirty();
        }
        
        public bool DragOver(float x, float y, Func<DockPanel, IDropMediator> getMediator, IDataObject data) {
            if (!PanelFromPosition(x, y, out var hoveredPanel))
                return false;

            var mediators = getMediator(hoveredPanel.Model).Mediators;
            for (var i = 0; i < mediators.Count; i++)
            {
                var m = mediators[i];
                if (m.DragCanAccept(data))
                {
                    return true;
                }
            }

            return false;
        }

        public void DragDrop(float x, float y, Func<DockPanel, IDropMediator> getMediator, IDataObject data)
        {
            if (!PanelFromPosition(x, y, out var hoveredPanel))
                return;

            var mediators = getMediator(hoveredPanel.Model).Mediators;
            for (var i = 0; i < mediators.Count; i++)
            {
                var m = mediators[i];
                if (m.DragCanAccept(data))
                {
                    //TODO: index
                    m.DragAccept(-1, data);
                }
            }
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

        public void UpdateScreen(string deviceName)
        {
            if (ActiveScreen != null && ActiveScreen.DeviceName == deviceName)
            {
                return;
            }

            foreach (var screen in Screen.AllScreens)
            {
                if (screen.DeviceName == deviceName)
                {
                    ActiveScreen = screen;
                    _screenName = deviceName;

                    SetDirty();
                    Changed?.Invoke();
                    return;
                }
            }

            ActiveScreen = Screen.AllScreens[0];
            _screenName = ActiveScreen.DeviceName;

            SetDirty();
            Changed?.Invoke();

        }

        public void UpdatePosition(Position position)
        {
            Position = position;
            SetDirty();

            Changed?.Invoke();
        }

        public void UpdateSkin(DockSkin skin)
        {
            SelectedSkin?.Unload();

            if (skin == null) {
                skin = Skins.First();
            }
            SelectedSkin = skin;
            SelectedSkin.Load();
            SetDirty();

            Changed?.Invoke();
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
            var panelsDrawHeight = 0f;
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                panelsWidth += panel.Width;
                panelsDrawHeight = MathF.Max(panelsDrawHeight, panel.DrawHeight);
            }

            panelsWidth += Math.Max(0, _panels.Count - 1) * IconSpace;

            var width =
                MathF.Max(
                    SelectedSkin.Padding.Horizontal+ IconSize,
                    SelectedSkin.Padding.Horizontal + panelsWidth);
            
            dockSize = new SizeF(
                width,
                SelectedSkin.Padding.Vertical + SelectedSkin.PanelPadding.Vertical + IconSize
            );

            var drawHeight = MathF.Max(
                SelectedSkin.Padding.Top + IconSize + SelectedSkin.Padding.Bottom,
                SelectedSkin.Padding.Top + panelsDrawHeight + SelectedSkin.Padding.Bottom);

            drawSize = new Size(
                (int) MathF.Max(1, width),
                (int) MathF.Max(1, drawHeight)
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
        
        public void ClearIconsDistance() {
            for (var pi = 0; pi < _panels.Count; pi++) {
                var panel = _panels[pi];
                for (var ii = 0; ii < panel.Icons.Count; ii++)
                {
                    panel.Icons[ii].SetDistanceToCursor(0f, false);
                }
            }
        }
        
        private void UpdateIconsDistance(float x) {
            var left = (float)SelectedSkin.Padding.Left;
            
            for (var pi = 0; pi < _panels.Count; pi++) {
                var panel = _panels[pi];
                var iconLeft = (float)SelectedSkin.PanelPadding.Left;
                for (var ii = 0; ii < panel.Icons.Count; ii++) {
                    var icon = panel.Icons[ii];
                    var panelX = x - left;

                    if (icon.State != DockIconGraphics.DisplayState.Display) {
                        continue;
                    }
                    
                    var iconCenter = iconLeft + icon.Width * 0.5f;
                    var distance = MathF.Abs(iconCenter - panelX);
                    float ratio;

                    var effectDistance = IconSize * 2f;
                    if (distance > effectDistance) {
                        ratio = 0;
                    }
                    else {
                        ratio = 1f - (distance / effectDistance);
                    }
                    
                    icon.SetDistanceToCursor(ratio, true);

                    iconLeft += icon.Width + IconSpace;
                }

                left += panel.Width + IconSpace;
            }
        }

        internal void Render()
        {
            IsDirty = false;
            
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
            
            SelectedSkin.Dock?.Draw(_graphics, _dockSize);
            
            _graphics.TranslateTransform(
                SelectedSkin.Padding.Left,
                SelectedSkin.Padding.Top
            );
            
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                panel.Render(_graphics);
                _graphics.TranslateTransform(panel.Width + IconSpace, 0);
            }
            
            _graphics.Restore(state);
        }

    }
}
