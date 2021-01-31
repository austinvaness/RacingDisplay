using System.Collections.Generic;
using System;
using VRage;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using System.Linq;
using avaness.RacingPaths.Recording;
using avaness.RacingPaths.Data;
using VRage.Utils;
using System.Collections;

namespace avaness.RacingPaths.Storage
{
    public class PathStorage : IEnumerable<SerializablePathInfo>
    {
        private const string variableId = "RacePathData";

        private Dictionary<ulong, SerializablePathInfo> saveData = new Dictionary<ulong, SerializablePathInfo>();
        private Dictionary<ulong, PathRecorder> recorders = new Dictionary<ulong, PathRecorder>();

        public PathStorage()
        {
            Load();
        }

        public void Update()
        {
            foreach (PathRecorder rec in recorders.Values)
                rec.Update();
            MyAPIGateway.Utilities.ShowNotification($"Recorders: {recorders.Count} Paths: {saveData.Count}", 16);
        }

        public void Unload()
        {
            foreach(PathRecorder rec in recorders.Values)
            {
                rec.OnBestChanged -= OnBestChanged;
                rec.Unload();
            }
        }

        private SerializablePathInfo ConvertToPath(string base64)
        {
            try
            {
                byte[] compressed = Convert.FromBase64String(base64);
                byte[] uncompressed = MyCompression.Decompress(compressed);
                return MyAPIGateway.Utilities.SerializeFromBinary<SerializablePathInfo>(uncompressed);
            }
            catch (Exception e)
            {
                string s = "Error converting base64 to Path: " + e.ToString();
                MyLog.Default.WriteLineAndConsole(s);
                MyAPIGateway.Utilities.ShowNotification(s, 5000);
                return null;
            }
        }

        private string ConvertToBase64(SerializablePathInfo info)
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
            if (MyAPIGateway.Utilities.GetVariable(variableId, out storage))
            {
                saveData.Clear();
                foreach (string s in storage)
                {
                    SerializablePathInfo temp = ConvertToPath(s);
                    if (temp != null)
                        saveData[temp.PlayerId] = temp;
                }

            }
            else
            {
                MyAPIGateway.Utilities.RemoveVariable(variableId);
            }
        }

        public void ClearAll()
        {
            saveData.Clear();
            foreach (PathRecorder rec in recorders.Values)
                rec.Clear();
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
                    saveData[kv.Key] = new SerializablePathInfo(kv.Key, p);
            }

            List<string> base64s = new List<string>();
            foreach(SerializablePathInfo info in saveData.Values)
            {
                string base64 = ConvertToBase64(info);
                if (base64 != null)
                    base64s.Add(base64);
            }
            MyAPIGateway.Utilities.SetVariable(variableId, base64s.ToArray());
        }

        public bool TryGetRecorder(ulong id, out PathRecorder rec)
        {
            return recorders.TryGetValue(id, out rec);
        }

        public PathRecorder GetRecorder(IMyPlayer p)
        {
            ulong pid = p.SteamUserId;
            PathRecorder temp;
            if (recorders.TryGetValue(pid, out temp))
                return temp;

            Path saved;
            if (TryGetPath(pid, out saved))
                temp = new PathRecorder(p, saved);
            else
                temp = new PathRecorder(p);
            temp.OnBestChanged += OnBestChanged;
            recorders[pid] = temp;
            return temp;
        }

        private void OnBestChanged(ulong id, Path newBest)
        {
            if(newBest == null || newBest.IsEmpty)
                saveData.Remove(id);
            else
                saveData[id] = new SerializablePathInfo(id, newBest);
        }

        public bool TryGetPathInfo(ulong id, out SerializablePathInfo info)
        {
            return saveData.TryGetValue(id, out info);
        }

        public bool TryGetPath(ulong id, out Path path)
        {
            SerializablePathInfo info;
            if(saveData.TryGetValue(id, out info))
            {
                path = info.Data;
                return true;
            }

            path = null;
            return false;
        }

        public void Remove(ulong id)
        {
            PathRecorder temp;
            if (TryGetRecorder(id, out temp))
                temp.Clear();
            else
                saveData.Remove(id);
        }

        public IEnumerator<SerializablePathInfo> GetEnumerator()
        {
            return saveData.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return saveData.Values.GetEnumerator();
        }
    }
}
