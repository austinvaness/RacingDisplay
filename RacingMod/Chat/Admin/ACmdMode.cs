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
            if (cmd.Length < 3)
            {
                ShowChatMsg(p, 
                    $"Usage:\n{RacingCommands.prefix} {Id} <mode>\n" +
                    "distance: Racer positions as distance\n" +
                    "interval: Racer positions as time\n" +
                    "qualify: Sort racers by best time\n" +
                    $"Current mode: {mapSettings.Mode}");
                return;
            }

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
            return len == 2 || len == 3;
        }
    }
}
