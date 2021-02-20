using avaness.RacingMod.Chat.Admin;
using avaness.RacingMod.Chat.General;
using avaness.RacingMod.Race;
using avaness.RacingMod.Race.Finish;
using avaness.RacingMod.Racers;
using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat
{
    public class RacingCommands
    {
        public const string prefix = "/race";

        private readonly Track race;
        private readonly Dictionary<string, ChatCommand> commands = new Dictionary<string, ChatCommand>();
        private string helpMsg, adminHelpMsg;

        public RacingCommands(Track race)
        {
            this.race = race;

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
            if (RacingConstants.IsServer)
            {
                Add(new CmdAutoJoin(), "autojoin", "aj");
                Add(new CmdJoin(), "join", "j");
                Add(new CmdLeave(), "leave", "l");
                Add(new CmdRejoin(), "rejoin");
                Add(new ACmdClear(), new ACmdFinish(), new ACmdGrant(), new ACmdKick(), new ACmdLaps(), new ACmdLooped(), new ACmdMode(), new ACmdStrictStart(), new ACmdTimers());
                BuildText();
                Add(new CmdDebug());

                MyAPIGateway.Utilities.MessageRecieved += ReceiveCommand;
                RacingSession.Instance.Net.Register(RacingConstants.packetCmd, ReceiveCommandPacket);
            }
        }

        public void Unload()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
            MyAPIGateway.Utilities.MessageRecieved -= ReceiveCommand;
        }

        // Called on server
        private void ReceiveCommandPacket(byte[] data)
        {
            try
            {
                CommandInfo cmd = MyAPIGateway.Utilities.SerializeFromBinary<CommandInfo>(data);
                ReceiveCommand(cmd.id, cmd.command);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        // Called on server
        private void ReceiveCommand(ulong id, string command)
        {
            IMyPlayer p = RacingTools.GetPlayer(id);
            if (p != null)
                ProcessCommand(p, command);
        }

        // Called on clients
        private void MessageEntered(string messageText, ref bool sendToOthers)
        {
            IMyPlayer p = MyAPIGateway.Session.Player;
            if (p == null)
                return;
            
            if (IsRaceCommand(messageText))
            {
                sendToOthers = false;

                if (RacingConstants.IsServer)
                    ProcessCommand(p, messageText);
                else
                    RacingSession.Instance.Net.SendToServer(RacingConstants.packetCmd, new CommandInfo(messageText, p.SteamUserId));
            }
        }

        private bool IsRaceCommand(string command)
        {
            return command.StartsWith("/rcd", StringComparison.OrdinalIgnoreCase) || command.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private void ProcessCommand(IMyPlayer p, string command)
        {
            if (!IsRaceCommand(command))
                return;

            bool admin = RacingTools.IsPlayerAdmin(p);
            string[] cmd = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ChatCommand cmdOb;
            if (cmd.Length <= 1 || !commands.TryGetValue(cmd[1], out cmdOb))
                ShowChatHelp(p, admin);
            else
                cmdOb.Run(p, admin, cmd, race);
        }

        private void BuildText()
        {
            // Admin
            StringBuilder admin = new StringBuilder("Admin Commands:");
            HashSet<string> printed = new HashSet<string>();
            foreach (ChatCommand cmd in commands.Values)
            {
                AdminCommand acmd = cmd as AdminCommand;
                if (acmd != null && printed.Add(cmd.Id))
                {
                    admin.AppendLine();
                    AppendCommand(admin, acmd.Id, acmd.Usage);
                }
            }

            string adminHelp = admin.ToString();

            // General
            admin.Clear();
            StringBuilder gen = new StringBuilder();
            gen.Append("Commands:");
            admin.Append("Commands:");
            foreach (ChatCommand cmd in commands.Values)
            {
                if (printed.Add(cmd.Id))
                {
                    admin.AppendLine();
                    AppendCommand(admin, cmd.Id, cmd.AdminUsage);
                    gen.AppendLine();
                    AppendCommand(gen, cmd.Id, cmd.Usage);
                }
            }
            helpMsg = gen.ToString();

            ACmdHelp adminHelpCmd = new ACmdHelp(adminHelp);
            Add(adminHelpCmd, "admin");
            AppendCommand(admin, adminHelpCmd.Id, adminHelpCmd.Usage);
            adminHelpMsg = admin.ToString();
        }

        private void AppendCommand(StringBuilder sb, string id, string usage)
        {
            sb.Append(prefix).Append(' ').Append(id);
            if(usage.Length > 0)
            {
                if (usage[0] != ':')
                    sb.Append(' ');
                sb.Append(usage);
            }
        }

        void ShowChatHelp (IMyPlayer p, bool admin)
        {
            string msg;
            if (admin)
                msg = adminHelpMsg;
            else
                msg = helpMsg;
            MyVisualScriptLogicProvider.SendChatMessage(msg, "rcd", p.IdentityId, "Blue");
        }

        private void Add(params ChatCommand[] cmds)
        {
            foreach (ChatCommand cmd in cmds)
                commands[cmd.Id] = cmd;
        }

        private void Add(ChatCommand cmd, params string[] ids)
        {
            foreach (string id in ids)
                commands[id] = cmd;
        }

        [ProtoContract]
        public class CommandInfo
        {
            [ProtoMember(1)]
            public string command;
            [ProtoMember(2)]
            public ulong id;
            public CommandInfo()
            {
            }
            public CommandInfo(string cmd, ulong steamId)
            {
                this.command = cmd;
                this.id = steamId;
            }
        }
    }
}
