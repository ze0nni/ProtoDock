﻿using System.Linq;
using ProtoDock.Api;

namespace ProtoDock.Core {
	public class DockViewSettings : IDockSettingsSource {

        private readonly DockGraphics _graphics;

        public DockViewSettings(DockGraphics graphics) {
            _graphics = graphics;
        }

        public void Display(IDockSettingsDisplay display)
        {
            display.Header("View");
            
            display.Combo(
                "Skin",
                _graphics.SelectedSkin,
                _graphics.Skins.List,
                out var skins,               
                s =>
                {
                    _graphics.UpdateSkin(s);
                });

            display.Buttons()
                .Add("Reload", () => {
                    var selectedName = _graphics.SelectedSkin.Name;
                    _graphics.Skins.Reload();
                    _graphics.UpdateSkin(_graphics.Skins.List.FirstOrDefault(s => s.Name == selectedName));
                    skins.update(_graphics.Skins.List);
                    skins.@select(_graphics.SelectedSkin);
                    
                    _graphics.SetDirty();
                });
            
            display.Buttons()
                .Add("Flash", () => {
                    display.FlashWindow();
                });
            
            display.Number(
                "Icons size",
                _graphics.IconSize,
                DockGraphics.MIN_ICON_SIZE,
                DockGraphics.MAX_ICON_SIZE,
                out _,
                v =>
                {
                    _graphics.UpdateIconSize(v);
                });

            display.Number(
                "Hovered icon value",
                _graphics.IconHoverValue,
                0,
                DockGraphics.MAX_ICON_SIZE,
                out _,
                v =>
                {
                    _graphics.UpdateIconHoverValue(v);
                });

            display.Number(
                "Icons space",
                _graphics.IconSpace,
                DockGraphics.MIN_ICON_SPACE,
                DockGraphics.MAX_ICON_SPACE,
                out _,
                v =>
                {
                    _graphics.UpdateIconSpace(v);
                });

            display.Toggle(
                "Enable hint",
                _graphics.EnableHint,
                out _,
                out _,
                v =>
                {
                    _graphics.UpdateEnableHint(v);
                });

            display.Number(
                "Hint font size",
                _graphics.Hint.FontSize,
                HintWindow.MIN_FONT_SIZE,
                HintWindow.MAX_FONT_SIZE,
                out _,
                v =>
                {
                    _graphics.Hint.UpdateFontSize(v);
                });

            display.Header("Behaviour");

            display.Combo<string>(
                "Screen",
                _graphics.ActiveScreen.DeviceName,
                System.Windows.Forms.Screen.AllScreens.Select(s => s.DeviceName),
                out _,
                s =>
                {
                    _graphics.UpdateScreen(s);
                });

            display.Combo<Position>(
                "Position",
                _graphics.Position,
                out _,
                p =>
                {
                    _graphics.UpdatePosition(p);
                });

            display.Combo<AutohideRule>(
                "Auto hide",
                _graphics.Autohide,
                out _,
                v =>
                {
                    _graphics.UpdateAutoHide(v);
                });
        }

        public override string ToString() {
            return "Dock";
        }
    }
}