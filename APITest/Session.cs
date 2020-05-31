using System;
using System.Linq;
using RacingMod.API;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace APITest
{
    // This object is always present, from the world load to world unload.
    // NOTE: all clients and server run mod scripts, keep that in mind.
    // The MyUpdateOrder arg determines what update overrides are actually called.
    // Remove any method that you don't need, none of them are required, they're only there to show what you can use.
    // Also remove all comments you've read to avoid the overload of comments that is this file.
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingDisplayAPITest : MySessionComponentBase
    {
        RacingDisplayAPI racingApi;

        public RacingDisplayAPITest ()
        {

        }

        public override void BeforeStart ()
        {

        }

        protected override void UnloadData ()
        {
            racingApi.OnFinishersModified -= RacingApi_OnFinishersModified;
            racingApi.OnRaceStarted -= RacingApi_OnRaceStarted;
            racingApi.OnTrackLapsModified -= RacingApi_OnTrackLapsModified;
        }

        public override void UpdateAfterSimulation ()
        {
            if (racingApi == null)
                racingApi = new RacingDisplayAPI(OnEnabled);
        }

        private void OnEnabled ()
        {
            racingApi.OnFinishersModified += RacingApi_OnFinishersModified;
            racingApi.OnRaceStarted += RacingApi_OnRaceStarted;
            racingApi.OnTrackLapsModified += RacingApi_OnTrackLapsModified;
        }

        private void RacingApi_OnTrackLapsModified (int laps)
        {
            MyAPIGateway.Utilities.ShowNotification($"There are now {laps} laps in the race.");
        }

        private void RacingApi_OnRaceStarted ()
        {
            MyAPIGateway.Utilities.ShowNotification("'/rcd start' was run.");
        }

        private void RacingApi_OnFinishersModified (MyTuple<ulong, TimeSpan> [] finishers)
        {
            MyAPIGateway.Utilities.ShowNotification($"There are now {finishers.Length} finishers.");
        }
    }
}