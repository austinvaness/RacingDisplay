using avaness.RacingMod.Race;
using avaness.RacingMod.Racers;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdRecord : ChatCommand
    {
        public override string Id => "record";

        public override string Usage => ": (obsolete)";

        public override bool Hidden => true;

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            if (race.MapSettings.TimedMode)
            {
                StaticRacerInfo info = race.Racers.GetStaticInfo(p);
                if (info.Recorder == null)
                {
                    ShowMsg(p, "Your best time is now being recorded.");
                    info.CreateRecorder();
                }
                else
                {
                    ShowMsg(p, "Your recorder is already active.");
                }
            }
            else
            {
                ShowMsg(p, "Recording only works in timed mode.");
            }
            return;
        }
    }
}
