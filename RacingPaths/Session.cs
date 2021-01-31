using avaness.RacingMod.API;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using avaness.RacingPaths.Storage;
using VRageMath;
using avaness.RacingPaths.Data;
using avaness.RacingPaths.Recording;
using System.Linq;

namespace avaness.RacingPaths
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingPathsSession : MySessionComponentBase
    {
        public static RacingPathsSession Instance;

        public PathStorage Paths => paths;

        private bool init = false;
        private RacingDisplayAPI racingApi;
        private PathStorage paths;
        private PathPlayer player = new PathPlayer();
        private HashSet<ulong> selectedPlayers = new HashSet<ulong>();
        private Commands cmds;

        public override void LoadData()
        {
            Instance = this;
        }

        protected override void UnloadData()
        {
            Instance = null;
            if(racingApi != null)
            {
                racingApi.OnPlayerStarted -= RacingApi_OnPlayerStarted;
                racingApi.OnPlayerFinished -= RacingApi_OnPlayerFinished;
                racingApi.OnPlayerLeft -= RacingApi_OnPlayerLeft;
            }
            cmds?.Unload();
            paths?.Unload();
        }

        private void Start()
        {
            racingApi = new RacingDisplayAPI(OnRacingAPI);
            paths = new PathStorage();
            cmds = new Commands(paths);
            init = true;
        }

        private void OnRacingAPI()
        {
            racingApi.OnPlayerStarted += RacingApi_OnPlayerStarted;
            racingApi.OnPlayerFinished += RacingApi_OnPlayerFinished;
            racingApi.OnPlayerLeft += RacingApi_OnPlayerLeft;
        }

        private void RacingApi_OnPlayerLeft(IMyPlayer p)
        {
            PathRecorder rec;
            if (paths.TryGetRecorder(p.SteamUserId, out rec))
            {
                rec.Cancel();
                if (rec.IsLocal)
                    player.Clear();
            }
        }

        private IEnumerable<Path> GetSelectedPaths()
        {
            foreach(ulong id in selectedPlayers)
            {
                Path p;
                if (paths.TryGetPath(id, out p))
                    yield return p;
            }
        }

        private void RacingApi_OnPlayerFinished(IMyPlayer p)
        {
            PathRecorder rec;
            if (paths.TryGetRecorder(p.SteamUserId, out rec))
            {
                rec.Stop();
                if (rec.IsLocal)
                    player.Clear();
            }
        }

        private void RacingApi_OnPlayerStarted(IMyPlayer p)
        {
            PathRecorder rec = paths.GetRecorder(p);
            rec.Start();
            if (rec.IsLocal)
            {
                if (rec.Best != null && !rec.Best.IsEmpty)
                    player.Play(GetSelectedPaths(), rec.Best);
                else
                    player.Play(GetSelectedPaths());
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!init)
                Start();

            player.Update();
            paths.Update();
        }

        public override void SaveData()
        {
            if(paths != null && MyAPIGateway.Session.IsServer)
                paths.Save();
        }
    }
}