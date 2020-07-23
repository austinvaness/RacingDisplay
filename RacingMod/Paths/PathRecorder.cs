using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Paths
{
    public class PathRecorder
    {
        private int runtime = 0;
        private GridInfo prevGrid;
        private readonly List<GridData> data = new List<GridData>();

        private bool play;
        private int playbackTick = 0;
        private int playbackGroupRequested = -1;
        private List<IMyCubeGrid> playbackGroup;

        private const int maxData = 108000; // about 30 minutes at 60tps

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
                    if (tick.newGrid != null)
                    {
                        ClosePlaybackGroup();
                        playbackGroupRequested = runtime;
                        tick.Create(NewPlaybackGroup, runtime);
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
            MyVisualScriptLogicProvider.AddGPSToEntity(RacingConstants.ghostId, RacingConstants.ghostName, RacingConstants.ghostDescription, RacingConstants.ghostWaypointColor);
        }

        private void RemoveGps()
        {
            MyVisualScriptLogicProvider.RemoveGPSFromEntity(RacingConstants.ghostId, RacingConstants.ghostName, RacingConstants.ghostDescription);
        }

        private class GridInfo
        {
            private long[] id;
            private int[] blockCount;
            private Vector3D[] pos;

            public GridInfo(IMyCubeGrid grid)
            {
                SetInfo(MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical));
            }

            private void SetInfo(List<IMyCubeGrid> grids)
            {
                id = new long[grids.Count];
                blockCount = new int[grids.Count];
                pos = new Vector3D[grids.Count];
                for (int i = 0; i < id.Length; i++)
                {
                    IMyCubeGrid g = grids[i];
                    id[i] = g.EntityId;
                    blockCount[i] = GridCount(g);
                    pos[i] = g.WorldMatrix.Translation;
                }
            }

            private int GridCount(IMyCubeGrid grid)
            {
                return ((MyCubeGrid)grid).GetFatBlocks().Count;
            }

            public bool Changed(IMyCubeGrid grid, out bool positionOnly)
            {
                List<IMyCubeGrid> grids = MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical);
                if (grids.Count != id.Length)
                {
                    positionOnly = false;
                    SetInfo(grids);
                    return false;
                }

                positionOnly = true;
                bool diff = false;
                for (int i = 0; i < id.Length; i++)
                {
                    IMyCubeGrid g = grids[i];
                    int count = GridCount(g);
                    if (g.EntityId != id[i])
                    {
                        id[i] = g.EntityId;
                        blockCount[i] = count;
                        pos[i] = g.WorldMatrix.Translation;
                        positionOnly = false;
                        diff = true;
                    }
                    else if (count != blockCount[i])
                    {
                        blockCount[i] = count;
                        pos[i] = g.WorldMatrix.Translation;
                        positionOnly = false;
                        diff = true;
                    }
                    else if (g.WorldMatrix.Translation != pos[i])
                    {
                        pos[i] = g.WorldMatrix.Translation;
                        diff = true;
                    }
                }

                return diff;
            }
        }

        private class GridData
        {
            public int Runtime { get; }

            public MyObjectBuilder_CubeGrid[] newGrid;
            public Matrix[] gridMatrix;

            public GridData(int runtime, IMyCubeGrid grid, bool saveBuilder)
            {
                Runtime = runtime;

                List<IMyCubeGrid> grids = MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical);
                if(saveBuilder)
                {
                    newGrid = grids.Select(GridToBuilder).ToArray();
                    gridMatrix = null;
                }
                else
                {
                    newGrid = null;
                    gridMatrix = grids.Select(GridToMatrix).ToArray();
                }

            }

            private Matrix GridToMatrix(IMyCubeGrid grid)
            {
                return grid.WorldMatrix;
            }

            private MyObjectBuilder_CubeGrid GridToBuilder(IMyCubeGrid grid)
            {
                var temp = (MyObjectBuilder_CubeGrid)grid.GetObjectBuilder(false);
                temp.CreatePhysics = false;
                temp.Immune = true;
                temp.DestructibleBlocks = false;
                temp.IsPowered = false;
                temp.Editable = false;
                return temp;
            }

            public void Create(Action<List<IMyCubeGrid>, int> onReady, int runtime)
            {
                if (newGrid == null || newGrid.Length == 0)
                {
                    onReady.Invoke(new List<IMyCubeGrid>(), runtime);
                }
                else
                {
                    var g = newGrid[0];
                    MyAPIGateway.Entities.RemapObjectBuilder(g);
                    g.Name = g.DisplayName = RacingConstants.ghostId;
                    MyAPIGateway.Entities.CreateFromObjectBuilderParallel(g, false, e => OnCreateFinish(e, runtime, onReady));
                }
            }

            private void OnCreateFinish(IMyEntity e, int runtime, Action<List<IMyCubeGrid>, int> onReady)
            {
                List<IMyCubeGrid> temp = new List<IMyCubeGrid>(newGrid.Length);
                IMyCubeGrid g1 = (IMyCubeGrid)e;
                g1.Name = g1.DisplayName = g1.CustomName = RacingConstants.ghostId;
                Prep(g1);
                MyAPIGateway.Entities.AddEntity(e);
                temp.Add(g1);
                foreach(var builder in newGrid.Skip(1))
                {
                    MyAPIGateway.Entities.RemapObjectBuilder(builder);
                    temp.Add(Build(builder));
                }
                onReady.Invoke(temp, runtime);
            }

            private void Prep(IMyCubeGrid g)
            {
                g.Flags &= ~EntityFlags.Save;
                g.Flags &= ~EntityFlags.Sync;
                g.Save = false;
                g.Synchronized = false;
            }

            private IMyCubeGrid Build(MyObjectBuilder_CubeGrid builder)
            {
                IMyCubeGrid g = (IMyCubeGrid)MyAPIGateway.Entities.CreateFromObjectBuilder(builder);
                Prep(g);
                MyAPIGateway.Entities.AddEntity(g);
                return g;
            }

            public void Teleport(IEnumerable<IMyCubeGrid> grids)
            {
                int index = 0;
                foreach(IMyCubeGrid grid in grids)
                {
                    if (index >= gridMatrix.Length)
                        return;
                    grid.WorldMatrix = gridMatrix[index++];
                }
            }
        }
    }
}
