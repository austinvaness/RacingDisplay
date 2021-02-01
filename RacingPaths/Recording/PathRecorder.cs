﻿using avaness.RacingPaths.Data;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingPaths.Recording
{
    /// <summary>
    /// Keeps the best time of racer.
    /// </summary>
    public class PathRecorder
    {
        public Path Best
        {
            get
            {
                return best;
            }
            private set
            {
                best = value;
                OnBestChanged?.Invoke(p.SteamUserId, value);
            }
        }
        public event Action<ulong, Path> OnBestChanged;

        public bool IsLocal { get; }


        private readonly IMyPlayer p;
        private bool rec;
        private Path temp;
        private Path best;

        public PathRecorder(IMyPlayer p, Path best = null)
        {
            this.p = p;
            this.best = best;
            temp = new Path("BestTime" + p.SteamUserId, p.DisplayName, "The best time of " + p.DisplayName);

            IMyPlayer local = MyAPIGateway.Session?.Player;
            IsLocal = local != null && local.SteamUserId == p.SteamUserId;
        }

        public void Update()
        {
            if (rec)
            {
                RecordTick();
                if (Best != null && !Best.IsEmpty && !temp.BetterThan(Best))
                {
                    Cancel();
                }
            }
        }

        public void Unload()
        {
            OnBestChanged = null;
        }

        public void Start()
        {
            temp.ClearData();
            rec = true;
        }

        public void Stop()
        {
            if (rec)
            {
                if (Best == null || Best.IsEmpty || temp.BetterThan(Best))
                {
                    Path newEmpty = temp.EmptyCopy();
                    Best = temp;
                    temp = newEmpty;
                }
                else
                {
                    temp.ClearData();
                }
                rec = false;
            }
        }

        public void Clear()
        {
            Best.ClearData();
            Best = null;
            temp.ClearData();
            rec = false;
        }

        public void Cancel()
        {
            rec = false;
            temp.ClearData();
        }

        private void RecordTick()
        {
            temp.Record(GetCockpit()?.CubeGrid);
        }

        private IMyCubeBlock GetCockpit()
        {
            return p?.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
        }
    }
}
