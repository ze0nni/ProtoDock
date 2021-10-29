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

    public interface IDockSettingsDisplay {
        void Flush();
        void SetDirty();
        
        void Header(string text);
        void Combo<T>(
            string label,
            T selected,
            IEnumerable<T> items,
            out ICollectionController<T> controller,
            Action<T> onValueChanged);

        void List<T>(
            string label,
            T selected,
            IEnumerable<T> items,
            out ICollectionController<T> controller,
            Action<T> onValueChanged);
        
        void Toggle(
            string label,
            bool value,
            out Func<bool> getValue,
            out Action<bool> setValue,
            Action<bool> onValueChanged
            );

        void Number(
            string label,
            int value,
            int min,
            int max,
            out Action<int> setValue,
            Action<int> onValueChange
            );

        IButtonsPanel Buttons();
    }

    public interface ICollectionController<T>
    {
        T getValue();
        void addItem(T i);
        void removeItem(T i);
        void select(T i);
        void update(IEnumerable<T> items);
    }

    public interface IButtonsPanel {
        IButtonsPanel Add(string text, Action onClick);
    }

    public static class IDockSettingsDisplayTools
    {
        public static void Combo<T>(
            this IDockSettingsDisplay display,
            string label,
            T selected,
            out ICollectionController<T> controller,
            Action<T> onValueChanged) where T: Enum
        {
            controller = default;

            display.Combo<T>(
                label,
                selected,
                (T[])Enum.GetValues(typeof(T)),
                out controller,
                onValueChanged);
        }
    }
}
