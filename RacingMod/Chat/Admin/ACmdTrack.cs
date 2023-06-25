using avaness.RacingMod.Race;
using System.Collections.Generic;
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
                    AppendTracks(sb, session.GetNodeManagers());
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
                StringBuilder sb = new StringBuilder();
                sb.Append("Usage:").AppendLine();
                sb.Append(RacingCommands.prefix).Append(' ').Append(Id).Append(' ').Append(Usage).AppendLine();
                sb.Append("Current: ").Append(session.CurrentNodes.Id).AppendLine();
                AppendTracks(sb, session.GetNodeManagers());
                ShowChatMsg(p, sb.ToString());
            }
        }

        private void AppendTracks(StringBuilder sb, IEnumerable<NodeManager> nodeManagers)
        {
            sb.Append("Tracks: ");
            bool first = true;
            foreach (NodeManager n in nodeManagers.Where(x => x.Count >= 2))
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                sb.Append(n.Id);
            }
        }

        protected override bool ValidateLength(int len)
        {
            return len == 2 || len == 3;
        }
    }
}
