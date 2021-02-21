using avaness.RacingMod.Race;
using avaness.RacingMod.Race.Finish;
using avaness.RacingMod.Racers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdFinish : AdminCommand
    {
        public override string Id => "finish";

        public override string Usage => "<name> [position]: Adds a finisher to the list.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            FinishList finishers = race.Finishers;
            RacerStorage racers = race.Racers;
            IMyPlayer result = RacingTools.GetPlayer(cmd[2]);
            if (result == null)
            {
                ShowChatMsg(p, $"No racer was found with a name containing '{cmd[2]}'.");
            }
            else
            {
                StaticRacerInfo info = racers.GetStaticInfo(result);
                race.LeaveRace(result, false);
                if (cmd.Length == 4)
                {
                    int index;
                    if (!int.TryParse(cmd[3], out index))
                    {
                        ShowChatMsg(p, $"Error, {cmd[3]} is not a number.");
                        return;
                    }
                    index--;
                    if (index < 0 || index > finishers.Count)
                    {
                        ShowChatMsg(p, $"Error, {cmd[3]} must be between 1 and {finishers.Count + 1}.");
                        return;
                    }
                    finishers.Add(info, index);
                }
                else
                {
                    finishers.Add(info);
                }

                ShowChatMsg(p, $"Added {result.DisplayName} to the finishers list.");
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len == 3 || len == 4;
        }
    }
}
