using System.Collections.Generic;
using System;
using VRage;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using avaness.RacingPaths.Recording;
using avaness.RacingPaths.Data;
using VRage.Utils;
using System.Collections;

namespace avaness.RacingPaths.Storage
{
    public class PathStorage : IEnumerable<Path>
    {
        private const string variableId = "RacePathData";

        private Dictionary<ulong, Path> saveData = new Dictionary<ulong, Path>();
        private bool needSave = false;
        private Dictionary<ulong, PathRecorder> recorders = new Dictionary<ulong, PathRecorder>();

        public PathStorage()
        {
            Load();
        }

        public void Update()
        {
            foreach (PathRecorder rec in recorders.Values)
                rec.Update();
        }

        public void Unload()
        {
            foreach(PathRecorder rec in recorders.Values)
            {
                rec.OnBestChanged -= OnBestChanged;
                rec.Unload();
            }
        }

        private Path ConvertToPath(string base64)
        {
            try
            {
                byte[] compressed = Convert.FromBase64String(base64);
                byte[] uncompressed = MyCompression.Decompress(compressed);
                return MyAPIGateway.Utilities.SerializeFromBinary<Path>(uncompressed);
            }
            catch (Exception e)
            {
                string s = "Error converting base64 to Path: " + e.ToString();
                MyLog.Default.WriteLineAndConsole(s);
                return null;
            }
        }

        private string ConvertToBase64(Path info)
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
                    Path temp = ConvertToPath(s);
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
            if (!needSave)
                return;

            List<string> base64s = new List<string>();
            foreach(Path p in saveData.Values)
            {
                string base64 = ConvertToBase64(p);
                if (base64 != null)
                    base64s.Add(base64);
            }
            MyAPIGateway.Utilities.SetVariable(variableId, base64s.ToArray());
            needSave = false;
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
            if (newBest == null || newBest.IsEmpty)
                saveData.Remove(id);
            else
                saveData[id] = newBest;
            needSave = true;
        }

        public bool TryGetPath(ulong id, out Path path)
        {
            return saveData.TryGetValue(id, out path);
        }

        public void Remove(ulong id)
        {
            PathRecorder temp;
            if (TryGetRecorder(id, out temp))
                temp.Clear();
            else
                saveData.Remove(id);
            needSave = true;
        }

        public IEnumerator<Path> GetEnumerator()
        {
            return saveData.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return saveData.Values.GetEnumerator();
        }
    }
}
