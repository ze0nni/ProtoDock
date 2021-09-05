using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ProtoDock
{
    class DockIconGraphics
    {
        private readonly DockGraphics _dock;
        private readonly IDockIcon _model;
        private int _size => 1;

        private float _width;
        private float _height;
        private float _targetWidth;
        private float _targetHeight;

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
            _model = model;

            _targetWidth = _dock.IconSize * _size;
            _targetHeight = _dock.IconSize;
        }

        public void Update(float dt)
        {
            if (UpdateToTarget(ref _width, _targetWidth, dt * _dock.IconScaleSpeed * _size))
                _dock.SetDirty();

            if (UpdateToTarget(ref _height, _targetHeight, dt * _dock.IconScaleSpeed))
                _dock.SetDirty();
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
            _model.Render(graphics, _width, _height, _isMouseOver);            
        }
    }
}
