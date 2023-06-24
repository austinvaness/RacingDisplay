using avaness.RacingMod.Beacon;
using avaness.RacingMod.Race;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdNode : AdminCommand
    {
        private const string NodePrefab = "RacingNodePrefab";

        public override string Id => "node";

        public override string Usage => "create|open: Create a node or open the closest node";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            switch (cmd[2].ToLowerInvariant())
            {
                case "create":
                    CreateNode(p);
                    break;
                case "open":
                    OpenNode(p); 
                    break;
                default:
                    ShowChatMsg(p, $"'{cmd[2]}' is not a valid option. Expected 'create' or 'open'.");
                    break;
            }
        }

        private void OpenNode(IMyPlayer p)
        {
            if (p.Character?.PositionComp == null)
                return;
            if (MyAPIGateway.Session.Player == null || MyAPIGateway.Session.Player.IdentityId != p.IdentityId)
            {
                ShowChatMsg(p, "Opening nodes via command only works for the server host.");
                return;
            }

            RacingBeacon node;
            if (TryGetNode(p, out node))
                MyAPIGateway.Gui.ShowTerminalPage(MyTerminalPageEnum.ControlPanel, p.Character, node.Entity, false);
            else
                ShowChatMsg(p, "No nodes found, move closer to the node.");
        }

        private bool TryGetNode(IMyPlayer p, out RacingBeacon node)
        {
            // Character position must be used instead of the camera because keen only checks character position
            Vector3D position = p.Character.PositionComp.GetPosition();

            float interactiveDistance2 = MyConstants.DEFAULT_INTERACTIVE_DISTANCE * MyConstants.DEFAULT_INTERACTIVE_DISTANCE;
            BoundingSphereD sphere = new BoundingSphereD(position, interactiveDistance2);
            RacingBeacon closestBeacon = null;
            double closestDistance = double.PositiveInfinity;
            foreach (IMyEntity e in MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere))
            {
                IMyCubeGrid grid = e as IMyCubeGrid;
                if (grid?.Physics != null && grid.IsStatic)
                {
                    foreach (IMyBeacon beacon in grid.GetFatBlocks<IMyBeacon>())
                    {
                        if (beacon.PositionComp == null)
                            continue;

                        RacingBeacon racingBeacon = beacon.GameLogic.GetAs<RacingBeacon>();
                        if (racingBeacon == null)
                            continue;

                        double distance = beacon.PositionComp.WorldAABB.DistanceSquared(position);
                        if (distance >= interactiveDistance2)
                            continue;

                        if (distance < closestDistance)
                        {
                            closestBeacon = racingBeacon;
                            closestDistance = distance;
                        }
                    }
                }
            }

            if (closestBeacon != null)
            {
                node = closestBeacon;
                return true;
            }

            node = null;
            return false;
        }

        private void CreateNode(IMyPlayer p)
        {
            if (p.Character?.PositionComp == null)
                return;

            MatrixD worldMatrix = p.Character.WorldMatrix;
            Vector3D newPosition = worldMatrix.Translation - (worldMatrix.Up * MyConstants.DEFAULT_INTERACTIVE_DISTANCE * 0.8);
            List<IMyCubeGrid> result = new List<IMyCubeGrid>();
            MyAPIGateway.PrefabManager.SpawnPrefab(result, NodePrefab, newPosition, worldMatrix.Forward, worldMatrix.Up, ownerId: p.IdentityId, callback: () =>
            {
                IMySession session = MyAPIGateway.Session;

                if (session.Player != null && session.Player.IdentityId == p.IdentityId)
                {
                    // This command is running on the server, so only add a gps if the command came from the host
                    IMyGps gps = session.GPS.Create("New Node", "New Racing Display node", newPosition, true, true);
                    gps.DiscardAt = session.ElapsedPlayTime + TimeSpan.FromSeconds(10);
                    session.GPS.AddLocalGps(gps);
                }

                string trackName = RacingSession.Instance.CurrentNodes.Id;
                RacingBeacon node;
                if (TryGetNode(p, out node))
                    node.Insert(trackName);
            });
        }

        protected override bool ValidateLength(int len)
        {
            return len == 3;
        }
    }
}
