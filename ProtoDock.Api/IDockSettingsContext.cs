using System;
using System.Collections.Generic;

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

    public static class IDockSettingsDisplayTools
    {
        public static void Combo<T>(
            this IDockSettingsDisplay display,
            T selected,
            out Func<T> getValue,
            out Action<T> addItem,
            out Action<T> removeItem,
            Action<T> onValueChanged) where T: Enum
        {
            getValue = default;
            addItem = default;
            removeItem = default;
            
            display.Combo<T>(
                selected,
                (T[])Enum.GetValues(typeof(T)),
                out getValue,
                out addItem,
                out removeItem,
                onValueChanged);
        }
    }
}
