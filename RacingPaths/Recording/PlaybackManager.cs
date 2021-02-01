using avaness.RacingPaths.Data;
using avaness.RacingPaths.Hud;
using avaness.RacingPaths.Net;
using avaness.RacingPaths.Storage;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace avaness.RacingPaths.Recording
{
    /// <summary>
    /// Manages playback client side.
    /// </summary>
    public class PlaybackManager
    {
        private readonly PathStorage paths;
        private readonly PathPlayer player;
        private readonly GhostHud hud;
        private readonly HashSet<ulong> toPlay = new HashSet<ulong>();
        private readonly IMyPlayer me;

        public PlaybackManager(Network net, PathStorage paths, PathPlayer player, GhostHud hud)
        {
            me = MyAPIGateway.Session.Player;
            this.paths = paths;
            this.player = player;
            this.hud = hud;
            net.Register(Network.packetRaceStart, RaceStart);
            net.Register(Network.packetRaceEnd, RaceEnd);
        }

        private void RaceEnd(byte[] data)
        {
            try
            {
                PacketRaceEnd packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketRaceEnd>(data);
                if (packet != null)
                    RaceEnd(packet.finish);
            }
            catch { }
        }

        public void RaceEnd(bool finish)
        {
            PathRecorder rec;
            if (paths.TryGetRecorder(me.SteamUserId, out rec))
            {
                if (finish)
                    rec.Stop();
                else
                    rec.Cancel();
            }

            player.Clear();
            hud.RecordingIndicator = false;
        }

        private void RaceStart(byte[] data)
        {
            try
            {
                PacketRaceStart packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketRaceStart>(data);
                if (packet != null)
                    RaceStart(packet.recording);
            }
            catch { }
        }

        public void RaceStart(bool recording)
        {
            List<Path> playable = new List<Path>();
            foreach (ulong id in toPlay)
            {
                Path p;
                if (paths.TryGetPath(id, out p))
                    playable.Add(p);
            }

            if (recording)
            {
                paths.GetRecorder(me).Start();

                Path p;
                if (!toPlay.Contains(me.SteamUserId) && paths.TryGetPath(me.SteamUserId, out p))
                    playable.Add(p); // Always play the local recording
            }

            player.Play(playable);
            hud.RecordingIndicator = recording;
        }

        public bool TogglePlay(ulong id)
        {
            if (toPlay.Add(id))
            {
                return true;
            }
            else
            {
                toPlay.Remove(id);
                return false;
            }
        }

        public void SetPlay(ulong id, bool state)
        {
            if (state)
            {
                toPlay.Add(id);
            }
            else
            {
                toPlay.Remove(id);
            }
        }
    }
}
