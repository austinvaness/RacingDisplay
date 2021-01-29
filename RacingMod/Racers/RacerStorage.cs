using System;
using avaness.RacingMod.Race.Finish;
using System.Collections;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Racers
{
    public class RacerStorage : IEnumerable<StaticRacerInfo>
    {
        private readonly Dictionary<ulong, StaticRacerInfo> staticRacerInfo = new Dictionary<ulong, StaticRacerInfo>();
        private readonly Dictionary<ulong, SerializableFinisher> data = new Dictionary<ulong, SerializableFinisher>();

        public void LoadData(IEnumerable<SerializableFinisher> data)
        {
            foreach (SerializableFinisher finisher in data)
            {
                StaticRacerInfo info;
                if (staticRacerInfo.TryGetValue(finisher.Id, out info))
                    info.UpdateTime(finisher);
                else
                    this.data[finisher.Id] = finisher;
            }
        }

        public bool GetStaticInfo(string name, out StaticRacerInfo info)
        {
            foreach (StaticRacerInfo i in staticRacerInfo.Values)
            {
                if (i.Name.ToLower().Contains(name))
                {
                    info = i;
                    return true;
                }
            }

            info = null;
            return false;
        }

        public bool GetStaticInfo(IMyPlayer p, out StaticRacerInfo info)
        {
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
            {
                info.Racer = p;
                return true;
            }
            return false;
        }

        public StaticRacerInfo GetStaticInfo(ulong id)
        {
            StaticRacerInfo info;
            if (staticRacerInfo.TryGetValue(id, out info))
                return info;
            return Create(RacingTools.GetPlayer(id));
        }

        public StaticRacerInfo GetStaticInfo(IMyPlayer p)
        {
            StaticRacerInfo info;
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
            {
                info.Racer = p;
                return info;
            }
            return Create(p);
        }

        private StaticRacerInfo Create(IMyPlayer p)
        {
            StaticRacerInfo info = new StaticRacerInfo(p);
            ulong id = p.SteamUserId;
            staticRacerInfo.Add(id, info);
            SerializableFinisher data;
            if (this.data.TryGetValue(id, out data))
            {
                info.UpdateTime(data);
                this.data.Remove(id);
            }
            return info;
        }

        public void SetMaxLaps(ulong id, int laps)
        {
            StaticRacerInfo info;
            if(staticRacerInfo.TryGetValue(id, out info))
                info.Laps = Math.Min(info.Laps, laps - 1);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return staticRacerInfo.Values.GetEnumerator();
        }
        public IEnumerator<StaticRacerInfo> GetEnumerator()
        {
            return staticRacerInfo.Values.GetEnumerator();
        }
    }
}
