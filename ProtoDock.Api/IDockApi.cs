using System;
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

    public interface IDockApi
    {
        IntPtr HInstance { get; }

        IReadOnlyList<IDockPlugin> Plugins { get; }
        void SetDirty();
        void Flush();

        void DrawSkin(SkinElement element, Graphics g, float x, float y, float width, float height);
    }
}
