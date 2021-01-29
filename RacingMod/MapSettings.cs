using Sandbox.ModAPI;
using System;
using ProtoBuf;
using VRage.Utils;

namespace avaness.RacingMod
{
    [ProtoContract]
    public class RacingMapSettings
    {

        public RacingMapSettings()
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
                    if (LoopedChanged != null)
                        LoopedChanged.Invoke(Looped);
                    if (NumLapsChanged != null)
                        NumLapsChanged.Invoke(value);
                }
            }
        }
        public event Action<int> NumLapsChanged;

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
                if (value != timedMode)
                {
                    timedMode = value;
                    Sync(new Packet(PacketEnum.TimedMode, timedMode));
                    if (TimedModeChanged != null)
                        TimedModeChanged.Invoke(value);
                }
            }
        }
        public event Action<bool> TimedModeChanged;

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
                    if (StrictStartChanged != null)
                        StrictStartChanged.Invoke(value);
                }
            }
        }
        public event Action<bool> StrictStartChanged;


        [ProtoMember(4)]
        private bool looped = false;
        /// <summary>
        /// If true, the start line is the finish line with only 1 lap.
        /// </summary>
        public bool Looped
        {
            get
            {
                return numLaps > 1 || looped;
            }
            set
            {
                if (value != looped)
                {
                    looped = value;
                    Sync(new Packet(PacketEnum.Looped, looped));
                    if (LoopedChanged != null)
                        LoopedChanged.Invoke(Looped);
                }
            }
        }
        public event Action<bool> LoopedChanged;

        [ProtoMember(5)]
        private bool botRecord = false;
        public bool BotRecord
        {
            get
            {
                return botRecord;
            }
            set
            {
                if(value != botRecord)
                {
                    botRecord = value;
                    Sync(new Packet(PacketEnum.BotRecord, botRecord));
                    if (BotRecordChanged != null)
                        BotRecordChanged.Invoke(value);
                }
            }
        }
        public event Action<bool> BotRecordChanged;

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
            if (NumLapsChanged != null)
                NumLapsChanged.Invoke(numLaps);

            timedMode = config.timedMode;
            if (TimedModeChanged != null)
                TimedModeChanged.Invoke(timedMode);

            strictStart = config.strictStart;
            if (StrictStartChanged != null)
                StrictStartChanged.Invoke(strictStart);

            looped = config.looped;
            if (LoopedChanged != null)
                LoopedChanged.Invoke(Looped);

            botRecord = config.botRecord;
            if (BotRecordChanged != null)
                BotRecordChanged.Invoke(botRecord);
        }

        public void Unload()
        {
            NumLapsChanged = null;
            TimedModeChanged = null;
            StrictStartChanged = null;
            LoopedChanged = null;
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
                RacingSession.Instance.Net.SendToOthers(RacingConstants.packetSettings, p);
            else
                RacingSession.Instance.Net.SendToServer(RacingConstants.packetSettings, p);
        }

        public enum PacketEnum : byte
        {
            NumLaps = 0,
            TimedMode = 1,
            StrictStart = 2,
            Looped = 3,
            BotRecord = 4
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
                        if (config.NumLapsChanged != null)
                            config.NumLapsChanged.Invoke(value);
                        break;
                    case PacketEnum.TimedMode:
                        bool b1 = value == 1;
                        config.timedMode = b1;
                        if (config.TimedModeChanged != null)
                            config.TimedModeChanged.Invoke(b1);
                        break;
                    case PacketEnum.StrictStart:
                        bool b2 = value == 1;
                        config.strictStart = b2;
                        if (config.StrictStartChanged != null)
                            config.StrictStartChanged.Invoke(b2);
                        break;
                    case PacketEnum.Looped:
                        bool b3 = value == 1;
                        config.looped = b3;
                        if (config.LoopedChanged != null)
                            config.LoopedChanged.Invoke(b3);
                        break;
                    case PacketEnum.BotRecord:
                        bool b4 = value == 1;
                        config.botRecord = b4;
                        if (config.BotRecordChanged != null)
                            config.BotRecordChanged.Invoke(b4);
                        break;
                }
            }
        }
    }
}
