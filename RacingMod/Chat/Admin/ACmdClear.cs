using avaness.RacingMod.Race;
using avaness.RacingMod.Racers;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdClear : AdminCommand
    {
        public override string Id => "clear";

        public override string Usage => "[name]: Removes finalist(s).";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            Race.Finish.FinishList finishers = race.Finishers;
            RacerStorage racers = race.Racers;
            if (cmd.Length == 2)
            {
                finishers.Clear();
                ShowChatMsg(p, "Cleared all finishers.");
            }
            else
            {
                string name = BuildString(cmd, 2);
                StaticRacerInfo info;
                if (racers.GetStaticInfo(name, out info))
                {
                    ShowChatMsg(p, $"Removed {info.Name} from the finalists.");
                    finishers.Remove(info);
                }
                else if (finishers.Remove(name))
                {
                    ShowChatMsg(p, $"Removed {name} from the finalists.");
                }
                else
                {
                    ShowChatMsg(p, $"No racer was found with a name containing '{name}'.");
                    return;
                }
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len >= 2;
        }
    }
}
