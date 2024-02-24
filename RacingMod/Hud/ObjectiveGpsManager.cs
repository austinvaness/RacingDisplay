using avaness.RacingMod.Paths;
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
    public class ObjectiveGpsManager
    {
        private readonly Dictionary<long, IMyGps> gpsList = new Dictionary<long, IMyGps>();
        private IMyGpsCollection gpsCollection;

        public void Init()
        {
            gpsCollection = MyAPIGateway.Session.GPS;

            // Remove existing gps by name
            if (RacingConstants.IsServer)
                MyVisualScriptLogicProvider.RemoveGPSForAll(RacingConstants.gateWaypointName);

        }

        public void RemoveGps(long playerId)
        {
            IMyGps gps;
            if (gpsList.TryGetValue(playerId, out gps))
            {
                gpsCollection.RemoveGps(playerId, gps);
                gpsList.Remove(playerId);
            }
            else
            {
                MyVisualScriptLogicProvider.RemoveGPS(RacingConstants.gateWaypointName, playerId);
            }
        }

        public void CreateGps(Vector3D position, long playerId)
        {
            IMyGps gps;
            if (gpsList.TryGetValue(playerId, out gps))
            {
                gps.Coords = position;
                gpsCollection.ModifyGps(playerId, gps);
            }
            else
            {
                MyVisualScriptLogicProvider.AddGPSObjective(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, position, RacingConstants.gateWaypointColor, 0, playerId);
                IMyGps result = gpsCollection.GetGpsList(playerId).Where(x => x.Name == RacingConstants.gateWaypointName).FirstOrDefault();
                if(result != null)
                    gpsList[playerId] = result;
            }
        }
    }
}
