using avaness.RacingMod.Race;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdStrictStart : AdminCommand
    {
        public override string Id => "strictstart";

        public override string Usage => ": Toggles if starting on the track is allowed.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            RacingMapSettings mapSettings = race.MapSettings;
            mapSettings.StrictStart = !mapSettings.StrictStart;
            if (mapSettings.StrictStart)
                ShowChatMsg(p, "Starting on the track is no longer allowed.");
            else
                ShowChatMsg(p, "Starting on the track is now allowed.");
        }
    }
}
