using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage.Game.Components;
using VRage.ModAPI;

namespace avaness.RacingMod.Beacon
{
    [ProtoContract]
    public class BeaconStorage
    {
        [ProtoMember(1)]
        public bool Enabled = false;

        [ProtoMember(2)]
        public bool Checkpoint = false;

        [ProtoMember(3)]
        public float NodeNum = 0;

        [ProtoMember(4)]
        public long EntityId;

        [ProtoMember(5)]
        public bool GridPosition = true;

        [ProtoMember(6)]
        public string TrackName = RacingConstants.DefaultTrackId;

        [ProtoMember(7)]
        public float CheckpointSize = 0;

        private readonly MyModStorageComponentBase storage;

        private BeaconStorage temp;
        public BeaconStorage Temporary => temp;

        /// <summary>
        /// Called when data is modified just before it is saved.
        /// Do not modify settings in this method unless you are server!
        /// </summary>
        public event Action OnDataReceived;

        /// <summary>
        /// Used for serialization.
        /// </summary>
        public BeaconStorage()
        {

        }

        public BeaconStorage(IMyEntity e)
        {
            EntityId = e.EntityId;
            storage = e.Storage;
        }

        public void Load()
        {
            string base64;
            if (storage.TryGetValue(RacingConstants.beaconStorage, out base64))
            {
                byte[] data = Convert.FromBase64String(base64);
                BeaconStorage storage = MyAPIGateway.Utilities.SerializeFromBinary<BeaconStorage>(data);
                if (storage != null)
                {
                    Copy(storage);
                    return;
                }
            }

            Save();
        }

        public void Unload()
        {
            OnDataReceived = null;
        }

        public void Save()
        {
            byte[] data = MyAPIGateway.Utilities.SerializeToBinary(this);
            if (RacingConstants.IsServer)
                SetStorage(data);
            else
                RacingSession.Instance.Net.SendToServer(RacingConstants.packetBeaconSettings, data);
        }

        public static void Register()
        {
            RacingSession.Instance.Net.Register(RacingConstants.packetBeaconSettings, PacketReceived);
        }

        private static void PacketReceived(ulong sender, byte[] data)
        {
            BeaconStorage storage = MyAPIGateway.Utilities.SerializeFromBinary<BeaconStorage>(data);
            if(storage != null)
            {
                IMyEntity e = MyAPIGateway.Entities.GetEntityById(storage.EntityId);
                if(e != null)
                {
                    BeaconStorage s = e.GameLogic.GetAs<RacingBeacon>()?.Storage;
                    if(s != null)
                        s.PacketReceived(storage);
                }
            }
        }

        private void PacketReceived(BeaconStorage storage)
        {
            if (storage != null)
            {
                Copy(storage);
                if (RacingConstants.IsServer)
                {
                    byte[] newData = MyAPIGateway.Utilities.SerializeToBinary<BeaconStorage>(this); // Data from the packet cant be used as it is might have been modified.
                    SetStorage(newData);
                }
            }
        }

        private void SetStorage(byte[] data)
        {
             
            storage[RacingConstants.beaconStorage] = Convert.ToBase64String(data);
            RacingSession.Instance.Net.SendToOthers(RacingConstants.packetBeaconSettings, data);
        }

        private void Copy(BeaconStorage other)
        {
            Enabled = other.Enabled;
            Checkpoint = other.Checkpoint;
            CheckpointSize = other.CheckpointSize;
            if (CheckpointSize < 0)
                CheckpointSize = 0;
            NodeNum = other.NodeNum;
            TrackName = other.TrackName;
            if (string.IsNullOrWhiteSpace(TrackName))
                TrackName = RacingConstants.DefaultTrackId;
            GridPosition = other.GridPosition;
            if (OnDataReceived != null)
                OnDataReceived.Invoke();
        }

        public bool CreateTemp()
        {
            if (temp != null)
                return false;
            temp = new BeaconStorage();
            temp.Copy(this);
            return true;
        }

        public void DeleteTemp()
        {
            temp = null;
        }

        public void ApplyTemp()
        {
            if (temp == null)
                return;

            Copy(temp);
            Save();
            temp = null;
        }

        public bool HasTemp()
        {
            return temp != null;
        }

        public void BackwardsCompatibility(IMyTerminalBlock block)
        {
            string name = block.CustomName;
            if (name.StartsWith("[finish]", StringComparison.OrdinalIgnoreCase))
            {
                Enabled = true;
                Checkpoint = true;
                NodeNum = RacingConstants.finishCompatNum;
                TrackName = RacingConstants.DefaultTrackId;
                Save();
                block.CustomName = CompatRemoveTag(name, 8);
            }
            else if(name.StartsWith("[node]", StringComparison.OrdinalIgnoreCase))
            {
                Checkpoint = false;
                TrackName = RacingConstants.DefaultTrackId;
                float num;
                if(float.TryParse(block.CustomData, out num))
                {
                    Enabled = true;
                    NodeNum = num;
                }
                else
                {
                    Enabled = false;
                }
                Save();
                block.CustomName = CompatRemoveTag(name, 6);
            }
            else if(name.StartsWith("[checkpoint]", StringComparison.OrdinalIgnoreCase))
            {
                Checkpoint = true;
                TrackName = RacingConstants.DefaultTrackId;
                float num;
                if (float.TryParse(block.CustomData, out num))
                {
                    Enabled = true;
                    NodeNum = num;
                }
                else
                {
                    Enabled = false;
                }
                Save();
                block.CustomName = CompatRemoveTag(name, 12);
            }

        }

        private string CompatRemoveTag(string name, int len)
        {
            if (name.Length == len)
                return name.Substring(1, len - 2);
            return name.Substring(1, len - 2) + ' ' + name.Substring(len);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ Enabled:").Append(Enabled).Append(", IsCheckpoint:").Append(Checkpoint).Append(", NodeNum:").Append(NodeNum).Append(" }");
            return sb.ToString();
        }
    }
}
