using System;
using System.Globalization;
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
            FINISH
        }

        public BeaconType Type { get; private set; } = BeaconType.IGNORED;
        public IMyBeacon Beacon { get; private set; } = null;
        public float NodeNumber { get; private set; } = float.NaN;

        public bool Valid => !float.IsNaN(NodeNumber);
        public BoundingBoxD CollisionArea => Beacon.CubeGrid.WorldAABB;
        public Vector3 Coords => GetCoords();

        private Vector3 GetCoords ()
        {
            if (Type == BeaconType.FINISH)
                return CollisionArea.Center;
            return Beacon.WorldMatrix.Translation;
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

        private void OnCustomNameChanged(IMyTerminalBlock obj)
        {
            if (Beacon?.CustomName == null)
                return;

            BeaconType oldType = Type;
            if (Beacon.CustomName.StartsWith("[Node]"))
                Type = BeaconType.RACING_NODE;
            else if (Beacon.CustomName.StartsWith("[Finish]"))
                Type = BeaconType.FINISH;
            else
                Type = BeaconType.IGNORED;

            if (Type != oldType && Valid)
            {
                if(oldType != BeaconType.IGNORED)
                    RacingSession.Instance.RemoveNode(this);
                
                if(Type != BeaconType.IGNORED)
                    RacingSession.Instance.RegisterNode(this);
            }
        }

        private void OnCustomDataChanged(IMyTerminalBlock obj)
        {
            // read node number from custom data
            float newNodeNumber;
            if (!float.TryParse(Beacon.CustomData, out newNodeNumber))
                newNodeNumber = float.NaN;
            UpdateNodeNumber(newNodeNumber);
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
            return NodeNumber.CompareTo(other.NodeNumber);
        }

        public bool Equals(RacingBeacon other)
        {
            if (other == null)
                return !Valid;
            return NodeNumber.Equals(other.NodeNumber);
        }
    }
}
