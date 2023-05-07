using Draygo.API;
using avaness.RacingMod.Font;
using VRage.Input;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using System.Text;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;

namespace avaness.RacingMod.Hud
{
    public class TextApiRacingHud : RacingHud
    {
        private readonly HudAPIv2 api;
        private readonly RacingPreferences config;
        private readonly RacingMapSettings mapSettings;
        private readonly Vector2D hudPosition = new Vector2D(-0.95, 0.90);
        private readonly List<IMyPlayer> players = new List<IMyPlayer>();

        public HudAPIv2.HUDMessage activeRacersHud;
        private HudAPIv2.MenuRootCategory menuRoot;
        private HudAPIv2.MenuKeybindInput hideHudInput;
        private HudAPIv2.MenuItem autoRecordInput;
        private MapSettingsMenu settingsMenu;

        public event Action OnEnabled;

        private TextApiRacingHud()
        { 
            
        }

        public TextApiRacingHud(RacingPreferences config, RacingMapSettings mapSettings)
        {
            api = new HudAPIv2(CreateHud);
            this.config = config;
            this.mapSettings = mapSettings;
        }

        public override RacingHud CreateTemporary()
        {
            return new TextApiRacingHud();
        }

        public override void Broadcast()
        {
            if (RacingSession.Instance.Runticks % 2 != 0)
                return;

            Net.Network net = RacingSession.Instance.Net;
            byte[] data = net.Prep(RacingConstants.packetMainId, Encoding.UTF8.GetBytes(text.ToString()));

            players.Clear();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (IMyPlayer p in players)
            {
                if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId)
                    continue;

                net.SendTo(data, p.SteamUserId);
            }
        }

        public override void Unload()
        {
            api.Unload();
            OnEnabled = null;
            if(settingsMenu != null)
                settingsMenu.Unload();
            config.OnHideHudChanged -= UpdateHideHud;
            config.OnAutoRecordChanged -= UpdateAutoRecord;
        }

        private void CreateHud()
        {
            new HudAPIFont(RacingConstants.fontData, RacingConstants.fontId).CreateFont();

            activeRacersHud = new HudAPIv2.HUDMessage(text, hudPosition, HideHud: false, Font: RacingConstants.fontId, Blend: BlendTypeEnum.PostPP);

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

        public override void ToggleUI()
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

        public override RacingHud Append(Color color)
        {
            text.Append("<color=").Append(color.R).Append(',').Append(color.G).Append(',').Append(color.B).Append('>');
            return this;
        }
    }
}
