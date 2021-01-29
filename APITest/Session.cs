using System.Linq;
using avaness.RacingMod.API;
using Sandbox.ModAPI;
using VRage.Game.Components;

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
            if(racingApi != null)
                racingApi.Close();
        }

        public override void UpdateAfterSimulation ()
        {
            if (MyAPIGateway.Session?.Player == null)
                return;
            if (racingApi == null)
                racingApi = new RacingDisplayAPI();
            if (!racingApi.Enabled)
                return;
            if (MyAPIGateway.Input.IsAnyCtrlKeyPressed())
            {
                if (MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.J))
                    racingApi.JoinRace(MyAPIGateway.Session.Player);
                else if (MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.L))
                    racingApi.LeaveRace(MyAPIGateway.Session.Player);
            }

            MyAPIGateway.Utilities.ShowNotification($"There are {racingApi.Racers.Count()} racers in the race.", 16);
        }
    }
}