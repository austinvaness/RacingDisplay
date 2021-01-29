using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Paths
{
    public partial class Path
    {
        /// <summary>
        /// Used to track changes in the grid.
        /// </summary>
        private class GridInfo
        {
            private long[] id;
            private int[] blockCount;
            private Vector3D[] pos;

            public GridInfo(IMyCubeGrid grid)
            {
                List<IMyCubeGrid> grids = new List<IMyCubeGrid>();
                MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical, grids);
                SetInfo(grids);
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
                List<IMyCubeGrid> grids = new List<IMyCubeGrid>();
                MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical, grids);
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
    }
}
