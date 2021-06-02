using Sandbox.ModAPI;
using VRage.Game.Components;
using avaness.RacingLeaderboard.Storage;
using avaness.RacingLeaderboard.Recording;
using avaness.RacingLeaderboard.Hud;
using VRage.Game;
using avaness.RacingLeaderboard.Net;
using System;
using avaness.RacingLeaderboard.Data;
using VRageMath;
using VRage;
using avaness.RacingLeaderboard.Chat;

namespace avaness.RacingLeaderboard
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingPathsSession : MySessionComponentBase
    {
        public static RacingPathsSession Instance;

        public static bool IsServer => MyAPIGateway.Session.IsServer;
        public static bool IsDedicated => IsServer && MyAPIGateway.Utilities.IsDedicated;
        public static bool IsPlayer => !IsDedicated;

        private bool init = false;
        public Random Rand = new Random();
        private PathStorage paths;
        private PathPlayer player = new PathPlayer();
        private Commands cmds;
        private CoreHud hud;
        private RecordingManager recs;
        private PlaybackManager play;
        private Network net;

        public override void LoadData()
        {
            Instance = this;
        }

        protected override void UnloadData()
        {
            Instance = null;
            net?.Unload();
            recs?.Unload();
            cmds?.Unload();
            paths?.Unload();
            hud?.Unload();
        }

        private void Start()
        {
            if (IsPlayer && MyAPIGateway.Session.Player == null)
                return;

            net = new Network();
            paths = new PathStorage();

            if (IsPlayer)
            {
                hud = new CoreHud(paths);
                play = new PlaybackManager(net, paths, player, hud);
                hud.Init(play);
            }

            if (IsServer)
            {
                recs = new RecordingManager(paths, net, play);
            }

            //cmds = new Commands(paths, recs, play);

            init = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!init)
            {
                Start();
                if (!init)
                    return;
            }
            player?.Update();
            paths.Update();
        }

        public override void SaveData()
        {
            if(paths != null && MyAPIGateway.Session.IsServer)
                paths.Save();
        }

        public override void Draw()
        {
            hud?.Draw();
        }
    }
}