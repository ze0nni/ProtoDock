using Microsoft.VisualBasic.CompilerServices;
using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace ProtoDock
{
    public enum Position
    {
        Top,
        Bottom
    }

    public enum State
    {
        Idle,
        LeftDown,
        Drag,
        DragData
    }

    public interface IDropMediator
    {
        IEnumerable<IDockPanelMediator> Mediators { get; }
    }

    public sealed class DockGraphics : IDisposable
    {
        private State _state;

        public Position Position => Position.Bottom;

        public bool IsDirty { get; private set; }
        
        public bool IsMouseOver { get; private set; }
        private PointF _mouseDownPoint;
        private PointF _mousePosition;
        private DockIconGraphics _draggedIcon;

        public Bitmap Bitmap { get; private set; }
        private Graphics _graphics;
        private SizeF _dockSize;
        private Size _drawSize;

        public readonly int IconSize;
        public readonly int IconSpace;

        public float ActiveIconScale = 1f;
        public float ActiveIconScaleDistance => IconSize * 3;
        public float IconScaleSpeed => 400;

        public readonly IReadOnlyCollection<DockSkin> Skins;
        public DockSkin SelectedSkin { get; private set; }


        private readonly List<DockIconGraphics> _icons = new List<DockIconGraphics>();
        private DockIconGraphics _selectedIcon;

        public DockGraphics(
            int iconSize,
            int iconSpace,
            List<DockSkin> skins
        )
        {
            IconSize = iconSize;
            IconSpace = iconSpace;
            Skins = skins;

            UpdateSkin(Skins.First());
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            Bitmap?.Dispose();
        }

        internal void AddIcon(IDockIcon model)
        {
            var icon = new DockIconGraphics(this, model);
            _icons.Add(icon);

            SetDirty();
        }
        
        internal void RemoveIcon(IDockIcon model) {
            var index = _icons.FindIndex(d => d.Model == model);
            if (index == -1)
                return;

            _icons[index].Hide();
            
            SetDirty();
        }

        internal void Update(float dt)
        {
            for (var i = _icons.Count - 1; i >= 0; i--)
            {
                var icon = _icons[i];
                if (icon.State == DockIconGraphics.DisplayState.Hidden)
                {
                    _icons.RemoveAt(i);
                }
                else {
                    icon.Update(dt);
                }
            }
        }

        public void MouseDown(float x, float y, MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    _mouseDownPoint = new PointF(x, y);
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
                            if (_selectedIcon != null && IsOverIcon(x, y, _selectedIcon) && _selectedIcon.Model.ContextClick())
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
                    if (_selectedIcon != null && IsOverIcon(x, y, _selectedIcon))
                    {
                        _selectedIcon.Model.Click();
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
            _mousePosition = new PointF(x, y);

            switch (_state)
            {
                case State.Idle:
                    MouseMoveIdle(x, y);
                    break;

                case State.LeftDown:
                    if (MathF.Abs(_mouseDownPoint.X - x) > IconSize * 0.3f)
                    {
                        _draggedIcon = _selectedIcon;

                        SetState(State.Drag);
                        MouseMoveDrag(x, y);
                    }
                    else
                    {
                        MouseMoveIdle(x, y);
                    }
                    break;

                case State.Drag:
                    MouseMoveDrag(x, y);
                    break;
            }
        }
        
        private void MouseMoveIdle(float x, float y)
        {
            IsMouseOver = true;

            var left = (float)SelectedSkin.Padding.Left;
            var maxDistance = ActiveIconScaleDistance;

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];
                var center = left + (icon.Width + IconSpace) * 0.5f;
                var distance = Math.Abs(center - x);

                var ratio = distance > maxDistance
                    ? 0f
                    : 1f - (distance / maxDistance);

                icon.SetDistanceToCursor(ratio);

                left += icon.Width + IconSpace;
            }

            GetIconFromX(x, out var selectedIcon, out _);
            if (selectedIcon != _selectedIcon)
            {
                UpdateSelectedIcon(selectedIcon);
            }
        }

        private void MouseMoveDrag(float x, float y)
        {
            GetIconFromX(x, out var icon, out var iconLeft);

            if (icon == _draggedIcon || icon == null)
            {
                return;
            }

            var iconCenter = (iconLeft + icon.Width * 0.5f);
            var distanceToCenter = MathF.Abs(iconCenter - x);
            if (distanceToCenter < IconSize * 0.5f)
            {
                var index = _icons.IndexOf(icon);
                var draggedIndex = _icons.IndexOf(_draggedIcon);

                _icons[index] = _draggedIcon;
                _icons[draggedIndex] = icon;

                SetDirty();
                //TODO: Flush
                //TODO: Swap Icons In Panel
            }
        }

        public bool DragOver(float x, float y, IDropMediator mediator, IDataObject data)
        {
            _mousePosition = new PointF(x, y);

            SetState(State.DragData);

            SetDirty();

            foreach (var panel in mediator.Mediators)
            {
                if (panel.DragCanAccept(data)) {
                    return true;
                }
            }

            return false;
        }

        public void DragDrop(float x, float y, IDropMediator mediator, IDataObject data)
        {
            SetState(State.Idle);

            foreach (var panel in mediator.Mediators)
            {
                if (panel.DragCanAccept(data))
                {
                    GetDropIndex(x, out var index, out _);
                    panel.DragAccept(index, data);
                    return;
                }
            }
        }

        public void DragLeave()
        {
            SetState(State.Idle);
        }

        private bool IsOverIcon(float x, float y, DockIconGraphics icon)
        {
            GetIconPos(icon, out var left, out var top);

            return x > left && x < left + icon.Width &&
                y > top && y < top + icon.Height;
        }

        public void MouseLeave()
        {
            IsMouseOver = false;

            for (var i = 0; i < _icons.Count; i++)
            {
                _icons[i].SetDistanceToCursor(0f);
            }

            UpdateSelectedIcon(null);

            SetDirty();
        }

        public void SetDirty()
        {
            IsDirty = true;
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
                    for (var i = 0; i < _icons.Count; i++)
                    {
                        _icons[i].SetDistanceToCursor(0f);
                    }
                    break;
            }

        }

        private void UpdateSelectedIcon(DockIconGraphics icon)
        {
            _selectedIcon?.MouseLeave();

            _selectedIcon = icon;
            _selectedIcon?.MouseEnter();

            SetDirty();
        }

        public void UpdateSkin(DockSkin skin)
        {
            SelectedSkin?.Unload();
            SelectedSkin = skin;
            SelectedSkin.Load();
            SetDirty();
        }

        private void CalculateSize(out SizeF dockSize, out Size drawSize)
        {
            var iconsCount = _icons.Count;
            var iconsWidthSum = 0f;
            var maxIconHeight = 0f;

            for (var i = 0; i < iconsCount; i++)
            {
                var icon = _icons[i];
                iconsWidthSum += icon.Width;
                maxIconHeight = MathF.Max(maxIconHeight, icon.Height);
            }

            var dockWidth = MathF.Max(
                SelectedSkin.Padding.Left + iconsWidthSum + Math.Max(0, iconsCount - 1) * IconSpace + SelectedSkin.Padding.Right,
                SelectedSkin.Dock.Scale9.Left + SelectedSkin.Dock.Scale9.Right);
            var dockHeight = SelectedSkin.Padding.Top + IconSize + SelectedSkin.Padding.Bottom;
            dockSize = new SizeF(
                dockWidth,
                dockHeight
            );

            drawSize = new Size(
                (int)MathF.Ceiling(dockWidth),
                (int)MathF.Ceiling(dockHeight + MathF.Max(0, maxIconHeight - IconSize))
            ); ;
        }

        private bool GetIconFromX(float x, out DockIconGraphics outIcon, out float outLeft)
        {
            var left = (float)SelectedSkin.Padding.Left;

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                if (x > left - IconSpace * 0.5f && x < left + icon.Width + IconSpace * 0.5f)
                {
                    outIcon =  icon;
                    outLeft = left;
                    return true;
                }


                left += icon.Width + IconSpace;
            }

            outIcon = default;
            outLeft = default;
            return false;
        }

        private void GetIconPos(DockIconGraphics value, out float left, out float top)
        {
            left = (float)SelectedSkin.Padding.Left;

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                if (icon == value)
                {
                    break;
                }

                left += icon.Width + IconSpace;
            }

            switch (Position)
            {
                case Position.Top:
                    top = SelectedSkin.Padding.Top;                    
                    break;

                case Position.Bottom:
                    var vOffset = value.Height - IconSize;
                    top = -vOffset + SelectedSkin.Padding.Bottom;
                    break;

                default:
                    throw new ArgumentException(Position.ToString());
            }
        }

        private bool GetDropIndex(float x, out int outIndex, out float outX)
        {
            if (!GetIconFromX(x, out var icon, out var left)) {
                outIndex = default;
                outX = default;
                return false;
            }
            outIndex = _icons.IndexOf(icon);
            outX = left;

            if (x > left + icon.Width * 0.5f)
            {
                outIndex += 1;
                outX += icon.Width;
            }

            return true;
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
                CalculateSize(out var dockSize, out var drawSize);

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

        internal void Render()
        {
            CalculateSize(out _dockSize, out _drawSize);

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
            RenderIcons();
            RenderDropTarget();

            _graphics.Restore(state);

            IsDirty = false;
        }

        private void RenderIcons()
        {
            var state = _graphics.Save();
            _graphics.TranslateTransform(SelectedSkin.Padding.Left, SelectedSkin.Padding.Top);

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                switch (Position)
                {
                    case Position.Top:
                        icon.Render(_graphics);
                        break;

                    case Position.Bottom:
                        var vOffset = icon.Height - IconSize;
                        _graphics.TranslateTransform(0, -vOffset);
                        icon.Render(_graphics);
                        _graphics.TranslateTransform(0, vOffset);
                        break;

                    default:
                        throw new ArgumentException(Position.ToString());
                }
                _graphics.TranslateTransform(IconSpace + icon.Width, 0);
            }

            _graphics.Restore(state);
        }

        private void RenderDropTarget()
        {
            if (_state != State.DragData)
            {
                return;
            }

            if (!GetDropIndex(_mousePosition.X, out var index, out var x))
            {
                return;
            }

            _graphics.FillRectangle(
                SystemBrushes.ButtonShadow,
                x - 1,
                SelectedSkin.Padding.Top + 1,
                4,
                IconSize);

            _graphics.FillRectangle(
                SystemBrushes.Highlight, 
                x-2,
                SelectedSkin.Padding.Top,
                4,
                IconSize);
        }

    }
}
