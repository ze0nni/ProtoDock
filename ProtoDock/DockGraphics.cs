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
        public enum State
        {
            Idle,
            LeftDown,
            Drag,
            DragData
        }
        
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

        private State _state;
        
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

        internal void Update(float dt)
        {
            for (var i = 0; i < _panels.Count; i++) {
                var panel = _panels[i];
                panel.Update(dt);
            }
            CalculateSize(out _dockSize, out _drawSize);
        }

        public void MouseDown(float x, float y, MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    SetState(State.LeftDown);
                    break;
            
                case MouseButtons.Right:
                    SetState(State.Idle);
                    break;
            }
        }

        public bool MouseUp(float x, float y, MouseButtons button)
        {
            switch (_state)
            {
                case State.Idle:                
                    SetState(State.Idle);

                    switch (button)
                    {                       
                        case MouseButtons.Right:
                            if (_selectedPanel != null && IsOverPanel(x, y, _selectedPanel) && _selectedPanel.ContextClick(x, y))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }

                        default:
                            return false;
                    }

                case State.LeftDown:                    
                    if (_selectedPanel != null && IsOverPanel(x, y, _selectedPanel))
                    {
                        _selectedPanel.Click(x, y);
                    }
                    SetState(State.Idle);
                    return false;

                case State.Drag:
                    SetState(State.Idle);
                    return false;

                default:
                    return false;
            }
        }            

        public void MouseMove(float x, float y)
        {
            // _mousePosition = new PointF(x, y);
            //
            // switch (_state)
            // {
            //     case State.Idle:
            //         MouseMoveIdle(x, y);
            //         break;
            //
            //     case State.LeftDown:
            //         if (MathF.Abs(_mouseDownPoint.X - x) > IconSize * 0.3f)
            //         {
            //             _draggedIcon = _selectedIcon;
            //
            //             SetState(State.Drag);
            //             MouseMoveDrag(x, y);
            //         }
            //         else
            //         {
            //             MouseMoveIdle(x, y);
            //         }
            //         break;
            //
            //     case State.Drag:
            //         MouseMoveDrag(x, y);
            //         break;
            // }
            //
            // var screenPos = _dockWindow.PointToScreen(new Point((int)x, (int)0));
            // switch (Position)
            // {
            //     case Position.Top:
            //         _hintWindow.SetPosition(
            //             screenPos.X,
            //             (int)(_dockSize.Height + IconSize * ActiveIconScale),
            //             Position);
            //         break;
            //     case Position.Bottom:
            //         _hintWindow.SetPosition(
            //             screenPos.X,
            //             (int)(screenPos.Y + OffsetY - IconSize * ActiveIconScale),
            //             Position
            //         );
            //         break;
            // }            
        }
        
        private void MouseMoveIdle(float x, float y)
        {
            // IsMouseOver = true;
            //
            // var left = (float)SelectedSkin.Padding.Left;
            // var maxDistance = ActiveIconScaleDistance;
            //
            // for (var i = 0; i < _icons.Count; i++)
            // {
            //     var icon = _icons[i];
            //     var center = left + (icon.Width + IconSpace) * 0.5f;
            //     var distance = Math.Abs(center - x);
            //
            //     var ratio = distance > maxDistance
            //         ? 0f
            //         : 1f - (distance / maxDistance);
            //
            //     icon.SetDistanceToCursor(ratio);
            //
            //     left += icon.Width + IconSpace;
            // }
            //
            // GetPanelFromX(x, out var selectedIcon, out _);
            // if (selectedIcon != _selectedIcon)
            // {
            //     UpdateSelectedIcon(selectedIcon);
            // }
        }

        private void MouseMoveDrag(float x, float y)
        {
            // GetIconFromX(x, out var icon, out var iconLeft);
            //
            // if (icon == _draggedIcon || icon == null)
            // {
            //     return;
            // }
            //
            // var iconCenter = (iconLeft + icon.Width * 0.5f);
            // var distanceToCenter = MathF.Abs(iconCenter - x);
            // if (distanceToCenter < IconSize * 0.5f)
            // {
            //     var index = _icons.IndexOf(icon);
            //     var draggedIndex = _icons.IndexOf(_draggedIcon);
            //
            //     _icons[index] = _draggedIcon;
            //     _icons[draggedIndex] = icon;
            //
            //     SetDirty();
            //     //TODO: Flush
            //     //TODO: Swap Icons In Panel
            // }
        }

        public bool DragOver(float x, float y, IDropMediator mediator, IDataObject data)
        {
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
            SetState(State.Idle);
        }

        private void SetState(State value)
        {
            if (_state == value)
                return;

            _state = value;

            switch (value)
            {
                case State.Idle:
                    break;

                case State.LeftDown:
                    break;

                case State.Drag:
                    // for (var i = 0; i < _icons.Count; i++)
                    // {
                    //     _icons[i].SetDistanceToCursor(0f);
                    // }
                    break;
            }

        }
        
        private bool IsOverPanel(float x, float y, DockPanelGraphics panel)
        {
            // GetPanelPos(icon, out var left, out var top);
            //
            // return x > left && x < left + icon.Width &&
            //     y > top && y < top + icon.Height;
            return false;
        }

        public void MouseLeave()
        {
            // IsMouseOver = false;
            //
            // for (var i = 0; i < _icons.Count; i++)
            // {
            //     _icons[i].SetDistanceToCursor(0f);
            // }
            //
            // UpdateSelectedIcon(null);
            // _hintWindow.Hide();
            //
            // SetDirty();
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        private void UpdateSelectedIcon(DockIconGraphics icon)
        {
            // if (_selectedIcon == icon)
            //     return;
            //
            // _selectedIcon?.MouseLeave();
            //
            // _selectedIcon = icon;
            // _selectedIcon?.MouseEnter();
            //
            // if (_selectedIcon != null)
            // {
            //     _hintWindow.SetText(icon.Model.Title);
            // } else
            // {
            //     _hintWindow.SetText("");
            // }
            //
            // SetDirty();
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
                panelsWidth += panel.Width;
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
            // RenderIcons();
            // RenderDropTarget();
            
            _graphics.Restore(state);

            IsDirty = false;
        }

    }
}
