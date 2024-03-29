﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ProtoDock.Api;
using ProtoDock.Core;

namespace ProtoDock {
	public class DockPanelGraphics
	{
		public enum State
		{
			Idle,
			LeftDown,
			DragIcon,
			DragData
		}
		
		public readonly DockGraphics Dock;
		public readonly DockPanel Model;

		private State _state;

		private PointF _position;
		public void Move(float x, float y) => _position = new PointF(x, y);
		public float Left => _position.X;
		public float Top => _position.Y;
		public float Right => _position.X + Width;
		public float Bottom => _position.Y + Height;
		
		public float Width => _drawSize.Width;
		public float Height => Dock.SelectedSkin.PanelPadding.Top + Dock.IconSlotSize + Dock.SelectedSkin.PanelPadding.Bottom;
		
		private SizeF _drawSize;
		public float DrawWidth => _drawSize.Width;
		public float DrawHeight => _drawSize.Height;

		private readonly List<DockIconGraphics> _icons = new List<DockIconGraphics>();
		internal IReadOnlyList<DockIconGraphics> Icons => _icons;
		
		public bool IsMouseOver { get; private set; }
		private PointF _mouseDownPoint;
		private PointF _mousePosition;
		private DockIconGraphics _draggedIcon;
		
		public DockPanelGraphics(DockGraphics dock, DockPanel model)
		{
			Dock = dock;
			Model = model;
		}

		public void AddIcon(IDockIcon model, bool playAppear)
		{
			var icon = new DockIconGraphics(this, model, playAppear);
			_icons.Add(icon);
		}
		
		internal void RemoveIcon(IDockIcon model, bool playDisappear) {
			var index = _icons.FindIndex(d => d.Model == model);
			if (index == -1)
				return;
        
			_icons[index].Hide(playDisappear);
            
			Dock.SetDirty();
		}

		internal void SetFlashIcon(IDockIcon model, bool value) {
			var index = _icons.FindIndex(d => d.Model == model);
			if (index == -1) {
				return;
			}

			_icons[index].Flash = value;
		}

		public void MouseDown(float x, float y, MouseButtons button)
		{
			GetIconFromX(x, out _draggedIcon, out _, out _);
			
			switch (button) {
				case MouseButtons.Left:
					_mouseDownPoint = new PointF(x, y);
					SetState(State.LeftDown);
					break;
			}
			
			_draggedIcon?.Model.MouseDown(0, 0, button);
		}

		public bool MouseUp(float x, float y, MouseButtons button) {
			var icon = _draggedIcon;
			_draggedIcon = null;
			SetState(State.Idle);

			return icon?.Model.MouseUp(0, 0, button) ?? false;
		}

		public void MouseMove(float x, float y) {
			switch (_state) {
				case State.Idle:
					break;
				
				case State.LeftDown:
					if (MathF.Abs(_mouseDownPoint.X - x) > Dock.IconSlotSize * 0.5f) {
						SetState(State.DragIcon);
					}
					break;

				case State.DragIcon:
					if (GetDropIndex(x, out var destIndex, out var left)) {
						if (GetIconIndex(_draggedIcon, out var srcIndex)) {
							if (srcIndex != destIndex) {
								_icons[srcIndex] = _icons[destIndex];
								_icons[destIndex] = _draggedIcon;
								Dock.SetDirty();
							}
						}
					}
					break;
			}
		}

		internal void Update(float dt) {
			for (var i = 0; i < Model.Mediators.Count; i++)
			{
				Model.Mediators[i].Update();
			}

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
			CalculateSize(out _drawSize);

			for (var i = 0; i < _icons.Count; i++) {
				if (_icons[i].Flash) {
					Dock.SetDirty();
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

				case State.DragIcon:
					Dock.ClearIconsDistance();
					break;
			}

		}

		private void CalculateSize(out SizeF dockSize)
		{
			var iconsCount = _icons.Count;
			var iconLeft = 0f;
			var maxIconHeight = 0f;

			for (var i = 0; i < iconsCount; i++)
			{
				var icon = _icons[i];
				iconLeft += icon.Width;
				maxIconHeight = MathF.Max(maxIconHeight, icon.Height);
			}

			var panelWidth = iconLeft + Math.Max(0, iconsCount - 1) * Dock.IconSpace + Dock.SelectedSkin.PanelPadding.Horizontal;
			if (_icons.Count == 0) {
				panelWidth = 0;
			}
				
			var panelHeight = Math.Max(Dock.IconSlotSize + Dock.SelectedSkin.PanelPadding.Vertical, maxIconHeight + Dock.SelectedSkin.PanelPadding.Vertical);
			dockSize = new SizeF(
				panelWidth,
				panelHeight
			);
		}
		
		internal void Render(Graphics graphics)
		{
			var state = graphics.Save();
			
			if (_icons.Count > 0)
				Dock.SelectedSkin.Panel?.Draw(graphics, new SizeF(Width, Height));
			
			graphics.TranslateTransform(
				Dock.SelectedSkin.PanelPadding.Left,
				Dock.SelectedSkin.PanelPadding.Top);
			
			RenderIcons(graphics);
			RenderDropTarget(graphics);
			
			graphics.TranslateTransform(
				-Dock.SelectedSkin.PanelPadding.Left,
				-Dock.SelectedSkin.PanelPadding.Top);
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
                        var vOffset = icon.Height - Dock.IconSlotSize;
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
                Dock.IconSlotSize);

            graphics.FillRectangle(
                SystemBrushes.Highlight, 
                x-2,
                0,
                4,
                Dock.IconSlotSize);
        }
        
        
        internal bool GetIconFromX(float x, out DockIconGraphics outIcon, out int outIndex, out float outLeft) {
	        var left = 0f;
	        
            for (var i = 0; i < _icons.Count; i++)
            {
                var icon = _icons[i];

                if (x > left - Dock.IconSpace * 0.5f && x <= left + icon.Width + Dock.IconSpace * 0.5f)
                {
                    outIcon =  icon;
					outIndex = i;
					outLeft = left;
                    return true;
                }


                left += icon.Width + Dock.IconSpace;
            }

            outIcon = default;
			outIndex = default;
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
                    var vOffset = value.Height - Dock.IconSlotSize;
                    top = -vOffset;
                    break;

                default:
                    throw new ArgumentException(Dock.Position.ToString());
            }
        }

        private bool GetDropIndex(float x, out int outIndex, out float outX)
        {
            if (!GetIconFromX(x, out var icon, out var _, out var left)) {
                outIndex = default;
                outX = default;
                return false;
            }			
            outIndex = _icons.IndexOf(icon);
            outX = left;

			if (x > left + icon.Width)
            {
                outIndex += 1;
                outX += icon.Width;
            }

			outIndex = Math.Min(_icons.Count - 1, outIndex);

            return true;
        }
        
        private bool GetIconIndex(DockIconGraphics icon, out int outIndex) {
	        for (var i = 0; i < _icons.Count; i++) {
		        if (icon == _icons[i]) {
			        outIndex = i;
			        return true;
		        }
	        }
	        
	        outIndex = default;
	        return false;
        }
	}
}