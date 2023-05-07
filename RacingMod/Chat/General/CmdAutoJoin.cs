using avaness.RacingMod.Race;
using avaness.RacingMod.Racers;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdAutoJoin : ChatCommand
    {
        public override string Id => "autojoin";

        public override string Usage => ": Stay in a timed race after completion.";

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            race.ToggleAutoJoin(p);
        }
    }
}
