using avaness.RacingMod.Race;
using avaness.RacingMod.Racers;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdGrant : AdminCommand
    {
        public override string Id => "grant";

        public override string Usage => "<name>: Fixes a 'missing' error on the ui.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            RacerStorage racers = race.Racers;
            StaticRacerInfo info;
            if (racers.GetStaticInfo(cmd[2], out info) && info.OnTrack)
            {
                ShowChatMsg(p, $"Reset {info.Name}'s missing status.");
                race.Nodes.ResetPosition(info);
            }
            else
            {
                ShowChatMsg(p, $"No racer was found with a name containing '{cmd[2]}'.");
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len == 3;
        }
    }
}
