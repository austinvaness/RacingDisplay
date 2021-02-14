using Sandbox.ModAPI;
using VRage.Game.Components;
using avaness.RacingMod.API;
using Sandbox.Game;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Utils;

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
            MyLog.Default.WriteLineAndConsole("Racing Display Add-on Started: Button Panels");
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

        private void ButtonPressed(string name, int button, long playerId, long blockId)
        {
            IMyPlayer p = GetPlayer(playerId);
            if (p == null)
                return;

            IMyButtonPanel panel = MyAPIGateway.Entities.GetEntityById(blockId) as IMyButtonPanel;
            if (panel != null)
                ButtonCommand(p, panel.GetButtonName(button).ToLowerInvariant());
        }

        private void ButtonCommand(IMyPlayer p, string s)
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