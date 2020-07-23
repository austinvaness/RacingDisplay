using Draygo.API;
using VRage.Input;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingMod
{
    public partial class RacingSession
    {
        private bool hudReady = false;
        private HudAPIv2 textApi;
        private HudAPIv2.MenuRootCategory menuRoot;
        private HudAPIv2.MenuRootCategory adminRoot;
        private HudAPIv2.HUDMessage activeRacersHud;
        private HudAPIv2.HUDMessage infoHud;
        private HudAPIv2.MenuKeybindInput nextRacerInput;
        private HudAPIv2.MenuKeybindInput prevRacerInput;
        private HudAPIv2.MenuKeybindInput stopSpecInput;
        private HudAPIv2.MenuKeybindInput hideHudInput;
        private HudAPIv2.MenuTextInput numLapsInput;
        private HudAPIv2.MenuItem timedModeInput;
        private HudAPIv2.MenuItem onTrackStartInput;
        private HudAPIv2.MenuItem loopedInput;

        private Vector2D activeHudPosition = new Vector2D(-0.95, 0.90);
        private Vector2D infoHudPosition = new Vector2D(1, 1);

        private void CreateHudItems ()
        {
            activeRacersHud = new HudAPIv2.HUDMessage(activeRacersText, activeHudPosition, HideHud: false, Font: "monospace", Blend: BlendTypeEnum.PostPP);
            infoHud = new HudAPIv2.HUDMessage(infoHudText, infoHudPosition, HideHud: true, Blend: BlendTypeEnum.PostPP);

            adminRoot = new HudAPIv2.MenuRootCategory("Racing Display", HudAPIv2.MenuRootCategory.MenuFlag.AdminMenu, "Racing Display Settings");
            {
                numLapsInput = new HudAPIv2.MenuTextInput("Laps - " + MapSettings.NumLaps, adminRoot, "Enter number of laps:", OnNumLapsChanged);
                MapSettings.NumLapsChanged += UpdateUI_NumLaps;
                timedModeInput = new HudAPIv2.MenuItem("Timed Mode - " + BoolToString(MapSettings.TimedMode), adminRoot, OnTimedModeChanged);
                MapSettings.TimedModeChanged += UpdateUI_TimedMode;
                onTrackStartInput = new HudAPIv2.MenuItem("Strict Start - " + BoolToString(MapSettings.StrictStart), adminRoot, OnTrackStartChanged);
                MapSettings.StrictStartChanged += UpdateUI_StrictStart;
                loopedInput = new HudAPIv2.MenuItem("Looped - " + BoolToString(MapSettings.Looped), adminRoot, OnLoopedChanged, MapSettings.NumLaps == 1);
                MapSettings.LoopedChanged += UpdateUI_Looped;
            }

            menuRoot = new HudAPIv2.MenuRootCategory("Race Spectator", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Spectator Settings");
            nextRacerInput = new HudAPIv2.MenuKeybindInput("Next Racer - " + config.NextPlayer.ToString(), menuRoot, "Press any Key [Next Racer]", SetNextPlayerKey);
            prevRacerInput = new HudAPIv2.MenuKeybindInput("Previous Racer - " + config.PrevPlayer.ToString(), menuRoot, "Press any Key [Previous Racer]", SetPrevPlayerKey);
            stopSpecInput = new HudAPIv2.MenuKeybindInput("Stop Spectating - " + config.StopSpec.ToString(), menuRoot, "Press any Key [Stop Spectating]", SetStopSpecKey);
            hideHudInput = new HudAPIv2.MenuKeybindInput("Hide Hud - " + config.HideHud.ToString(), menuRoot, "Press any Key [Hide Hud]", SetHideHudKey);
            hudReady = true;
        }

        public void UpdateUI_Spectator()
        {
            if (hudReady)
            {
                nextRacerInput.Text = "Next Racer - " + config.NextPlayer.ToString();
                prevRacerInput.Text = "Previous Racer - " + config.PrevPlayer.ToString();
                hideHudInput.Text = "Hide Hud - " + config.HideHud.ToString();
            }
        }

        private string BoolToString (bool b)
        {
            if (b)
                return "On";
            else
                return "Off";
        }

        private void OnTrackStartChanged ()
        {
            MapSettings.StrictStart = !MapSettings.StrictStart;
        }
        public void UpdateUI_StrictStart(bool strict)
        {
            if(hudReady)
                onTrackStartInput.Text = "Strict Start - " + BoolToString(strict);
        }

        private void OnTimedModeChanged ()
        {
            MapSettings.TimedMode = !MapSettings.TimedMode;
        }
        public void UpdateUI_TimedMode(bool timed)
        {
            if (hudReady)
                timedModeInput.Text = "Timed Mode - " + BoolToString(timed);
        }

        private void OnNumLapsChanged (string text)
        {
            int result;
            if(int.TryParse(text, out result))
                MapSettings.NumLaps = result;
        }
        public void UpdateUI_NumLaps(int laps)
        {
            if (hudReady)
                numLapsInput.Text = "Laps - " + laps;
            loopedInput.Interactable = laps == 1;
        }

        private void OnLoopedChanged ()
        {
            MapSettings.Looped = !MapSettings.Looped;
        }
        public void UpdateUI_Looped (bool looped)
        {
            if (hudReady)
                loopedInput.Text = "Looped - " + BoolToString(looped);
        }

        private void SetPrevPlayerKey (MyKeys key, bool arg1, bool arg2, bool arg3)
        {
            config.PrevPlayer = new Keybind(key, arg1, arg2, arg3);
            prevRacerInput.Text = "Previous Racer - " + config.PrevPlayer.ToString();
        }
        private void SetNextPlayerKey (MyKeys key, bool arg1, bool arg2, bool arg3)
        {
            config.NextPlayer = new Keybind(key, arg1, arg2, arg3);
            nextRacerInput.Text = "Next Racer - " + config.NextPlayer.ToString();
        }
        private void SetStopSpecKey (MyKeys key, bool arg1, bool arg2, bool arg3)
        {
            config.StopSpec = new Keybind(key, arg1, arg2, arg3);
            stopSpecInput.Text = "Stop Spectating - " + config.StopSpec.ToString();
        }
        private void SetHideHudKey (MyKeys key, bool arg1, bool arg2, bool arg3)
        {
            config.HideHud = new Keybind(key, arg1, arg2, arg3);
            hideHudInput.Text = "Hide Hud - " + config.HideHud.ToString();
        }
    }
}
