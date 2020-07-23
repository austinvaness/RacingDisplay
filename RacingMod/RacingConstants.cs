using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRageMath;

namespace avaness.RacingMod
{
    public static class RacingConstants
    {
        public static bool IsServer => MyAPIGateway.Session.IsServer || MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE;
        public static bool IsDedicated => IsServer && MyAPIGateway.Utilities.IsDedicated;
        public static bool IsPlayer => !IsDedicated;

        // One second = 10,000,000 ticks
        public static TimeSpan oneTick = new TimeSpan((long)(MyEngineConstants.UPDATE_STEP_SIZE_IN_SECONDS * 10000000));

        public const long ModMessageId = 1708268562;

        public const ushort packetSettings = 45565;
        public const ushort packetSettingsInit = 45566;
        public const ushort packetMainId = 1337;
        public const ushort packetCmd = 1339;
        public const ushort packetSpecRequest = 1357;
        public const ushort packetSpecResponse = 1358;
        public const ushort packetRec = 1338;

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

        public static Color ghostWaypointColor = new Color(0, 0, 255);
        public static string ghostId => "BestTime" + MyAPIGateway.Session.Player.SteamUserId;
        public static string ghostName = "Best Time";
        public static string ghostDescription = "The path of your best time.";
        public static Color gateWaypointColor = new Color(0, 255, 255);
        public const string gateWaypointName = "Waypoint";
        public const string gateWaypointDescription = "The next waypoint to guide you through the race.";

        public const int apiRaceStarted = 0;
        public const int apiFinishers = 1;
        public const int apiLaps = 2;

        public const string finishCompatNum = "10000";

    }
}
