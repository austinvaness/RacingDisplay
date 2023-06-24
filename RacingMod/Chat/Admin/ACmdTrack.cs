using avaness.RacingMod.Race;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdTrack : AdminCommand
    {
        public override string Id => "track";

        public override string Usage => "<name>: Switches the currently selected track.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            RacingSession session = RacingSession.Instance;
            if(cmd.Length == 3)
            {
                NodeManager nodes;
                if (!session.TryGetNodeManager(cmd[2], out nodes))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("No track was found with the name '").Append(cmd[2]).Append("'.").AppendLine();
                    sb.Append("Tracks: ").AppendLine();
                    foreach (NodeManager n in session.GetNodeManagers().Where(x => x.Count >= 2))
                        sb.Append(n.Id);
                    ShowChatMsg(p, sb.ToString());
                }
                else
                {
                    session.SetNodeManager(nodes);
                    ShowChatMsg(p, $"Track set to {nodes.Id}");
                }
            }
            else
            {
                ShowChatMsg(p, $"Usage:\n{RacingCommands.prefix} {Id} {Usage}\nCurrent track: {session.CurrentNodes.Id}");
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len == 2 || len == 3;
        }
    }
}
