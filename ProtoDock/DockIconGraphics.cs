using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ProtoDock
{
    class DockIconGraphics
    {
        public enum DisplayState {
            Display,
            Disappear,
            Hidden
        }
        
        private readonly DockGraphics _dock;
        public readonly IDockIcon Model;
        private int _size => 1;

        private float _width;
        private float _height;
        private float _targetWidth;
        private float _targetHeight;
        public DisplayState State { get; private set; }

        public float Width
        {
            get => _width;
            private set
            {
                if (value != _width)
                {
                    _width = value;
                    _dock.SetDirty();
                }
            }
        }
        public float Height
        {
            get => _height;
            private set
            {
                if (value != _height)
                {
                    _height = value;
                    _dock.SetDirty();
                }
            }
        }

        private bool _isMouseOver;

        public DockIconGraphics(DockGraphics dock, IDockIcon model)
        {
            _dock = dock;
            Model = model;

            _targetWidth = _dock.IconSize * _size;
            _targetHeight = _dock.IconSize;
        }

        public void Update(float dt)
        {
            switch (State) {
                case DisplayState.Display:
                {
                    if (UpdateToTarget(ref _width, _targetWidth, dt * _dock.IconScaleSpeed * _size))
                        _dock.SetDirty();

                    if (UpdateToTarget(ref _height, _targetHeight, dt * _dock.IconScaleSpeed))
                        _dock.SetDirty();

                    break;
                }

                case DisplayState.Disappear:
                {
                    if (UpdateToTarget(ref _width, 0, dt * _dock.IconScaleSpeed * _size) ||
                        UpdateToTarget(ref _height, 0, dt * _dock.IconScaleSpeed)) {
                        _dock.SetDirty();
                    }
                    else {
                        State = DisplayState.Hidden;
                    }

                    break;
                }
        }
        }

        public bool UpdateToTarget(ref float value, float target, float step)
        {
            if (target > value)
            {
                value = MathF.Min(target, value + step);
                return true;
            }

            if (target < value)
            {
                value = MathF.Max(target, value - step);
                return true;
            }

            return false;
        }

        public void SetDistanceToCursor(float ratio)
        {
            var size = _dock.IconSize * (1f + _dock.ActiveIconScale * ratio);

            _targetWidth = size * _size;
            _targetHeight = size;
        }

        public void MouseEnter()
        {
            _isMouseOver = true;
        }

        public void MouseLeave()
        {
            _isMouseOver = false;
        }

        public void Render(Graphics graphics)
        {
            if (State == DisplayState.Display)
            {
                Model.Render(graphics, _width, _height, _isMouseOver);
            }
        }

        public void Hide() {
            State = DisplayState.Disappear;
        }
    }
}
