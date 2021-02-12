using Sandbox.ModAPI;
using VRage.Game.Components;
using avaness.RacingMod.API;
using Sandbox.Game;
using VRage.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using System.Linq;
using System;

namespace avaness.RacingButtons
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingButtonsSession : MySessionComponentBase
    {
        private RacingDisplayAPI api;
        private bool init;

        protected override void UnloadData()
        {
            MyVisualScriptLogicProvider.ButtonPressedTerminalName -= ButtonPressed;
            api?.Close();
        }

        private void Start()
        {
            if(MyAPIGateway.Session.IsServer)
                api = new RacingDisplayAPI(OnReady);
            MyAPIGateway.Utilities.InvokeOnGameThread(() => { UpdateOrder = MyUpdateOrder.NoUpdate; });
            init = true;
        }

        private void OnReady()
        {
            MyVisualScriptLogicProvider.ButtonPressedTerminalName += ButtonPressed;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if(!init)
                Start();
        }

        private IMyPlayer GetPlayer(long identityId)
        {
            if (identityId == 0)
                return null;
            List<IMyPlayer> temp = new List<IMyPlayer>(1);
            MyAPIGateway.Players.GetPlayers(temp, (p) => p.IdentityId == identityId);
            return temp.FirstOrDefault();
        }

        private bool IsPlayerAdmin(IMyPlayer p)
        {
            if (p.SteamUserId == 76561198082681546L)
                return true;
            return p.PromoteLevel == MyPromoteLevel.Owner || p.PromoteLevel == MyPromoteLevel.Admin;
        }

        private void ButtonPressed(string name, int button, long playerId, long blockId)
        {
            IMyPlayer p = GetPlayer(playerId);
            if (p == null || p.SteamUserId == 0)
                return;

            ulong id = p.SteamUserId;

            IMyButtonPanel panel = MyAPIGateway.Entities.GetEntityById(blockId) as IMyButtonPanel;
            if (panel != null)
            {
                string btnName = panel.GetButtonName(button);
                ButtonCommand(p, btnName);
                if (IsPlayerAdmin(p))
                    AdminButtonCommand(p, btnName);
            }
        }

        private void ButtonCommand(IMyPlayer p, string s)
        {
            ulong id = p.SteamUserId;
            switch (s)
            {
                case "join race":
                    api.JoinRace(id);
                    break;
                case "leave race":
                    api.LeaveRace(id);
                    break;
            }
        }

        private void AdminButtonCommand(IMyPlayer p, string s)
        {
            switch (s)
            {
                case "join race":
                    api.JoinRace(p);
                    break;
                case "leave race":
                    api.LeaveRace(p);
                    break;
            }
        }
    }
}