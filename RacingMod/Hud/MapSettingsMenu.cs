using avaness.RacingMod.Race.Modes;
using Draygo.API;
using System;

namespace avaness.RacingMod.Hud
{
    class MapSettingsMenu
    {
        private readonly HudAPIv2.MenuRootCategory adminRoot;
        private readonly HudAPIv2.MenuTextInput numLapsInput;
        private readonly HudAPIv2.MenuTextInput modeInput;
        private readonly HudAPIv2.MenuItem onTrackStartInput;
        private readonly HudAPIv2.MenuItem loopedInput;
        private readonly RacingMapSettings mapSettings;

        public MapSettingsMenu(RacingMapSettings mapSettings)
        {
            this.mapSettings = mapSettings;
            adminRoot = new HudAPIv2.MenuRootCategory("Racing Display", HudAPIv2.MenuRootCategory.MenuFlag.AdminMenu, "Racing Display Settings");
            numLapsInput = new HudAPIv2.MenuTextInput("Laps - " + mapSettings.NumLaps, adminRoot, "Enter number of laps:", OnNumLapsChanged);
            mapSettings.NumLapsChanged += UpdateNumLaps;
            modeInput = new HudAPIv2.MenuTextInput("Mode - " + mapSettings.Mode, adminRoot, "Enter distance, interval, or qualify:", OnModeChanged);
            mapSettings.ModeChanged += UpdateMode;
            onTrackStartInput = new HudAPIv2.MenuItem("Strict Start - " + BoolToString(mapSettings.StrictStart), adminRoot, OnTrackStartChanged);
            mapSettings.StrictStartChanged += UpdateStrictStart;
            loopedInput = new HudAPIv2.MenuItem("Looped - " + BoolToString(mapSettings.Looped), adminRoot, OnLoopedChanged, mapSettings.NumLaps == 1);
            mapSettings.LoopedChanged += UpdateLooped;
        }

        public void Unload()
        {
            mapSettings.NumLapsChanged -= UpdateNumLaps;
            mapSettings.ModeChanged -= UpdateMode;
            mapSettings.StrictStartChanged -= UpdateStrictStart;
        }

        private string BoolToString(bool b)
        {
            if (b)
                return "On";
            else
                return "Off";
        }

        private void OnTrackStartChanged()
        {
            mapSettings.StrictStart = !mapSettings.StrictStart;
        }
        public void UpdateStrictStart(bool strict)
        {
            onTrackStartInput.Text = "Strict Start - " + BoolToString(strict);
        }

        private void OnModeChanged(string text)
        {
            byte mode;
            if (TrackModeBase.TryParseType(text.Trim(), out mode))
                mapSettings.ModeType = mode;
        }
        public void UpdateMode(TrackModeBase mode)
        {
            modeInput.Text = "Mode - " + mode;
        }

        private void OnNumLapsChanged(string text)
        {
            int result;
            if (int.TryParse(text, out result))
                mapSettings.NumLaps = result;
        }
        public void UpdateNumLaps(int laps)
        {
            numLapsInput.Text = "Laps - " + laps;
            loopedInput.Interactable = laps == 1;
        }

        private void OnLoopedChanged()
        {
            mapSettings.Looped = !mapSettings.Looped;
        }
        public void UpdateLooped(bool looped)
        {
            loopedInput.Text = "Looped - " + BoolToString(looped);
        }
    }
}
