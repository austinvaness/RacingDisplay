using System;
using System.Linq;
using System.Text;
using avaness.RacingMod.API;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace APITest
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingDisplayAPITest : MySessionComponentBase
    {
        RacingDisplayAPI racingApi;

        public RacingDisplayAPITest ()
        {

        }

        protected override void UnloadData ()
        {
            racingApi.OnFinishersModified -= RacingApi_OnFinishersModified;
            racingApi?.Close();
        }

        public override void UpdateAfterSimulation ()
        {
            if (MyAPIGateway.Session?.Player == null)
                return;
            if (racingApi == null)
                racingApi = new RacingDisplayAPI(OnEnabled);
            if (!racingApi.Enabled)
                return;
            if(MyAPIGateway.Input.IsAnyCtrlKeyPressed() && MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.J))
                racingApi.JoinRace(MyAPIGateway.Session.Player);
            MyAPIGateway.Utilities.ShowNotification($"There are {racingApi.Racers.Length} racers in the race.", 16);
        }

        private void OnEnabled ()
        {
            racingApi.OnFinishersModified += RacingApi_OnFinishersModified;
        }

        private void RacingApi_OnFinishersModified (MyTuple<ulong, TimeSpan> [] finishers)
        {
            MyAPIGateway.Utilities.ShowNotification($"There are now {finishers.Length} finishers.");
        }
    }
}