using System;
using System.Collections;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Racers
{
    public class RacerStorage : IEnumerable<StaticRacerInfo>
    {
        private readonly Dictionary<ulong, StaticRacerInfo> staticRacerInfo = new Dictionary<ulong, StaticRacerInfo>();


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
            info = new StaticRacerInfo(RacingTools.GetPlayer(id));
            staticRacerInfo.Add(id, info);
            return info;
        }

        public StaticRacerInfo GetStaticInfo(IMyPlayer p)
        {
            StaticRacerInfo info;
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
            {
                info.Racer = p;
                return info;
            }
            info = new StaticRacerInfo(p);
            staticRacerInfo.Add(p.SteamUserId, info);
            return info;
        }

        public void ClearRecorders()
        {
            foreach (StaticRacerInfo info in staticRacerInfo.Values)
                info.Recorder?.ClearData();
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
