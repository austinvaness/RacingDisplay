using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.ModAPI;

namespace RacingMod.API
{
    class RacingDisplayAPI
    {
        const long MessageId = 1708268562;

        /// <summary>
        /// True when the API has connected to the Racing Display mod.
        /// Will always be false on clients.
        /// </summary>
        public bool Enabled { get; private set; } = false;
        private Action onEnabled;

        /// <summary>
        /// Call Close() when done to unregister message handlers. 
        /// Check Enabled to see if it is communicating with the Racing Display mod. 
        /// Can not be used on clients, only the server.
        /// </summary>
        /// <param name="onEnabled">Called once the API has connected to the Racing Display mod.</param>
        public RacingDisplayAPI (Action onEnabled = null)
        {
            this.onEnabled = onEnabled;
            MyAPIGateway.Utilities.RegisterMessageHandler(MessageId, RecieveData);
        }

        /// <summary>
        /// Call this method to cleanup the mod once done.
        /// </summary>
        public void Close ()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(MessageId, RecieveData);
            OnRaceStarted = null;
            OnFinishersModified = null;
            OnTrackLapsModified = null;
            onEnabled = null;
        }

        private void RecieveData (object obj)
        {
            if (!Enabled && obj is MyTuple<Func<ulong []>, Func<MyTuple<ulong, TimeSpan> []>, Func<ulong []>, Func<MyTuple<ulong, double, TimeSpan, int> []>, Func<double>, Func<int>>)
            {
                // Initialization
                var funcs = (MyTuple<Func<ulong []>, Func<MyTuple<ulong, TimeSpan> []>, Func<ulong []>, Func<MyTuple<ulong, double, TimeSpan, int> []>, Func<double>, Func<int>>)obj;
                finishers = funcs.Item1;
                finisherInfo = funcs.Item2;
                racers = funcs.Item3;
                racerInfo = funcs.Item4;
                trackLength = funcs.Item5;
                trackLaps = funcs.Item6;
                Enabled = true;
                if(onEnabled != null)
                {
                    onEnabled.Invoke();
                    onEnabled = null;
                }
            }
            else if(Enabled && obj is int)
            {
                // Events
                int i = (int)obj;
                if (i == 0)
                {
                    if(OnRaceStarted != null)
                        OnRaceStarted.Invoke();
                }
                else if(i == 1)
                {
                    if (OnFinishersModified != null)
                        OnFinishersModified.Invoke(finisherInfo.Invoke());
                }
                else if(i == 2)
                {
                    if (OnTrackLapsModified != null)
                        OnTrackLapsModified.Invoke(trackLaps.Invoke());
                }
            }
        }

        private Func<ulong []> finishers;
        /// <summary>
        /// Returns a list of players that have finished the race, in order by rank.
        /// </summary>
        public ulong [] Finishers
        {
            get
            {
                if (Enabled)
                    return finishers();
                return null;
            }
        }

        private Func<MyTuple<ulong, TimeSpan> []> finisherInfo;
        /// <summary>
        /// Returns a list of players that have finished the race, in order by rank.
        /// Player information includes id and the current best time.
        /// </summary>
        public MyTuple<ulong, TimeSpan> [] FinisherData
        {
            get
            {
                if (Enabled)
                    return finisherInfo();
                return null;
            }
        }

        private Func<ulong []> racers;
        /// <summary>
        /// Returns a list of players that are currently in the race, in order by rank.
        /// </summary>
        public ulong [] Racers
        {
            get
            {
                if (Enabled)
                    return racers();
                return null;
            }
        }

        private Func<MyTuple<ulong, double, TimeSpan, int> []> racerInfo;
        /// <summary>
        /// Returns a list of players that are currently in the race, in order by rank.
        /// Player information includes id, current distance from start, current elapsed time, and the number of completed laps.
        /// </summary>
        public MyTuple<ulong, double, TimeSpan, int> [] RacerData
        {
            get
            {
                if (Enabled)
                    return racerInfo();
                return null;
            }
        }

        private Func<double> trackLength;
        /// <summary>
        /// Returns the distance of one lap around the track.
        /// </summary>
        public double TrackLength
        {
            get
            {
                if (Enabled)
                    return trackLength();
                return 0;
            }
        }

        private Func<int> trackLaps;
        /// <summary>
        /// Returns the current number of laps needed to complete the race.
        /// </summary>
        public int TrackLaps
        {
            get
            {
                if (Enabled)
                    return trackLaps();
                return 0;
            }
        }

        /// <summary>
        /// This event will be called when the display of finishers is modified.
        /// Make sure to unsubscribe from the event when you are done with it!
        /// </summary>
        public event Action<MyTuple<ulong, TimeSpan> []> OnFinishersModified;


        /// <summary>
        /// This event will be called when "/rcd start" is entered in chat.
        /// Make sure to unsubscribe from the event when you are done with it!
        /// </summary>
        public event Action OnRaceStarted;

        /// <summary>
        /// This event will be called when the number of laps to complete the race has changed.
        /// Make sure to unsubscribe from the event when you are done with it!
        /// </summary>
        public event Action<int> OnTrackLapsModified;
    }
}