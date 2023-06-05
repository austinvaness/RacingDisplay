using avaness.RacingMod.Race;
using avaness.RacingMod.Race.Modes;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdMode : AdminCommand
    {
        public override string Id => "mode";

        public override string Usage => "<mode>: Set the mode to distance, interval, or qualify.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            RacingMapSettings mapSettings = race.MapSettings;
            byte mode;
            if (TrackModeBase.TryParseType(cmd[2], out mode))
            {
                mapSettings.ModeType = mode;
                ShowChatMsg(p, "Mode is now " + mapSettings.Mode);
            }
            else
            {
                ShowChatMsg(p, $"Error, {cmd[2]} is not distance, interval, or qualify.");
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len == 3;
        }
    }
}
