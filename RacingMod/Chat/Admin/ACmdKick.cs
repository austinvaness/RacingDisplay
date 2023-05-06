using avaness.RacingMod.Race;
using avaness.RacingMod.Racers;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdKick : AdminCommand
    {
        public override string Id => "kick";

        public override string Usage => "<name>: Removes a player from the race.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            IMyPlayer result = RacingTools.GetPlayer(cmd[2]);
            if (result == null)
            {
                ShowChatMsg(p, $"No player was found with a name containing '{cmd[2]}'.");
            }
            else
            {
                if (race.Contains(result.SteamUserId))
                {
                    ShowChatMsg(p, $"Removed {result.DisplayName} from the race.");
                    race.LeaveRace(result);
                    ShowMsg(result, "You have been kicked from the race.");
                }
                else
                {
                    ShowChatMsg(p, $"{result.DisplayName} is not in the race.");
                }
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len == 3;
        }
    }
}
