﻿using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Paths
{
    [ProtoContract]
    public partial class Path
    {
        private int runtime = 0;
        private GridInfo prevGrid;
        [ProtoMember(1)]
        private readonly List<GridData> data = new List<GridData>();

        private bool play;
        private int playbackTick = 0;
        private int playbackGroupRequested = -1;
        private List<IMyCubeGrid> playbackGroup;

        private readonly string ghostId, ghostName, ghostDescription;

        private static Color ghostWaypointColor = new Color(0, 0, 255);
        private const int maxData = 108000; // about 30 minutes at 60tps

        public bool IsEmpty => data.Count == 0;

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        private Path()
        {
        }

        public Path(string ghostId, string ghostName, string ghostDescription)
        {
            this.ghostId = ghostId;
            this.ghostName = ghostName;
            this.ghostDescription = ghostDescription;
        }

        public Path EmptyCopy()
        {
            return new Path(ghostId, ghostName, ghostDescription);
        }

        public void Debug(StringBuilder sb, bool rec, bool play)
        {
            if (data.Count == 0)
                sb.Append("Recorder is empty.");
            else if (runtime == 0 || (!rec && !play))
                sb.Append("Recorder is idle. (").Append(data.Count).Append(" frames)");
            else if (play)
                sb.Append("Playback: ").Append(playbackTick).Append('/').Append(data.Count);
            else if (rec)
            {
                float density = ((float)data.Count / runtime) * 100;
                sb.Append("Recording: ").Append(runtime).Append(' ')
                    .Append(Math.Round(density, 2, MidpointRounding.AwayFromZero)).Append('%');
            }
        }

        public void Record(IMyCubeGrid grid)
        {
            if (play)
            {
                runtime = 0;
                data.Clear();
                ClosePlaybackGroup();
                prevGrid = null;
            }
            else if(data.Count >= maxData)
            {
                return;
            }
            play = false;

            runtime++;

            if (grid == null)
            {
                prevGrid = null;
            }
            else if(prevGrid == null)
            {
                prevGrid = new GridInfo(grid);
                data.Add(new GridData(runtime, grid, true));
            }
            else 
            {
                bool posOnly;
                if (prevGrid.Changed(grid, out posOnly))
                {
                    data.Add(new GridData(runtime, grid, !posOnly));
                }
            }
        }

        public bool Play()
        {
            if (!play)
            {
                runtime = 0;
                playbackTick = 0;
                playbackGroupRequested = -1;
            }
            play = true;

            if(playbackTick < data.Count)
            {
                GridData tick = data[playbackTick];
                if (tick.Runtime <= runtime)
                {
                    if (tick.NewGrid)
                    {
                        ClosePlaybackGroup();
                        playbackGroupRequested = runtime;
                        tick.Create(NewPlaybackGroup, ghostId, runtime);
                    }
                    else if(playbackGroup != null)
                    {
                        tick.Teleport(playbackGroup);
                    }
                    playbackTick++;
                }
                runtime++;
                return true;
            }
            else
            {
                ClosePlaybackGroup();
                runtime++;
                return false;
            }
        }

        public void ClearData()
        {
            data.Clear();
            play = false;
            runtime = 0;
            prevGrid = null;
            ClosePlaybackGroup();
        }

        public void StopPlay()
        {
            play = false;
            ClosePlaybackGroup();
        }

        private void NewPlaybackGroup(List<IMyCubeGrid> group, int requestTime)
        {
            if(play && requestTime == playbackGroupRequested)
            {
                playbackGroup = group;
                AddGps();
            }
            else
            {
                foreach (IMyCubeGrid g in group)
                    MyAPIGateway.Entities.MarkForClose(g);
            }
        }

        private void ClosePlaybackGroup()
        {
            if (playbackGroup != null)
            {
                RemoveGps();
                foreach (IMyCubeGrid g in playbackGroup)
                    MyAPIGateway.Entities.MarkForClose(g);
                playbackGroup = null;
            }
        }

        private void AddGps()
        {
            MyVisualScriptLogicProvider.AddGPSToEntity(ghostId, ghostName, ghostDescription, ghostWaypointColor);
        }

        private void RemoveGps()
        {
            MyVisualScriptLogicProvider.RemoveGPSFromEntity(ghostId, ghostName, ghostDescription);
        }

        public bool SmallerThan(Path other)
        {
            return other.data.Count == 0 || data.Count < other.data.Count;
        }
    }
}
