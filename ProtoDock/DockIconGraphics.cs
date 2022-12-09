using ProtoDock.Api;
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

        private float _size;
        private float _targetSize;
        public DisplayState State { get; private set; }

        public float Width => Model.Width * _size;
        public float Height => _size;

        public bool Flash;
        
        private float Size
        {
            get => _size;
            set
            {
                if (value != _size)
                {
                    _size = value;
                    _panel.Dock.SetDirty();
                }
            }
        }

        private bool _isMouseOver;

        public DockIconGraphics(DockPanelGraphics panel, IDockIcon model, bool playAppear)
        {
            _panel = panel;
            Model = model;

            _targetSize = _panel.Dock.IconSlotSize;
            if (!playAppear) {
                _size = _targetSize;
            }
        }

        public void Update(float dt)
        {
            Model.Update();

            switch (State) {
                case DisplayState.Display:
                {
                    if (UpdateToTarget(ref _size, _targetSize, dt * _panel.Dock.IconScaleSpeed))
                        _panel.Dock.SetDirty();
                    
                    break;
                }

                case DisplayState.Disappear:
                {
                    if (UpdateToTarget(ref _size, 0, dt * _panel.Dock.IconScaleSpeed)) {
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

        public void SetDistanceToCursor(float ratio, bool fast) {

            if (!Model.Hovered) {
                ratio = 0;
            }            
            var size = _panel.Dock.IconSlotSize + (_panel.Dock.IconHoverValue * ratio);
            _targetSize = size ;

            if (fast) {
                Size = _targetSize;
            }
        }

        public void MouseEnter()
        {
            _isMouseOver = true;
            Model.MouseEnter();
        }

        public void MouseLeave()
        {
            _isMouseOver = false;
            Model.MouseLeave();
        }

        public void Render(Graphics graphics) {
            if (State != DisplayState.Display) {
                return;
            }

            var displayFlash = DateTime.Now.Millisecond > 500;
            if (Flash && displayFlash) {
                _panel.Dock.SelectedSkin.Draw(SkinElement.HighlightBg,  graphics, 0, 0, Width, Height);
            }

            var padding = _panel.Dock.SelectedSkin.IconPadding;

            var state = graphics.Save();
            graphics.TranslateTransform(padding, padding);
            Model.Render(graphics, Width - padding * 2, Height - padding * 2, new Rectangle(-padding, -padding, (int)Width, (int)Height));
            graphics.Restore(state);

            if (Flash && displayFlash) {
                _panel.Dock.SelectedSkin.Draw(SkinElement.HighlightFg,  graphics, 0, 0, Width, Height);
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
