using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Hud
{
    public class VanillaRacingHud : RacingHud
    {
        private readonly Vector2 hudPosition = new Vector2(-0.125f, 0.05f);
        private const float fontSize = 0.7f;
        private const long stringId = 1708268562;

        private readonly List<IMyPlayer> players = new List<IMyPlayer>();


        public override void Broadcast()
        {
            if (RacingSession.Instance.Runticks % 10 != 0)
                return;

            // TODO: One ui string per line for color?
            players.Clear();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (IMyPlayer p in players)
                MyVisualScriptLogicProvider.CreateUIString(stringId, text.ToString(), hudPosition.X, hudPosition.Y, fontSize, RacingConstants.fontId, playerId: p.IdentityId);
        }
    }
}
