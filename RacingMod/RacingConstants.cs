using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace RacingMod
{
    public static class RacingConstants
    {
        public const ushort packetSettings = 45565;
        public const ushort packetSettingsInit = 45566;
        public const ushort packetMainId = 1337;
        public const ushort packetCmd = 1339;
        public const ushort packetSpecRequest = 1357;
        public const ushort packetSpecResponse = 1358;

        public const string mapFile = "RacingDisplayConfig.xml";
        public const string playerFile = "RacingDisplayPreferences.xml";

        public const string timerFormating = "mm\\:ss\\:ff";

        public const int defaultMsgMs = 4000;

        public const int numberWidth = 2;
        public const int distWidth = 10;
        public const int nameWidth = 20;

        public const double moveThreshold2 = 16; // Squared minimum velocity
        public const int rankUpTime = 90; // Number of ticks to display color

        // Colors
        public const string headerColor = "<color=127,127,127>";
        public const string colorWhite = "<color=white>";
        public const string colorStationary = "<color=255,124,124>";
        public const string colorRankUp = "<color=124,255,154>";
        public const string colorFinalist = "<color=140,255,255>";

        public static Color gateWaypointColor = new Color(0, 255, 255); // nodes and cleared checkpoints
        public const string gateWaypointName = "Waypoint";
        public const string gateWaypointDescription = "The next waypoint to guide you through the race.";

    }
}
