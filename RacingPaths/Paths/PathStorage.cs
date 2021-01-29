using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using VRage;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using avaness.RacingPaths.Net;
using System.Linq;

namespace avaness.RacingPaths.Paths
{
    public class PathStorage
    {
        private Dictionary<ulong, SerializablePathInfo> paths = new Dictionary<ulong, SerializablePathInfo>();
        private Dictionary<ulong, PathRecorder> recorders = new Dictionary<ulong, PathRecorder>();

        public PathPlayer Player { get; } = new PathPlayer();

        public PathRecorder LocalRecorder { get; private set; }

        public PathStorage(Network net)
        {
            if(RacingConstants.IsPlayer)
                net.Register(RacingConstants.packetRecord, PacketReceived);
        }

        public void Update()
        {
            Player.Update();
            foreach (PathRecorder rec in recorders.Values)
                rec.Update();
            MyAPIGateway.Utilities.ShowNotification($"Recorders: {recorders.Count} Paths: {paths.Count}", 16);
        }

        private void PacketReceived(byte[] data)
        {
            try
            {
                PathPacket p = MyAPIGateway.Utilities.SerializeFromBinary<PathPacket>(data);
                if (p != null)
                    PacketReceived(p);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, typeof(PathStorage));
            }
        }

        public void PacketReceived(PathPacket p)
        {
            MyAPIGateway.Utilities.ShowNotification("Recieved packet " + p.method);

            PathRecorder local = LocalRecorder;
            if (local == null)
                return;


            switch (p.method)
            {
                case PathPacket.Method.Start:
                    local.StartTrack();
                    List<Path> paths = new List<Path>();
                    if (local.Best != null)
                        paths.Add(local.Best);
                    foreach (ulong gid in p.ghosts)
                    {
                        SerializablePathInfo info;
                        if (this.paths.TryGetValue(gid, out info))
                            paths.Add(info.Data);
                    }
                    Player.Play(paths);
                    break;
                case PathPacket.Method.End:
                    local.EndTrack();
                    Player.Clear();
                    break;
                case PathPacket.Method.Left:
                    local.LeftTrack();
                    Player.Clear();
                    break;
                case PathPacket.Method.Clear:
                    local.ClearData();
                    Player.Clear();
                    break;
            }
        }

        private SerializablePathInfo GetPath(string base64)
        {
            try
            {
                byte[] compressed = Convert.FromBase64String(base64);
                byte[] uncompressed = MyCompression.Decompress(compressed);
                return MyAPIGateway.Utilities.SerializeFromBinary<SerializablePathInfo>(uncompressed);
            }
            catch
            {
                return null;
            }
        }

        private string GetBase64(SerializablePathInfo info)
        {
            try
            {
                byte[] uncompressed = MyAPIGateway.Utilities.SerializeToBinary(info);
                byte[] compressed = MyCompression.Compress(uncompressed);
                return Convert.ToBase64String(compressed);
            }
            catch
            {
                return null;
            }
        }

        public void Load()
        {
            string[] storage;
            if (MyAPIGateway.Utilities.GetVariable(RacingConstants.pathVarId, out storage))
            {
                paths.Clear();
                foreach (string s in storage)
                {
                    SerializablePathInfo temp = GetPath(s);
                    if (temp != null)
                        paths[temp.PlayerId] = temp;
                }

                IMyPlayer me = MyAPIGateway.Session?.Player;
                if (me != null)
                    LocalRecorder = GetRecorder(me);
            }
            else
            {
                MyAPIGateway.Utilities.RemoveVariable(RacingConstants.pathVarId);
            }
        }

        /// <summary>
        /// Called on the server to save paths.
        /// </summary>
        public void Save()
        {
            foreach (KeyValuePair<ulong, PathRecorder> kv in recorders)
            {
                Path p = kv.Value.Best;
                if(p != null && !p.IsEmpty)
                    paths[kv.Key] = new SerializablePathInfo(kv.Key, p);
            }

            List<string> base64s = new List<string>();
            foreach(SerializablePathInfo info in paths.Values)
            {
                string base64 = GetBase64(info);
                if (base64 != null)
                    base64s.Add(base64);
            }
            MyAPIGateway.Utilities.SetVariable(RacingConstants.pathVarId, base64s.ToArray());
        }

        public PathRecorder GetRecorder(IMyPlayer p)
        {
            ulong pid = p.SteamUserId;
            PathRecorder temp;
            if (recorders.TryGetValue(pid, out temp))
                return temp;

            SerializablePathInfo info;
            if (paths.TryGetValue(pid, out info))
                temp = new PathRecorder(p, info.Data);
            else
                temp = new PathRecorder(p);
            recorders[pid] = temp;
            return temp;
        }
    }
}
