using avaness.RacingMod.Race;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdJoin : ChatCommand
    {
        public override string Id => "join";

        public override string Usage => ": Join the race.";
        public override string AdminUsage => "[name|all]: Join or force a player to join.";

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            if (cmd.Length > 2 && admin)
            {
                string arg = BuildString(cmd, 2);

                if (arg.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    List<IMyPlayer> players = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(players);
                    int count = 0;
                    foreach (IMyPlayer temp in players)
                    {
                        if (!race.Contains(temp.SteamUserId) && race.JoinRace(temp))
                            count++;
                    }
                    ShowChatMsg(p, $"Added {count} racers to the race.");
                }
                else
                {
                    IMyPlayer temp = RacingTools.GetPlayer(arg);
                    if (temp == null)
                        ShowChatMsg(p, $"No player was found with a name containing '{arg}'.");
                    else
                        race.JoinRace(temp, true);
                }
            }
            else
            {
                race.JoinRace(p);
            }
        }

        protected override bool ValidateLength(int len, bool admin)
        {
            return (!admin && len == 2) || admin;
        }
    }
}
