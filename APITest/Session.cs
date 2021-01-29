using System.Linq;
using System;
using avaness.RacingMod.API;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;

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
            {
                racingApi.OnPlayerJoined -= RacingApi_OnPlayerJoined;
                racingApi.OnPlayerStarted -= RacingApi_OnPlayerStarted;
                racingApi.OnPlayerLeft -= RacingApi_OnPlayerLeft;
                racingApi.OnPlayerFinished -= RacingApi_OnPlayerFinished;
                racingApi.Close();
            }
        }

        public override void UpdateAfterSimulation ()
        {
            if (MyAPIGateway.Session?.Player == null)
                return;
            if (racingApi == null)
                racingApi = new RacingDisplayAPI(OnEnabled);
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

        private void OnEnabled()
        {
            racingApi.OnPlayerJoined += RacingApi_OnPlayerJoined;
            racingApi.OnPlayerStarted += RacingApi_OnPlayerStarted;
            racingApi.OnPlayerLeft += RacingApi_OnPlayerLeft;
            racingApi.OnPlayerFinished += RacingApi_OnPlayerFinished;
        }

        private void RacingApi_OnPlayerFinished(IMyPlayer p)
        {
            MyAPIGateway.Utilities.ShowNotification($"{p.DisplayName} has finished the race.");
        }

        private void RacingApi_OnPlayerLeft(IMyPlayer p)
        {
            MyAPIGateway.Utilities.ShowNotification($"{p.DisplayName} has left the race.");
        }

        private void RacingApi_OnPlayerStarted(IMyPlayer p)
        {
            MyAPIGateway.Utilities.ShowNotification($"{p.DisplayName} has started the race.");
        }

        private void RacingApi_OnPlayerJoined(IMyPlayer p)
        {
            MyAPIGateway.Utilities.ShowNotification($"{p.DisplayName} has joined the race.");
        }
    }
}