using avaness.RacingMod.Race;
using avaness.RacingMod.Racers;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdLaps : AdminCommand
    {
        public override string Id => "laps";

        public override string Usage => "<number>: Changes the number of laps.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            RacingMapSettings mapSettings = race.MapSettings;
            int laps;
            if (int.TryParse(cmd[2], out laps))
            {
                mapSettings.NumLaps = laps;
                ShowChatMsg(p, $"Number of laps is now {laps}.");
            }
            else
            {
                ShowChatMsg(p, $"'{cmd[2]}' is not a valid number.");
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len == 3;
        }
    }
}
