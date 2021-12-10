﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

namespace ProtoDock.Api
{
    public enum SkinElement
    {
        Dock,
        SelectedBg,
        SelectedFg,
        HighlightBg,
        HighlightFg
    }


    public enum Position
    {
        Top,
        Bottom
    }

    public interface IDockApi
    {
        IntPtr HInstance { get; }
        IntPtr HWnd { get; }
        Position Position { get;  }
        IReadOnlyList<IDockPlugin> Plugins { get; }
        
        void InvokeAction(Action action);
        void SetDirty();
        void Flush();
        
        void DrawSkin(SkinElement element, Graphics g, float x, float y, float width, float height);
        
    }
}
