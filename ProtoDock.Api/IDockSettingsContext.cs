using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock.Api
{
    public interface IDockSettingsContext
    {
        void Register(IDockSettingsSource source);
    }

    public interface IDockSettingsSource
    {
        void Display(IDockSettingsDisplay display);
    }

    public interface IDockSettingsDisplay
    {
        void Header(string text);
        void Combo<T>(
            T selected,
            IEnumerable<T> items,
            out Func<T> getValue,
            out Action<T> addItem,
            out Action<T> removeItem,
            Action<T> onValueChanged);
    }
}
