using ProtoBuf;
using System;

namespace avaness.RacingPaths.Data
{
    [ProtoContract]
    public class TimeOnlyPath : IPath
    {
        [ProtoMember(1)]
        private long Ticks
        {
            get
            {
                return Length.Ticks;
            }
            set
            {
                Length = new TimeSpan(value);
            }
        }

        [ProtoMember(2)]
        public string PlayerName { get; private set; }

        [ProtoMember(3)]
        public ulong PlayerId { get; private set; }

        public TimeSpan Length { get; private set; }
    }
}
