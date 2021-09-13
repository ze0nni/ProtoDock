using System;
using System.Collections.Generic;
using System.Drawing;
using ProtoDock.Api;
using ProtoDock.Core;

namespace ProtoDock {
	public class DockPanelGraphics
	{
		public enum State
		{
			Idle,
			LeftDown,
			Drag,
			DragData
		}
		
		public readonly DockGraphics Dock;
		public readonly DockPanel Model;

		private State _state;

		private PointF _position;
		private void Move(float x, float y) => new PointF(x, y);
		
		private SizeF _size;
		public float Width => _size.Width;
		public float Height => _size.Height;

		private readonly List<DockIconGraphics> _icons = new List<DockIconGraphics>();
		
		public bool IsMouseOver { get; private set; }
		private PointF _mouseDownPoint;
		private PointF _mousePosition;
		private DockIconGraphics _draggedIcon;
		
		public DockPanelGraphics(DockGraphics dock, DockPanel model)
		{
			Dock = dock;
			Model = model;
		}

		public void AddIcon(IDockIcon model)
		{
			var icon = new DockIconGraphics(this, model);
			_icons.Add(icon);
		}
		
		internal void RemoveIcon(IDockIcon model) {
			var index = _icons.FindIndex(d => d.Model == model);
			if (index == -1)
				return;
        
			_icons[index].Hide();
            
			Dock.SetDirty();
		}

		internal void Click(float x, float y) {
			
		}
		
		internal bool ContextClick(float x, float y) {
			return false;
		}
		
		internal void Update(float dt) {
			CalculateSize(out _size);
			
			for (var i = _icons.Count - 1; i >= 0; i--)
			{
				var icon = _icons[i];
				if (icon.State == DockIconGraphics.DisplayState.Hidden)
				{
					_icons.RemoveAt(i);
					Dock.SetDirty();
				}
				else {
					icon.Update(dt);
				}
			}
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
		
		private void CalculateSize(out SizeF dockSize)
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

			var dockWidth = iconsWidthSum + Math.Max(0, iconsCount - 1) * Dock.IconSpace;
			var dockHeight = Dock.IconSize;
			dockSize = new SizeF(
				dockWidth,
				dockHeight
			);
		}
		
		internal void Render(Graphics graphics)
		{
			graphics.Clear(Color.Transparent);

			var state = graphics.Save();
			
			RenderIcons(graphics);
			RenderDropTarget(graphics);
		}
		
		
        private void RenderIcons(Graphics graphics)
        {
            var state = graphics.Save();
            
            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                switch (Dock.Position)
                {
                    case Position.Top:
                        icon.Render(graphics);
                        break;

                    case Position.Bottom:
                        var vOffset = icon.Height - Dock.IconSize;
                        graphics.TranslateTransform(0, -vOffset);
                        icon.Render(graphics);
                        graphics.TranslateTransform(0, vOffset);
                        break;

                    default:
                        throw new ArgumentException(Dock.Position.ToString());
                }
                graphics.TranslateTransform(Dock.IconSpace + icon.Width, 0);
            }

            graphics.Restore(state);
        }

        private void RenderDropTarget(Graphics graphics)
        {
            if (_state != State.DragData)
            {
                return;
            }

            if (!GetDropIndex(_mousePosition.X, out var index, out var x))
            {
                return;
            }

            graphics.FillRectangle(
                SystemBrushes.ButtonShadow,
                x - 1,
                1,
                4,
                Dock.IconSize);

            graphics.FillRectangle(
                SystemBrushes.Highlight, 
                x-2,
                0,
                4,
                Dock.IconSize);
        }
        
        
        private bool GetIconFromX(float x, out DockIconGraphics outIcon, out float outLeft) {
	        var left = 0f;
	        
            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                if (x > left - Dock.IconSpace * 0.5f && x < left + icon.Width + Dock.IconSpace * 0.5f)
                {
                    outIcon =  icon;
                    outLeft = left;
                    return true;
                }


                left += icon.Width + Dock.IconSpace;
            }

            outIcon = default;
            outLeft = default;
            return false;
        }

        private void GetIconPos(DockIconGraphics value, out float left, out float top) {
	        left = 0f;

            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                if (icon == value)
                {
                    break;
                }

                left += icon.Width + Dock.IconSpace;
            }

            switch (Dock.Position)
            {
                case Position.Top:
                    top = 0;                    
                    break;

                case Position.Bottom:
                    var vOffset = value.Height - Dock.IconSize;
                    top = -vOffset;
                    break;

                default:
                    throw new ArgumentException(Dock.Position.ToString());
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
	}
}