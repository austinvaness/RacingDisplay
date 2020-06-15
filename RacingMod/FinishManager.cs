using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RacingMod
{
    public class FinishList : IEnumerable<StaticRacerInfo>
    {
        private readonly StringBuilder tempSb = new StringBuilder();
        private string finalString;
        private RacingMapSettings MapSettings => RacingSession.Instance.MapSettings;
        private readonly List<StaticRacerInfo> finishers = new List<StaticRacerInfo>();

        public FinishList()
        {
            MapSettings.TimedModeChanged += TimedModeChanged;
        }

        public int Count => finishers.Count;

        public void Add(StaticRacerInfo info)
        {
            finishers.Remove(info);
            finishers.Add(info);
            FinishersModifed();
        }

        public void Add(StaticRacerInfo info, int index)
        {
            int current = finishers.IndexOf(info);
            if(current > 0)
            {
                finishers.RemoveAt(current);
                if (current < index)
                    index--;
            }
            finishers.Insert(index, info);
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
            FinishersModifed();
        }

        // Build the final racer text
        private void FinishersModifed ()
        {
            tempSb.Clear().Append(RacingConstants.colorFinalist);

            IEnumerable<StaticRacerInfo> temp;
            if (MapSettings.TimedMode)
                temp = new SortedSet<StaticRacerInfo>(finishers, new RacerBestTimeComparer());
            else
                temp = finishers;

            if (finishers.Count > 0)
            {
                int i = 0;
                foreach (StaticRacerInfo info in temp)
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
