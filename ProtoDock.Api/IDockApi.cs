using System;
using System.Collections.Concurrent;
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

    public enum AutohideRule
    {
        Never,
        Allways,
        OnFullscreen
    }
    public interface IDockApi
    {
        IntPtr HInstance { get; }
        IntPtr HWnd { get; }
        void Invoke(Action action);
        Position Position { get; }
        IReadOnlyList<IDockPlugin> Plugins { get; }
        void SetDirty();
        void Flush();
        void DrawSkin(SkinElement element, Graphics g, float x, float y, float width, float height);
        void DrawSkin(SkinElement element, Graphics g, Rectangle rect);
        bool GetPanelRect(IDockPanelMediator mediator, out Rectangle screenRect);
    }
}
