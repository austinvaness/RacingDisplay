using Sandbox.ModAPI;
using System;
using ProtoBuf;

namespace RacingMod
{
    [ProtoContract]
    public class RacingMapSettings
    {

        public RacingMapSettings ()
        {

        }

        [ProtoMember(1)]
        private byte numLaps = 1;
        public int NumLaps
        {
            get
            {
                return numLaps;
            }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > byte.MaxValue)
                    value = byte.MaxValue;

                if (value != numLaps)
                {
                    numLaps = (byte)value;
                    Sync(new Packet(PacketEnum.NumLaps, numLaps));
                }
            }
        }

        [ProtoMember(2)]
        private bool timedMode = false;
        public bool TimedMode
        {
            get
            {
                return timedMode;
            }
            set
            {
                if(value != timedMode)
                {
                    timedMode = value;
                    Sync(new Packet(PacketEnum.TimedMode, timedMode));
                }
            }
        }

        [ProtoMember(3)]
        private bool strictStart = true;
        public bool StrictStart
        {
            get
            {
                return strictStart;
            }
            set
            {
                if (value != strictStart)
                {
                    strictStart = value;
                    Sync(new Packet(PacketEnum.StrictStart, strictStart));
                }
            }
        }

        public void SaveFile ()
        {
            if (RacingConstants.IsServer)
            {
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(RacingConstants.mapFile, typeof(RacingMapSettings));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(this));
                writer.Flush();
                writer.Close();
            }
        }

        public void Copy(RacingMapSettings config)
        {
            numLaps = config.numLaps;
            timedMode = config.timedMode;
            strictStart = config.strictStart;
        }

        public static RacingMapSettings LoadFile ()
        {
            try
            {
                if (RacingConstants.IsServer && MyAPIGateway.Utilities.FileExistsInWorldStorage(RacingConstants.mapFile, typeof(RacingMapSettings)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(RacingConstants.mapFile, typeof(RacingMapSettings));
                    string xmlText = reader.ReadToEnd();
                    reader.Close();
                    RacingMapSettings config = MyAPIGateway.Utilities.SerializeFromXML<RacingMapSettings>(xmlText);
                    if (config == null)
                        throw new NullReferenceException("Failed to serialize from xml.");
                    else
                        return config;
                }
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, typeof(RacingMapSettings));
            }

            RacingMapSettings result = new RacingMapSettings();
            result.SaveFile();
            return result;
        }

        private void Sync(Packet p)
        {
            if (RacingConstants.IsServer)
                MyAPIGateway.Multiplayer.SendMessageToOthers(RacingConstants.packetSettings, MyAPIGateway.Utilities.SerializeToBinary(p));
            else
                MyAPIGateway.Multiplayer.SendMessageToServer(RacingConstants.packetSettings, MyAPIGateway.Utilities.SerializeToBinary(p));
        }

        public enum PacketEnum : byte
        {
            NumLaps = 0,
            TimedMode = 1,
            StrictStart = 2,
            MaxWheels = 3
        }

        [ProtoContract]
        public class Packet
        {
            [ProtoMember(1)]
            private readonly byte type;
            [ProtoMember(2)]
            private readonly byte value;

            public Packet()
            {

            }

            public Packet(PacketEnum type, bool value)
            {
                this.type = (byte)type;
                if (value)
                    this.value = 1;
                else
                    this.value = 0;
            }

            public Packet(PacketEnum type, byte value)
            {
                this.type = (byte)type;
                this.value = value;
            }

            public void Perform()
            {
                RacingMapSettings config = RacingSession.Instance.MapSettings;
                switch ((PacketEnum)type)
                {
                    case PacketEnum.NumLaps:
                        config.numLaps = value;
                        RacingSession.Instance.UpdateUI_NumLaps();
                        break;
                    case PacketEnum.TimedMode:
                        bool b1 = value == 1;
                        config.timedMode = b1;
                        RacingSession.Instance.UpdateUI_TimedMode();
                        break;
                    case PacketEnum.StrictStart:
                        bool b2 = value == 1;
                        config.strictStart = b2;
                        RacingSession.Instance.UpdateUI_StrictStart();
                        break;
                }
            }
        }
    }

    public partial class RacingSession
    {
        public override void SaveData ()
        {
            if (RacingConstants.IsServer && MapSettings != null)
                MapSettings.SaveFile();
        }

        private void ReceiveSettings (byte [] data)
        {
            try
            {
                RacingMapSettings.Packet p = MyAPIGateway.Utilities.SerializeFromBinary<RacingMapSettings.Packet>(data);
                p.Perform();
                if (RacingConstants.IsServer)
                    MyAPIGateway.Multiplayer.SendMessageToOthers(RacingConstants.packetSettings, data);
            }
            catch(Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void ReceiveSettingsInit (byte [] data)
        {
            try
            {
                if(RacingConstants.IsServer)
                {
                    ulong id = BitConverter.ToUInt64(data, 0);
                    if (id != 0)
                    {
                        byte [] config = MyAPIGateway.Utilities.SerializeToBinary(MapSettings);
                        MyAPIGateway.Multiplayer.SendMessageTo(RacingConstants.packetSettingsInit, config, id);
                    }
                }
                else
                {
                    RacingMapSettings config = MyAPIGateway.Utilities.SerializeFromBinary<RacingMapSettings>(data);
                    if (config != null)
                        MapSettings.Copy(config);
                }
                UpdateUI_Admin();
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }
    }
}
