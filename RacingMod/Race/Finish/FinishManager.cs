using avaness.RacingMod.Racers;
using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace avaness.RacingMod.Race.Finish
{
    public class FinishList : IEnumerable<IFinisher>
    {
        private readonly StringBuilder tempSb = new StringBuilder();
        private string finalString;
        private RacingMapSettings MapSettings => RacingSession.Instance.MapSettings;
        private readonly List<IFinisher> finishers = new List<IFinisher>();
        private SerializableFinisher[] serializable;

        public event Action OnFinishersModified;

        public FinishList()
        {
            MapSettings.TimedModeChanged += TimedModeChanged;
        }

        public int Count => finishers.Count;

        public void Add(StaticRacerInfo info)
        {
            RemoveUpdate(info);
            finishers.Add(info);
            FinishersModifed();
        }

        public void Add(StaticRacerInfo info, int index)
        {
            int current = IndexOf(info);
            if (current > 0)
            {
                finishers.RemoveAt(current);
                if (current < index)
                    index--;
            }

            finishers.Insert(index, info);
            FinishersModifed();
        }

        private bool RemoveUpdate(StaticRacerInfo info)
        {
            int current = IndexOf(info);
            if (current >= 0)
            {
                finishers.RemoveAt(current);
                return true;
            }
            return false;
        }

        private int IndexOf(IFinisher finisher)
        {
            for(int i = 0; i < finishers.Count; i++)
            {
                IFinisher temp = finishers[i];
                if (temp.Id == finisher.Id)
                    return i;
            }
            return -1;
        }

        public bool Remove(StaticRacerInfo info)
        {
            if (RemoveUpdate(info))
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
            OnFinishersModified = null;
        }

        private void TimedModeChanged (bool timed)
        {
            FinishersModifed();
        }

        // Build the final racer text
        private void FinishersModifed ()
        {
            tempSb.Clear().Append(RacingConstants.colorFinalist);

            IEnumerable<IFinisher> temp;
            if (MapSettings.TimedMode)
                temp = new SortedSet<IFinisher>(finishers, new RacerBestTimeComparer());
            else
                temp = finishers;

            SerializableFinisher[] serializable = new SerializableFinisher[finishers.Count];

            if (finishers.Count > 0)
            {
                int i = 0;
                foreach (IFinisher info in temp)
                {
                    SerializableFinisher s = info as SerializableFinisher;
                    if (s == null)
                        s = new SerializableFinisher(info);
                    serializable[i] = s;

                    i++;
                    tempSb.Append(RacingTools.SetLength(i, RacingConstants.numberWidth)).Append(' ');
                    tempSb.Append(info.Name).Append(' ');
                    tempSb.Append(info.BestTime.ToString(RacingConstants.timerFormating)).AppendLine();
                }

                tempSb.Length--;
                tempSb.Append(RacingConstants.colorWhite).AppendLine();
            }
            finalString = tempSb.ToString();
            if(this.serializable != null)
                this.serializable = serializable;
            if (OnFinishersModified != null)
                OnFinishersModified.Invoke();
        }

        public override string ToString ()
        {
            return finalString;
        }

        public void SaveFile()
        {
            if(serializable != null)
            {
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(RacingConstants.finisherFile, typeof(FinishList));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(serializable));
                writer.Flush();
                writer.Close();
            }
        }

        public void LoadFile(Racers.RacerStorage racers)
        {
            serializable = new SerializableFinisher[0];
            try
            {
                if (RacingConstants.IsServer && MyAPIGateway.Utilities.FileExistsInWorldStorage(RacingConstants.finisherFile, typeof(FinishList)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(RacingConstants.finisherFile, typeof(FinishList));
                    string xmlText = reader.ReadToEnd();
                    reader.Close();
                    SerializableFinisher[] temp = MyAPIGateway.Utilities.SerializeFromXML<SerializableFinisher[]>(xmlText);
                    if (temp != null)
                    {
                        serializable = temp;
                        foreach(SerializableFinisher finisher in temp)
                        {
                            finisher.Name = RacingTools.SetLength(finisher.Name, RacingConstants.nameWidth);
                            finishers.Add(finisher);
                        }
                        racers.LoadData(serializable);
                        FinishersModifed();
                    }
                }
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, typeof(FinishList));
            }
        }

        public IEnumerator<IFinisher> GetEnumerator ()
        {
            return finishers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return finishers.GetEnumerator();
        }
    }
}
