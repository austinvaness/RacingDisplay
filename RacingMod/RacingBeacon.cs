using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace RacingMod
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false)]
    public class RacingBeacon : MyGameLogicComponent, IComparable<RacingBeacon>, IEquatable<RacingBeacon>
    {
        public enum BeaconType
        {
            IGNORED,
            NODE,
            CHECKPOINT, // Racer must pass through to count
            FINISH // Node is last node
        }

        public BeaconType Type { get; private set; } = BeaconType.IGNORED;
        public IMyBeacon Beacon { get; private set; } = null;
        public float NodeNumber { get; private set; } = float.NaN;

        public bool Valid => !float.IsNaN(NodeNumber);
        public Vector3 Coords => Beacon.CubeGrid.WorldAABB.Center;
        public int Index = -1;

        public bool Contains(IMyPlayer p)
        {
            MatrixD transMatrix = MatrixD.Transpose(Beacon.CubeGrid.WorldMatrix);
            IMyEntity e = RacingSession.GetCockpit(p)?.CubeGrid;
            if (e == null)
                return Beacon.CubeGrid.LocalAABB.Contains(Vector3D.TransformNormal(p.GetPosition() - Beacon.CubeGrid.WorldMatrix.Translation, transMatrix)) != ContainmentType.Disjoint;
            return Beacon.CubeGrid.LocalAABB.Intersects(
                new BoundingSphereD(Vector3D.TransformNormal(e.WorldVolume.Center - Beacon.CubeGrid.WorldMatrix.Translation, transMatrix), e.WorldVolume.Radius)
                );
        }

        public override void Init (MyObjectBuilder_EntityBase objectBuilder)
        {
            Beacon = Entity as IMyBeacon;
            
            if (MyAPIGateway.Multiplayer.IsServer)
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
            if (Valid)
                RacingSession.Instance.RemoveNode(this);
            NodeNumber = float.NaN;

            // Unregister events
            Beacon.CustomNameChanged -= OnCustomNameChanged;
            Beacon.CustomDataChanged -= OnCustomDataChanged;

            NeedsUpdate = MyEntityUpdateEnum.NONE;

            Beacon = null;
        }

        public override void UpdateBeforeSimulation100 ()
        {
            try
            {
                if (RacingSession.Instance == null)
                    return;

                if (Beacon?.CubeGrid?.Physics == null)
                    return;

                if (Beacon.CubeGrid.IsStatic)
                {
                    // update once to initialize
                    OnCustomNameChanged(Beacon);
                    OnCustomDataChanged(Beacon);
                }
                else
                {
                    // we only want to be active for static grids
                    Beacon.CustomNameChanged -= OnCustomNameChanged;
                    Beacon.CustomDataChanged -= OnCustomDataChanged;
                }

                // done waiting, bail out of here for good
                NeedsUpdate = MyEntityUpdateEnum.NONE;
            }
            catch (Exception e)
            {
                RacingSession.ShowError(e, GetType());
            }
        }

        private void OnCustomNameChanged(IMyTerminalBlock obj)
        {
            try
            {
                if (Beacon?.CustomName == null)
                    return;

                string name = Beacon.CustomName.ToLower();
                BeaconType oldType = Type;
                if (name.StartsWith("[node]"))
                    Type = BeaconType.NODE;
                else if (name.StartsWith("[finish]"))
                    Type = BeaconType.FINISH;
                else if (name.StartsWith("[checkpoint]"))
                    Type = BeaconType.CHECKPOINT;
                else
                    Type = BeaconType.IGNORED;

                if (Type != oldType && Valid)
                {
                    if (oldType != BeaconType.IGNORED)
                        RacingSession.Instance.RemoveNode(this);

                    if (Type != BeaconType.IGNORED)
                        RacingSession.Instance.RegisterNode(this);
                }
            }
            catch(Exception e)
            {
                RacingSession.ShowError(e, GetType());
            }
        }

        public void OnCustomDataChanged(IMyTerminalBlock obj)
        {
            try
            {
                // Read node number from custom data
                float newNodeNumber;
                if (!float.TryParse(Beacon.CustomData, out newNodeNumber))
                    newNodeNumber = float.NaN;
                UpdateNodeNumber(newNodeNumber);
            }
            catch (Exception e)
            {
                RacingSession.ShowError(e, GetType());
            }
        }

        private void UpdateNodeNumber(float newNodeNumber)
        {
            if (Valid)
                RacingSession.Instance.RemoveNode(this);

            NodeNumber = newNodeNumber;
            
            if (Valid && Type != BeaconType.IGNORED)
                RacingSession.Instance.RegisterNode(this);
        }

        public int CompareTo(RacingBeacon other)
        {
            if (Type == BeaconType.FINISH)
                return 1;
            return NodeNumber.CompareTo(other.NodeNumber);
        }

        public bool Equals(RacingBeacon other)
        {
            if (other == null)
                return !Valid;
            return NodeNumber.Equals(other.NodeNumber) && Type != BeaconType.FINISH;
        }
    }
}
