using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace avaness.RacingMod.Race.Finish
{
    public class SerializableFinisher : IFinisher, IEquatable<SerializableFinisher>
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public long Time 
        { 
            get
            {
                return BestTime.Ticks;
            }
            set
            {
                BestTime = new TimeSpan(value);
            }
        }

        [XmlIgnore]
        public TimeSpan BestTime { get; set; }

        public SerializableFinisher()
        {

        }

        public SerializableFinisher(IFinisher info)
        {
            Id = info.Id;
            Name = info.Name;
            BestTime = info.BestTime;
        }

        public void RemoveFinish()
        {

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SerializableFinisher);
        }

        public bool Equals(SerializableFinisher other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }

        public static bool operator ==(SerializableFinisher left, SerializableFinisher right)
        {
            return EqualityComparer<SerializableFinisher>.Default.Equals(left, right);
        }

        public static bool operator !=(SerializableFinisher left, SerializableFinisher right)
        {
            return !(left == right);
        }
    }
}
