using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Ghost.Data
{
    public partial class Path
    {
        [ProtoContract]
        private class GridData
        {
            [ProtoMember(1)]
            public int Runtime { get; private set; }
            public bool NewGrid => newGrid != null && newGrid.Length > 0;

            [ProtoMember(2)]
            private MyObjectBuilder_CubeGrid[] newGrid = new MyObjectBuilder_CubeGrid[0];

            [ProtoMember(3)]
            private SerializableMatrix[] serializableMatrix
            {
                get
                {
                    return Array.ConvertAll(gridMatrix, item => (SerializableMatrix)item);
                }
                set
                {
                    gridMatrix = Array.ConvertAll(value, item => (Matrix)item);
                }
            }

            private Matrix[] gridMatrix = new Matrix[0];

            /// <summary>
            /// Used for serialization only.
            /// </summary>
            private GridData()
            {

            }

            public GridData(int runtime, IMyCubeGrid grid, bool saveBuilder)
            {
                Runtime = runtime;

                List<IMyCubeGrid> grids = new List<IMyCubeGrid>();
                MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical, grids);
                if (saveBuilder)
                    newGrid = grids.Select(GridToBuilder).ToArray();
                else
                    gridMatrix = grids.Select(GridToMatrix).ToArray();

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

            public void Create(Action<List<IMyCubeGrid>, int> onReady, string ghostId, int runtime)
            {
                if (NewGrid)
                {
                    var g = newGrid[0];
                    MyAPIGateway.Entities.RemapObjectBuilder(g);
                    g.Name = g.DisplayName = ghostId;
                    MyAPIGateway.Entities.CreateFromObjectBuilderParallel(g, false, e => OnCreateFinish(e, ghostId, runtime, onReady));
                }
                else
                {
                    onReady.Invoke(new List<IMyCubeGrid>(), runtime);
                }
            }

            private void OnCreateFinish(IMyEntity e, string ghostId, int runtime, Action<List<IMyCubeGrid>, int> onReady)
            {
                List<IMyCubeGrid> temp = new List<IMyCubeGrid>(newGrid.Length);
                IMyCubeGrid g1 = (IMyCubeGrid)e;
                g1.Name = g1.DisplayName = g1.CustomName = ghostId;
                MyAPIGateway.Entities.SetEntityName(g1);
                Prep(g1);
                MyAPIGateway.Entities.AddEntity(e);
                temp.Add(g1);
                foreach (var builder in newGrid.Skip(1))
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
                foreach (IMyCubeGrid grid in grids)
                {
                    if (index >= gridMatrix.Length)
                        return;
                    grid.WorldMatrix = gridMatrix[index++];
                }
            }
        }
    }
}
