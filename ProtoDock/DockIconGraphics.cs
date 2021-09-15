﻿using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ProtoDock
{
    internal class DockIconGraphics
    {
        public enum DisplayState {
            Display,
            Disappear,
            Hidden
        }
        
        private readonly DockPanelGraphics _panel;
        public readonly IDockIcon Model;

        public float Left;

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
                    _panel.Dock.SetDirty();
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
                    _panel.Dock.SetDirty();
                }
            }
        }

        private bool _isMouseOver;

        public DockIconGraphics(DockPanelGraphics panel, IDockIcon model, bool playAppear)
        {
            _panel = panel;
            Model = model;

            _targetWidth = _panel.Dock.IconSize * Model.Width;
            _targetHeight = _panel.Dock.IconSize;
            if (!playAppear) {
                _width = _targetWidth;
                _height = _targetHeight;
            }
        }

        public void Update(float dt)
        {
            Model.Update();

            switch (State) {
                case DisplayState.Display:
                {
                    if (UpdateToTarget(ref _width, _targetWidth, dt * _panel.Dock.IconScaleSpeed * Model.Width))
                        _panel.Dock.SetDirty();

                    if (UpdateToTarget(ref _height, _targetHeight, dt * _panel.Dock.IconScaleSpeed))
                        _panel.Dock.SetDirty();

                    break;
                }

                case DisplayState.Disappear:
                {
                    if (UpdateToTarget(ref _width, 0, dt * _panel.Dock.IconScaleSpeed * Model.Width) ||
                        UpdateToTarget(ref _height, 0, dt * _panel.Dock.IconScaleSpeed)) {
                        _panel.Dock.SetDirty();
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

        public void SetDistanceToCursor(float ratio) {

            if (!Model.Hovered) {
                ratio = 0;
            }
            var size = _panel.Dock.IconSize * (1f + _panel.Dock.ActiveIconScale * ratio);

            _targetWidth = size * Model.Width ;
            _targetHeight = size;
        }

        public void MouseEnter()
        {
            _isMouseOver = true;
            SetDistanceToCursor(1);
        }

        public void MouseLeave()
        {
            _isMouseOver = false;

            SetDistanceToCursor(0);
        }

        public void Render(Graphics graphics)
        {
            if (State == DisplayState.Display)
            {
                Model.Render(graphics, _width, _height, _isMouseOver);
            }
        }

        public void Hide(bool playDisappear) {
            if (playDisappear) {
                State = DisplayState.Disappear;
            }
            else {
                State = DisplayState.Hidden;
            }
        }
    }
}
