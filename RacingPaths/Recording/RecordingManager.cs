using avaness.RacingMod.API;
using avaness.RacingPaths.Net;
using avaness.RacingPaths.Storage;
using Sandbox.Game;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace avaness.RacingPaths.Recording
{
    /// <summary>
    /// Manages recordings server side.
    /// </summary>
    public class RecordingManager
    {
        private readonly RacingDisplayAPI racingApi;
        private readonly PathStorage paths;
        private readonly HashSet<ulong> toRecord = new HashSet<ulong>();
        private readonly Network net;

        public RecordingManager(PathStorage paths, Network net)
        {
            this.paths = paths;
            this.net = net;
            racingApi = new RacingDisplayAPI(OnRacingAPI);
            MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnected;
        }

        private void PlayerDisconnected(long playerId)
        {
            ulong id = MyAPIGateway.Players.TryGetSteamId(playerId);
            if(id > 0)
                SetRecording(id, false);
        }

        public void Unload()
        {
            if(racingApi != null)
            {
                racingApi.Close();
                racingApi.OnPlayerStarted -= OnPlayerStarted;
                racingApi.OnPlayerFinished -= OnPlayerFinished;
                racingApi.OnPlayerLeft -= OnPlayerLeft;
            }
        }

        private void OnRacingAPI()
        {
            racingApi.OnPlayerStarted += OnPlayerStarted;
            racingApi.OnPlayerFinished += OnPlayerFinished;
            racingApi.OnPlayerLeft += OnPlayerLeft;
        }

        private void OnPlayerLeft(IMyPlayer p)
        {
            PathRecorder rec;
            if (paths.TryGetRecorder(p.SteamUserId, out rec))
                rec.Cancel();

            net.SendTo(Network.packetRaceStart, new PacketRaceEnd(false), p.SteamUserId);
        }

        private void OnPlayerFinished(IMyPlayer p)
        {
            PathRecorder rec;
            if (paths.TryGetRecorder(p.SteamUserId, out rec))
                rec.Stop();

            net.SendTo(Network.packetRaceStart, new PacketRaceEnd(true), p.SteamUserId);
        }

        private void OnPlayerStarted(IMyPlayer p)
        {
            bool recording = toRecord.Contains(p.SteamUserId);
            
            if(recording)
                paths.GetRecorder(p).Start();

            net.SendTo(Network.packetRaceStart, new PacketRaceStart(recording), p.SteamUserId);
        }

        public bool ToggleRecording(ulong id)
        {
            if (toRecord.Add(id))
            {
                return true;
            }
            else
            {
                toRecord.Remove(id);
                
                PathRecorder rec;
                if (paths.TryGetRecorder(id, out rec))
                    rec.Cancel();
                return false;
            }
        }

        public void SetRecording(ulong id, bool state)
        {
            if (state)
            {
                toRecord.Add(id);
            }
            else
            {
                PathRecorder rec;
                if(toRecord.Remove(id) && paths.TryGetRecorder(id, out rec))
                    rec.Cancel();
            }
        }
    }
}
