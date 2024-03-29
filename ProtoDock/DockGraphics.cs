﻿using ProtoDock.Api;
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
        IReadOnlyList<IDockPanelMediator> Mediators { get; }
    }

    public sealed class DockGraphics : IDisposable
    {
        public const int MIN_ICON_SIZE = 8;
        public const int MAX_ICON_SIZE = 512;
        public const int MIN_ICON_SPACE = 0;
        public const int MAX_ICON_SPACE = 512;

        public Position Position { get; private set; }

        public bool IsDirty { get; private set; }

        public Bitmap Bitmap { get; private set; } = new Bitmap(1, 1);
        private Graphics _graphics;
        public SizeF DockSize => _dockSize;
        private SizeF _dockSize = new SizeF(1, 1);
        private Size _drawSize = new Size(1, 1);

        public int IconSize { get; private set; } = 48;
        public int IconSlotSize => IconSize + SelectedSkin.IconPadding * 2;
        public int IconHoverValue { get; private set; } = 24;
        public int IconSpace { get; private set; } = 8;

        public readonly DockWindow DockWindow;        

        public bool EnableHint { get; private set; } = true;
        public HintWindow Hint { get; private set; }
        public float IconScaleSpeed => 400;

        public readonly Skins Skins;
        public DockSkin SelectedSkin { get; private set; }

        private string _screenName;
        public Screen ActiveScreen { get; private set;  }

        public AutohideRule Autohide { get; private set; }
        public bool OnScreen { get; private set; }
        public float HideFromScreenBoundRatio { get; private set; }

        public IReadOnlyList<DockPanelGraphics> Panels => _panels;
        private readonly List<DockPanelGraphics> _panels = new List<DockPanelGraphics>();
        private readonly Dictionary<DockPanel, DockPanelGraphics> _panelsMap = new Dictionary<DockPanel, DockPanelGraphics>();
        private DockPanelGraphics _selectedPanel;
        private DockIconGraphics _hoveredIcon;

        public bool IsMouseOver { get; private set; }

        public event Action Changed;

        public DockGraphics(
            DockWindow dockWindow,
            HintWindow hintWindow
        )
        {
            DockWindow = dockWindow;
            Hint = hintWindow;
            Skins = new Skins();
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            Bitmap?.Dispose();
        }

        internal void Store(Config.DockConfig config)
        {
            config.Skin = SelectedSkin.Name;
            config.IconSize = IconSize;
            config.IconHoverValue = IconHoverValue;
            config.IconSpace = IconSpace;
            config.ScreenName = _screenName;
            config.Position = Position;
            config.EnableHint = EnableHint;
            config.HintFontSize = Hint.FontSize;
            config.Autohide = Autohide;
        }

        internal void Restore(Config.DockConfig config)
        {
            if (config == null)
            {
                UpdateSkin(null);
                UpdateIconSize(48);
                UpdateIconSpace(8);
                UpdateScreen(null);
                UpdatePosition(Position.Bottom);
                UpdateEnableHint(true);
                Hint.UpdateFontSize(24);
            }
            else
            {
                var skin = Skins.List.FirstOrDefault(s => s.Name == config.Skin);
                UpdateSkin(skin);
                UpdateIconSize(config.IconSize);
                UpdateIconHoverValue(config.IconHoverValue);
                UpdateIconSpace(config.IconSpace);

                UpdateScreen(config.ScreenName);
                UpdatePosition(config.Position);
                UpdateEnableHint(config.EnableHint);
                Hint.UpdateFontSize(config.HintFontSize);
                UpdateAutoHide(config.Autohide);
            }
        }

        internal void AddPanel(DockPanel model) {
            var panel = new DockPanelGraphics(this, model);
            _panels.Add(panel);
            _panelsMap[model] = panel; 
            SetDirty();
        }
        
        internal void RemovePanel(DockPanel model) {
            var panel = _panelsMap[model];
            _panels.Remove(panel);
            SetDirty();
        }

        internal void MovePanel(DockPanel model, int index) {
            var panel = _panelsMap[model];
            _panels.Remove(panel);
            _panels.Insert(index, panel);
            SetDirty();
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

        internal void SetFlashIcon(DockPanel panelModel, IDockIcon iconModel, bool value) {
            foreach (var panel in _panels) {
                if (panel.Model == panelModel) {
                    panel.SetFlashIcon(iconModel, value);
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
            OnScreen = GetOnScreen();
            UpdateHideFromScreenBoundRatio(dt);
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

            var dx2 = (Bitmap.Width - (int)_dockSize.Width) / 2;

            var screenPos = DockWindow.PointToScreen(new Point((int)x, (int)0));

            if (EnableHint)
            {
                switch (Position)
                {
                    case Position.Top:
                        Hint.SetPosition(
                            screenPos.X + dx2,
                            (int)(_dockSize.Height + IconHoverValue),
                            Position);
                        break;
                    case Position.Bottom:
                        Hint.SetPosition(
                            screenPos.X + dx2,
                            (int)(screenPos.Y + OffsetY - IconHoverValue),
                            Position
                        );
                        break;
                }
            } else
            {
                Hint.Hide();
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
            Hint.Hide();
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
                Hint.SetText(icon.Model.Title);
            }
            else
            {
                Hint.SetText("");
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
                skin = Skins.List.First();
            }
            SelectedSkin = skin;
            SelectedSkin.Load();
            SetDirty();

            Changed?.Invoke();
        }

        public void UpdateIconSize(int value)
        {
            IconSize = Math.Max(MIN_ICON_SIZE, Math.Min(MAX_ICON_SIZE, value));

            SetDirty();
            Changed?.Invoke();
        }

        public void UpdateIconHoverValue(int value)
        {
            IconHoverValue = Math.Max(0, Math.Min(MAX_ICON_SIZE, value));

            SetDirty();
            Changed?.Invoke();
        }

        public void UpdateIconSpace(int value)
        {
            IconSpace = Math.Max(MIN_ICON_SPACE, Math.Min(MAX_ICON_SPACE, value));

            SetDirty();
            Changed?.Invoke();
        }

        public void UpdateEnableHint(bool value)
        {
            EnableHint = value;
            SetDirty();
            Changed?.Invoke();
        }

        public void UpdateAutoHide(AutohideRule value)
        {
            Autohide = value;
            SetDirty();
            Changed?.Invoke();
        }

        public float OffsetX
        {
            get
            {
                if (Bitmap == null)
                {
                    return 0;
                }
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
                if (Bitmap == null)
                {
                    return 0;
                }
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
        private bool GetOnScreen()
        {
            switch (Autohide)
            {
                case AutohideRule.Never:
                    {
                        return true;
                    }
                    break;
                
                case AutohideRule.Allways:
                    {
                        switch (GetActiveBound())
                        {
                            case ActiveBound.None:
                                return false;

                            case ActiveBound.Screen:
                            case ActiveBound.Panel:
                                return true;
                        }
                    }
                    break;

                case AutohideRule.OnFullscreen:
                    {
                        switch (GetActiveBound())
                        {
                            case ActiveBound.Panel:
                            case ActiveBound.Screen:
                                return true;
                            default:
                                return !IsFullscreenWindowActive();
                        }
                    }
                    break;
            }
            throw new ArgumentOutOfRangeException();
        }
        private void UpdateHideFromScreenBoundRatio(float dt)
        {
            var lastR = HideFromScreenBoundRatio;
            if (OnScreen)
            {
                HideFromScreenBoundRatio = Math.Max(0, HideFromScreenBoundRatio - dt * 5f);
            } else
            {
                HideFromScreenBoundRatio = Math.Min(1, HideFromScreenBoundRatio + dt * 5f);
            }
            if (lastR != HideFromScreenBoundRatio)
            {
                SetDirty();
                if (HideFromScreenBoundRatio == 1)
                {
                    DockWindow.Visible = false;
                } else
                {
                    DockWindow.Visible = true;
                }
            }
        }

        private IntPtr _lastFullscreenWindow;
        private bool IsFullscreenWindowActive()
        {
            if (_lastFullscreenWindow != IntPtr.Zero && !User32.IsWindow(_lastFullscreenWindow))
            {
                _lastFullscreenWindow = IntPtr.Zero;
            }
            if (_lastFullscreenWindow != IntPtr.Zero && !User32.IsWindowVisible(_lastFullscreenWindow))
            {
                _lastFullscreenWindow = IntPtr.Zero;
            }
            if (_lastFullscreenWindow != IntPtr.Zero)
            {
                var hRes = DwmApi.DwmGetWindowAttribute(_lastFullscreenWindow, (int)DwmApi.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out var cloakedVal, sizeof(int));
                if (cloakedVal != IntPtr.Zero)
                {
                    _lastFullscreenWindow = IntPtr.Zero;
                }
            }
           
            if (_lastFullscreenWindow == IntPtr.Zero)
            {
                _lastFullscreenWindow = User32.GetForegroundWindow();
            }
            if (_lastFullscreenWindow == IntPtr.Zero)
            {
                return false;
            }
            
            User32.GetWindowRect(_lastFullscreenWindow, out var wndRect);
            var scrRect = ActiveScreen.Bounds;

            if (wndRect.Left != scrRect.Left
                || wndRect.Top != scrRect.Top
                || wndRect.Right != scrRect.Right
                || wndRect.Bottom != scrRect.Bottom)
            {
                _lastFullscreenWindow = IntPtr.Zero;
                return false;
            }

            return true;
        }

        private enum ActiveBound
        {
            None,
            Panel,
            Screen
        }

        private ActiveBound GetActiveBound()
        {
            var cursor = Cursor.Position;
            if (DockWindow.Visible)
            {
                if (cursor.X >= DockWindow.Left
                    && cursor.X <= DockWindow.Right
                    && cursor.Y >= DockWindow.Top
                    && cursor.Y < DockWindow.Bottom)
                {
                    return ActiveBound.Panel;
                }
            }

            var bounds = ActiveScreen.Bounds;
            switch (Position)
            {
                case Position.Top:
                    if (cursor.Y == bounds.Top && cursor.X >= DockWindow.Left && cursor.X < DockWindow.Right)
                    {
                        return ActiveBound.Screen;
                    }
                    break;

                case Position.Bottom:
                    if (cursor.Y == bounds.Bottom - 1 && cursor.X >= DockWindow.Left && cursor.X < DockWindow.Right)
                    {
                        return ActiveBound.Screen;
                    }
                    break;
            }

            return ActiveBound.None;
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
                    SelectedSkin.Padding.Horizontal+ IconSlotSize,
                    SelectedSkin.Padding.Horizontal + panelsWidth);
            
            dockSize = new SizeF(
                width,
                SelectedSkin.Padding.Vertical + SelectedSkin.PanelPadding.Vertical + IconSlotSize
            );

            var drawHeight = MathF.Max(
                SelectedSkin.Padding.Top + IconSlotSize + SelectedSkin.Padding.Bottom,
                SelectedSkin.Padding.Top + panelsDrawHeight + SelectedSkin.Padding.Bottom);

            drawSize = new Size(
                (int) MathF.Max(1, width),
                (int) MathF.Max(1, drawHeight)
            );
        }

        private bool PanelFromPosition(float x, float y, out DockPanelGraphics outPanel) {
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                if (x < panel.Left || x > panel.Right)
                    continue;

                bool onPanel;
                switch (Position) {
                    case Position.Top:
                        onPanel = y > panel.Top;
                        break;
                    
                    case Position.Bottom:
                        onPanel = y < panel.Bottom;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!onPanel) {
                    continue;
                }
                
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

                    var effectDistance = IconSlotSize * 2f;
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
