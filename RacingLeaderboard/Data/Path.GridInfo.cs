using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.RacingLeaderboard.Data
{
    public partial class Path
    {
        /// <summary>
        /// Used to track changes in the grid.
        /// </summary>
        private class GridInfo
        {
            private long[] id;
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
                pos = new Vector3D[grids.Count];
                for (int i = 0; i < id.Length; i++)
                {
                    IMyCubeGrid g = grids[i];
                    id[i] = g.EntityId;
                    pos[i] = g.WorldMatrix.Translation;
                }
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
                    if (g.EntityId != id[i])
                    {
                        id[i] = g.EntityId;
                        pos[i] = g.WorldMatrix.Translation;
                        positionOnly = false;
                        diff = true;
                    }
                    else if (!g.WorldMatrix.Translation.Equals(pos[i], 0.1))
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
