using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingMod
{
    public class FinishList : IEnumerable<StaticRacerInfo>
    {
        private readonly StringBuilder tempSb = new StringBuilder();
        private string finalString;
        private RacingMapSettings MapSettings => RacingSession.Instance.MapSettings;
        private SortedSet<StaticRacerInfo> finishers;

        public FinishList()
        {
            MapSettings.TimedModeChanged += TimedModeChanged;
            if (MapSettings.TimedMode)
                finishers = new SortedSet<StaticRacerInfo>(new RacerBestTimeComparer());
            else
                finishers = new SortedSet<StaticRacerInfo>(new RacerRankComparer());
        }

        public int Count => finishers.Count;

        public void Add(StaticRacerInfo info)
        {
            finishers.Remove(info);
            finishers.Add(info);
            FinishersModifed();
        }

        public bool Remove(StaticRacerInfo info)
        {
            if (finishers.Remove(info))
            {
                info.RemoveFinish();
                FinishersModifed();
                return true;
            }
            return false;
        }

        public void Clear()
        {
            foreach (StaticRacerInfo info in finishers)
                info.RemoveFinish();
            finishers.Clear();
            FinishersModifed();
        }

        public void Unload()
        {
            MapSettings.TimedModeChanged -= TimedModeChanged;
        }

        private void TimedModeChanged (bool timed)
        {
            SortedSet<StaticRacerInfo> temp;
            if(timed)
                temp = new SortedSet<StaticRacerInfo>(new RacerBestTimeComparer());
            else
                temp = new SortedSet<StaticRacerInfo>(new RacerRankComparer());
            foreach (StaticRacerInfo info in finishers)
                temp.Add(info);
            finishers = temp;
            FinishersModifed();
        }

        // Build the final racer text
        private void FinishersModifed ()
        {
            tempSb.Clear();

            if (finishers.Count > 0)
            {
                tempSb.Append(RacingConstants.colorFinalist);

                int i = 0;
                foreach (StaticRacerInfo info in finishers)
                {
                    i++;
                    tempSb.Append(RacingTools.SetLength(i, RacingConstants.numberWidth)).Append(' ');
                    tempSb.Append(RacingTools.SetLength(info.Name, RacingConstants.nameWidth));
                    if (MapSettings.TimedMode)
                        tempSb.Append(' ').Append(info.BestTime.ToString(RacingConstants.timerFormating));
                    tempSb.AppendLine();
                }

                tempSb.Length--;
                tempSb.Append(RacingConstants.colorWhite).AppendLine();
            }
            RacingTools.SendAPIMessage(RacingConstants.apiFinishers);
            finalString = tempSb.ToString();
        }

        public override string ToString ()
        {
            return finalString;
        }

        public IEnumerator<StaticRacerInfo> GetEnumerator ()
        {
            return finishers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return finishers.GetEnumerator();
        }
    }
}
