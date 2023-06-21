using avaness.RacingMod.Race;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdDebug : ChatCommand
    {
        public override string Id => "debug";

        public override string Usage => ": Toggles debug mode.";

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            if (MyAPIGateway.Session.Player != p)
            {
                ShowChatMsg(p, "Debug view only works for the server host.");
            }
            else
            {
                RacingSession.Instance.ToggleDebug();
                ShowChatMsg(p, "Toggled debug view.");
            }
        }
    }
}
