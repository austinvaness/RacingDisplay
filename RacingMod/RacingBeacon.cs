using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
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
            RACING_NODE,
            CHECKPOINT
        }

        public BeaconType Type { get; private set; } = BeaconType.IGNORED;
        public IMyBeacon Beacon { get; private set; } = null;
        public float NodeNumber { get; private set; } = float.NaN;

        public bool Valid => !float.IsNaN(NodeNumber);
        public BoundingBoxD CollisionArea => Beacon.CubeGrid.WorldAABB;
        public Vector3 Coords => Type == BeaconType.CHECKPOINT ? CollisionArea.Center : Beacon.WorldMatrix.Translation;

        public override void Init (MyObjectBuilder_EntityBase objectBuilder)
        {
            Beacon = Entity as IMyBeacon;
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
                
                if (Beacon != null)
                {
                    // register events
                    Beacon.CustomNameChanged += OnCustomNameChanged;
                    Beacon.CustomDataChanged += OnCustomDataChanged;
                }
            }
        }
        
        public override void Close ()
        {
            if (Valid)
                RacingSession.Instance.RemoveNode(this);
            
            // deregister events
            Beacon.CustomNameChanged -= OnCustomNameChanged;
            Beacon.CustomDataChanged -= OnCustomDataChanged;

            NeedsUpdate = MyEntityUpdateEnum.NONE;
            
            Beacon = null;
        }
        
        public override void UpdateBeforeSimulation100 ()
        {
            if (RacingSession.Instance == null)
                return;

            // update once to initialise once the RacingSession instance exists
            OnCustomNameChanged(Beacon);
            OnCustomDataChanged(Beacon);

            // then bail out for good
            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        private void OnCustomNameChanged(IMyTerminalBlock obj)
        {
            BeaconType oldType = Type;
            
            if (Beacon.CustomName.StartsWith("[Node]"))
                Type = BeaconType.RACING_NODE;
            else if (Beacon.CustomName.StartsWith("[CP]") || Beacon.CustomName.StartsWith("[Checkpoint]"))
                Type = BeaconType.CHECKPOINT;
            else
                Type = BeaconType.IGNORED;

            if (Type != oldType && Valid)
            {
                if(Type == BeaconType.IGNORED)
                    RacingSession.Instance.RemoveNode(this);
                else
                    RacingSession.Instance.RegisterNode(this);
            }
        }

        private void OnCustomDataChanged(IMyTerminalBlock obj)
        {
            // read node number from custom data
            float newNodeNumber;
            UpdateNodeNumber(float.TryParse(Beacon.CustomData, out newNodeNumber) ? newNodeNumber : float.NaN);
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
            return this.NodeNumber.CompareTo(other.NodeNumber);
        }

        public bool Equals(RacingBeacon other)
        {
            if (other == null)
                return !Valid;
            return this.NodeNumber.Equals(other.NodeNumber);
        }
    }
}
