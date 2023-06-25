using Sandbox.ModAPI;
using System;
using ProtoBuf;
using VRage.Utils;
using avaness.RacingMod.Race.Modes;
using System.Xml.Serialization;

namespace avaness.RacingMod
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
                    if(LoopedChanged != null)
                        LoopedChanged.Invoke(Looped);
                    if (NumLapsChanged != null)
                        NumLapsChanged.Invoke(value);
                }
            }
        }
        public event Action<int> NumLapsChanged;

        /// <summary>
        /// Used only for backwards compatibility
        /// </summary>
        public bool? TimedMode
        {
            get
            {
                return null;
            }
            set
            {
                if (value.HasValue)
                    ModeType = value.Value ? (byte)TrackModeType.Qualify : (byte)TrackModeType.Distance;
            }
        }
        public bool ShouldSerializeTimedMode()
        {
            return false; // This prevents the obsolete TimedMode from going back into the settings file
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

        private byte mode = (byte)TrackModeType.Distance;
        [ProtoMember(5)]
        public byte ModeType
        {
            get
            {
                return mode;
            }
            set
            {
                if (!Enum.IsDefined(typeof(TrackModeType), value))
                    value = (byte)TrackModeType.Distance;

                if (value != mode)
                {
                    mode = value;
                    Sync(new Packet(PacketEnum.Mode, mode));
                    if (ModeChanged != null)
                        ModeChanged.Invoke(Mode);
                }
            }
        }

        private TrackModeBase trackMode;
        public TrackModeBase Mode
        {
            get
            {
                TrackModeType trackModeType = (TrackModeType)mode;
                if (trackMode == null || trackMode.TypeEnum != trackModeType)
                    trackMode = TrackModeBase.Create(trackModeType);
                return trackMode;
            }
        }

        public event Action<TrackModeBase> ModeChanged;

        [ProtoMember(6)]
        private string selectedTrack = RacingConstants.DefaultTrackId;
        public string SelectedTrack
        {
            get
            {
                if (string.IsNullOrWhiteSpace(selectedTrack))
                    return RacingConstants.DefaultTrackId;
                return selectedTrack;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = RacingConstants.DefaultTrackId;
                else
                    value = value.ToLowerInvariant();


                if (value != selectedTrack)
                {
                    selectedTrack = value;
                    Sync(new Packet(PacketEnum.SelectedTrack, selectedTrack));
                    if (SelectedTrackChanged != null)
                        SelectedTrackChanged.Invoke(value);
                }
            }
        }
        public event Action<string> SelectedTrackChanged;

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

            strictStart = config.strictStart;
            if (StrictStartChanged != null)
                StrictStartChanged.Invoke(strictStart);

            looped = config.looped;
            if (LoopedChanged != null)
                LoopedChanged.Invoke(looped);

            mode = config.mode;
            if (ModeChanged != null)
                ModeChanged.Invoke(Mode);

            selectedTrack = config.selectedTrack;
            if (SelectedTrackChanged != null)
                SelectedTrackChanged.Invoke(SelectedTrack);
        }

        public void Unload()
        {
            NumLapsChanged = null;
            StrictStartChanged = null;
            LoopedChanged = null;
            ModeChanged = null;
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
            //TimedMode = 1,
            StrictStart = 2,
            Looped = 3,
            Mode = 4,
            SelectedTrack = 5
        }

        [ProtoContract]
        public class Packet
        {
            [ProtoMember(1)]
            private readonly byte type;
            [ProtoMember(2)]
            private readonly byte value;
            [ProtoMember(3)]
            private readonly string valueString;

            public Packet()
            {

            }

            public Packet(PacketEnum type, string value)
            {
                this.type = (byte)type;
                this.valueString = value;
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
                    case PacketEnum.Mode:
                        if (!Enum.IsDefined(typeof(TrackModeType), value))
                            config.mode = (byte)TrackModeType.Distance;
                        else
                            config.mode = value;
                        if (config.ModeChanged != null)
                            config.ModeChanged.Invoke(config.Mode);
                        break;
                    case PacketEnum.SelectedTrack:
                        if (string.IsNullOrWhiteSpace(valueString))
                            config.selectedTrack = RacingConstants.DefaultTrackId;
                        else
                            config.selectedTrack = valueString;
                        if (config.SelectedTrackChanged != null)
                            config.SelectedTrackChanged.Invoke(config.selectedTrack);
                        break;

                }
            }
        }
    }
}
