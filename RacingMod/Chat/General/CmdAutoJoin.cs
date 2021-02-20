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
            RacingMapSettings mapSettings = race.MapSettings;
            if (mapSettings.TimedMode && mapSettings.Looped)
            {
                if (!race.Contains(p.SteamUserId) && !race.JoinRace(p))
                    return;

                StaticRacerInfo info = race.Racers.GetStaticInfo(p);
                if (info.AutoJoin)
                    ShowChatMsg(p, "You will leave the race after finishing.");
                else
                    ShowChatMsg(p, "Your timer will reset after finishing.");
                info.AutoJoin = !info.AutoJoin;
            }
            else
            {
                ShowChatMsg(p, "Auto join only works for looped timed races.");
            }
        }
    }
}
