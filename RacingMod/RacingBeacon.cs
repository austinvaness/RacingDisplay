using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace RacingMod
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false)]
    public class RacingBeacon : MyGameLogicComponent, IComparable<RacingBeacon>, IEquatable<RacingBeacon>
    {
        // Import a potential [finish] node to support older maps.
        public static bool finishCompatibility = true;

        public enum BeaconType
        {
            IGNORED,
            NODE,
            CHECKPOINT, // Racer must pass through to count
        }

        public BeaconType Type { get; private set; } = BeaconType.IGNORED;
        public IMyBeacon Beacon { get; private set; } = null;
        public float NodeNumber { get; private set; } = float.NaN;

        public bool Valid => !float.IsNaN(NodeNumber);
        public Vector3 Coords => multiBeaconMode ? Beacon.GetPosition() : Beacon.CubeGrid.WorldAABB.Center;

        private bool events = true;
        private bool multiBeaconMode = false;

        public bool Contains (IMyPlayer p)
        {
            MatrixD gridMatrix;
            BoundingBoxD gridAABB;
            GetGridInfo(out gridAABB, out gridMatrix);
            return Contains(p, ref gridAABB, ref gridMatrix);
        }

        public void DrawDebug()
        {
            MatrixD gridMatrix;
            BoundingBoxD gridAABB;
            GetGridInfo(out gridAABB, out gridMatrix);

            Color color;
            if (Contains(MyAPIGateway.Session.Player, ref gridAABB, ref gridMatrix))
                color = Color.Green;
            else
                color = Color.Red;
            color.A = 1;

            MyStringId material = MyStringId.GetOrCompute("Square");
            MySimpleObjectDraw.DrawTransparentBox(ref gridMatrix, ref gridAABB, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 1, 0.01f, material, material, blendType: BlendTypeEnum.PostPP);
        }


        private void GetGridInfo(out BoundingBoxD gridAABB, out MatrixD gridMatrix)
        {
            if(InvalidGrid(Beacon))
            {
                gridAABB = Beacon.LocalAABB;
                gridAABB = gridAABB.Inflate(15);
                gridMatrix = Beacon.WorldMatrix;
            }
            else
            {
                gridAABB = Beacon.CubeGrid.LocalAABB;
                gridMatrix = Beacon.CubeGrid.WorldMatrix;
            }
        }

        private bool Contains(IMyPlayer p, ref BoundingBoxD gridAABB, ref MatrixD gridMatrix)
        {
            MatrixD transMatrix = MatrixD.Transpose(gridMatrix);
            IMyEntity e = RacingTools.GetCockpit(p)?.CubeGrid;
            if (e == null)
                e = p.Character;

            if (e == null)
                return false;

            Vector3D gridLocalPos = Vector3D.TransformNormal(e.WorldVolume.Center - gridMatrix.Translation, transMatrix);
            if (gridAABB.Intersects(new BoundingSphereD(gridLocalPos, e.WorldVolume.Radius)))
                return true;

            if (e?.Physics == null)
                return false;

            Vector3D vel = e.Physics.LinearVelocity;
            if (vel == Vector3D.Zero)
                return false;

            Vector3D direction = Vector3D.TransformNormal(e.Physics.LinearVelocity / -300, transMatrix);
            double speed = direction.Length();
            direction /= speed;
            double? result = gridAABB.Intersects(new Ray(e.GetPosition(), direction));
            if (!result.HasValue)
                return false;

            return result.Value > 0 && result.Value < speed;
        }

        public override void Init (MyObjectBuilder_EntityBase objectBuilder)
        {
            Beacon = Entity as IMyBeacon;

            if (RacingConstants.IsServer)
            {
                // wait for the grid to fully load in
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;

                // register events
                Beacon.CustomNameChanged += OnCustomNameChanged;
                Beacon.CustomDataChanged += OnCustomDataChanged;
            }
        }

        public override void Close ()
        {
            if (Valid && RacingSession.Instance?.Nodes != null)
                RacingSession.Instance.Nodes.RemoveNode(this);

            NodeNumber = float.NaN;

            if(Beacon != null)
            {
                Beacon.CustomNameChanged -= OnCustomNameChanged;
                Beacon.CustomDataChanged -= OnCustomDataChanged;
            }

            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void UpdateAfterSimulation10 ()
        {
            if(Type != BeaconType.IGNORED && Valid)
            {
                multiBeaconMode = !IsAlone();
            }
        }

        /// <summary>
        /// Returns true if the beacon is the only beacon on the grid
        /// </summary>
        private bool IsAlone ()
        {
            if (!InvalidGrid(Beacon))
            {
                long me = Beacon.EntityId;
                long myGrid = Beacon.CubeGrid.EntityId;
                foreach (RacingBeacon b in RacingSession.Instance.Nodes)
                {
                    if (b.Beacon.EntityId != me && b.Beacon.CubeGrid != null && b.Beacon.CubeGrid.EntityId == myGrid)
                        return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        private bool InvalidGrid(IMyCubeBlock block)
        {
            return block.CubeGrid == null || block.CubeGrid.MarkedForClose;
        }

        public override void UpdateBeforeSimulation100 ()
        {
            try
            {
                if (RacingSession.Instance?.Nodes == null)
                    return;

                if (Beacon?.CubeGrid?.Physics == null)
                    return;

                if (Beacon.CubeGrid.IsStatic)
                {
                    // [finish] backwards compatibility
                    if (finishCompatibility && Beacon.CustomName.ToLower().StartsWith("[finish]"))
                    {
                        if (Beacon.CustomName.Length > 8)
                            Beacon.CustomName = "[Checkpoint]" + Beacon.CustomName.Substring(8);
                        else
                            Beacon.CustomName = "[Checkpoint]";
                        Beacon.CustomData = (float.MaxValue * 0.5f).ToString();
                        finishCompatibility = false;
                    }

                    // update once to initialize
                    OnCustomDataChanged(Beacon);
                    OnCustomNameChanged(Beacon);
                }
                else
                {
                    // we only want to be active for static grids
                    Beacon.CustomNameChanged -= OnCustomNameChanged;
                    Beacon.CustomDataChanged -= OnCustomDataChanged;
                }

                // done waiting, bail out of here for good
                NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void OnCustomNameChanged (IMyTerminalBlock block)
        {
            try
            {
                if (Beacon?.CustomName == null)
                    return;

                string name = Beacon.CustomName.ToLower();
                BeaconType oldType = Type;
                if (name.StartsWith("[node]"))
                    Type = BeaconType.NODE;
                else if (name.StartsWith("[checkpoint]"))
                    Type = BeaconType.CHECKPOINT;
                else
                    Type = BeaconType.IGNORED;

                if (Type != BeaconType.IGNORED && !Valid)
                    GenerateNodeNumber();

                if (Type != oldType)
                {
                    if (oldType != BeaconType.IGNORED)
                        RacingSession.Instance.Nodes.RemoveNode(this);

                    if (Type != BeaconType.IGNORED)
                        RacingSession.Instance.Nodes.RegisterNode(this);
                }
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        public void OnCustomDataChanged (IMyTerminalBlock block)
        {
            if (!events)
                return;
            events = false;
            try
            {
                float newNodeNumber;
                if (float.TryParse(block.CustomData, out newNodeNumber))
                    UpdateNodeNumber(newNodeNumber);
                else if(Type != BeaconType.IGNORED)
                    GenerateNodeNumber();
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
            events = true;
        }

        private void UpdateNodeNumber (float newNodeNumber)
        {
            NodeNumber = newNodeNumber;
            events = false;
            Beacon.CustomData = newNodeNumber.ToString();
            events = true;
            if (Type != BeaconType.IGNORED)
            {
                RacingSession.Instance.Nodes.RemoveNode(this);
                RacingSession.Instance.Nodes.RegisterNode(this);
            }
        }

        private void GenerateNodeNumber()
        {
            UpdateNodeNumber(RacingSession.Instance.Nodes.GenerateNodeNumber(Coords));
        }

        public int CompareTo (RacingBeacon other)
        {
            return NodeNumber.CompareTo(other.NodeNumber);
        }

        public bool Equals (RacingBeacon other)
        {
            if (other == null)
                return !Valid;
            return NodeNumber.Equals(other.NodeNumber);
        }
    }
}
