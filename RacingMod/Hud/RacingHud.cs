using Draygo.API;
using avaness.RacingMod.Font;
using VRage.Input;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using System.Text;
using System;

namespace avaness.RacingMod.Hud
{
    public class RacingHud
    {
        public readonly HudAPIv2 api;
        private readonly RacingPreferences config;
        private readonly RacingMapSettings mapSettings;
        private Vector2D activeHudPosition = new Vector2D(-0.95, 0.90);

        public HudAPIv2.HUDMessage activeRacersHud;
        private HudAPIv2.MenuRootCategory menuRoot;
        private HudAPIv2.MenuKeybindInput hideHudInput;
        private HudAPIv2.MenuItem autoRecordInput;
        private MapSettingsMenu settingsMenu;

        public event Action OnEnabled;

        private StringBuilder text = new StringBuilder("Loading...");
        public StringBuilder Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                activeRacersHud.Message = value;
            }
        }

        public RacingHud(RacingPreferences config, RacingMapSettings mapSettings)
        {
            if (RacingSession.Instance.Hud != null)
                throw new Exception("Error, cannot create another hud instance!");
            api = new HudAPIv2(CreateHud);
            this.config = config;
            this.mapSettings = mapSettings;
        }

        public void Unload()
        {
            api.Unload();
            OnEnabled = null;
            if(settingsMenu != null)
                settingsMenu.Unload();
            config.OnHideHudChanged -= UpdateHideHud;
        }

        private void CreateHud()
        {
            new HudAPIFont(RacingConstants.fontData, RacingConstants.fontId).CreateFont();

            activeRacersHud = new HudAPIv2.HUDMessage(text, activeHudPosition, HideHud: false, Font: RacingConstants.fontId, Blend: BlendTypeEnum.PostPP);

            menuRoot = new HudAPIv2.MenuRootCategory("Racing Display", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Racing Display");
            hideHudInput = new HudAPIv2.MenuKeybindInput("Hide Hud - " + config.HideHud.ToString(), menuRoot, "Press any Key [Hide Hud]", SetHideHudKey);
            config.OnHideHudChanged += UpdateHideHud;
            autoRecordInput = new HudAPIv2.MenuItem("Auto Record - " + BoolToString(config.AutoRecord), menuRoot, SetAutoRecord);
            config.OnAutoRecordChanged += UpdateAutoRecord;

            settingsMenu = new MapSettingsMenu(mapSettings);

            if(OnEnabled != null)
            {
                OnEnabled.Invoke();
                OnEnabled = null;
            }

            RacingSession.Instance.Net.Register(RacingConstants.packetMainId, ReceiveHudText);
        }

        private void SetAutoRecord()
        {
            config.AutoRecord = !config.AutoRecord;
        }

        private void UpdateAutoRecord(bool autoRecord)
        {
            autoRecordInput.Text = "Auto Record - " + BoolToString(autoRecord);
        }

        private void ReceiveHudText(byte[] data)
        {
            try
            {
                string text = Encoding.UTF8.GetString(data);
                this.text.Clear();
                this.text.Append(text);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        public void ToggleUI()
        {
            if (activeRacersHud == null)
                return;
            activeRacersHud.Visible = !activeRacersHud.Visible;
        }

        private void SetHideHudKey (MyKeys key, bool shift, bool ctrl, bool alt)
        {
            config.HideHud = new Keybind(key, shift, ctrl, alt);
        }
        private void UpdateHideHud(Keybind keybind)
        {
            hideHudInput.Text = "Hide Hud - " + keybind.ToString();
        }

        private string BoolToString(bool b)
        {
            if (b)
                return "On";
            else
                return "Off";
        }
    }
}
