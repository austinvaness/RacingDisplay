using avaness.RacingMod.Race;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdMode : AdminCommand
    {
        public override string Id => "mode";

        public override string Usage => ": Toggles timed mode.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            RacingMapSettings mapSettings = race.MapSettings;
            mapSettings.TimedMode = !mapSettings.TimedMode;
            if (mapSettings.TimedMode)
                ShowChatMsg(p, "Mode is now timed.");
            else
                ShowChatMsg(p, "Mode is now normal.");
        }
    }
}
