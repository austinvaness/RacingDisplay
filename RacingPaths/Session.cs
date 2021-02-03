using Sandbox.ModAPI;
using VRage.Game.Components;
using avaness.RacingPaths.Storage;
using avaness.RacingPaths.Recording;
using avaness.RacingPaths.Hud;
using VRage.Game;
using avaness.RacingPaths.Net;
using RichHudFramework.Client;
using System;

namespace avaness.RacingPaths
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingPathsSession : MySessionComponentBase
    {
        public static RacingPathsSession Instance;

        public static bool IsServer => MyAPIGateway.Session.IsServer || MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE;
        public static bool IsDedicated => IsServer && MyAPIGateway.Utilities.IsDedicated;
        public static bool IsPlayer => !IsDedicated;

        private bool init = false;
        private PathStorage paths;
        private PathPlayer player = new PathPlayer();
        private Commands cmds;
        private GhostHud hud;
        private RecordingManager recs;
        private PlaybackManager play;
        private Network net;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            RichHudClient.Init(DebugName, HudReady, () => { });
        }

        private void HudReady()
        {
            if(IsPlayer)
            {
                if (hud == null)
                    hud = new GhostHud();
                else
                    hud.CreateRichHud();
            }
        }

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
                if(hud == null)
                    hud = new GhostHud();
                play = new PlaybackManager(net, paths, player, hud);
            }

            if (IsServer)
            {
                recs = new RecordingManager(paths, net, play);
            }

            cmds = new Commands(paths, recs, play);

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
    }
}