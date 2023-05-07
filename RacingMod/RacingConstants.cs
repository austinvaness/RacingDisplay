using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRageMath;

namespace avaness.RacingMod
{
    public static class RacingConstants
    {
        public static bool IsServer => MyAPIGateway.Session.IsServer;
        public static bool IsDedicated => IsServer && MyAPIGateway.Utilities.IsDedicated;
        public static bool IsPlayer => !IsDedicated;

        // One second = 10,000,000 ticks
        public static TimeSpan oneTick = new TimeSpan((long)(MyEngineConstants.UPDATE_STEP_SIZE_IN_SECONDS * 10000000));

        public const long ModMessageId = 1708268562;

        public const byte packetMainId = 0;
        public const byte packetCmd = 1;
        public const byte packetSettings = 2;
        public const byte packetSettingsInit = 3;
        public const byte packetRec = 4;
        public const byte packetBeaconSettings = 5;
        public const byte packetAutoRec = 6;

        public const string mapFile = "RacingDisplayConfig.xml";
        public const string playerFile = "RacingDisplayPreferences.xml";
        public const string finisherFile = "RacingDisplayFinishers.xml";

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

        public const float finishCompatNum = 10000;
        public const int maxCheckpointSize = 500;
        public static Guid beaconStorage = new Guid("77E1A8DB-DC82-45A9-87F1-F1A0D86F24DB");

        public const string fontId = "FreeMono_Racing";
        public const string fontData = @"
<?xml version=""1.0"" encoding=""UTF-8"" ?>
<font
    xmlns = ""http://xna.microsoft.com/bitmapfont""

    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
	xsi:schemaLocation=""http://xna.microsoft.com/bitmapfont bitmapfont.xsd""
	
	name=""FontDataRacePA"" base=""32"" height=""40""
	face=""FreeMono_Racing"" size=""30"" style=""b""
	>
<bitmaps>
<bitmap id = ""0"" name=""FontDataRacePA-0.dds"" size=""1024x1024"" />
<bitmap id = ""1"" name=""FontDataRacePA-1.dds"" size=""1024x1024"" />
</bitmaps>
<glyphs>
<glyph ch = "" "" code=""0020"" bm=""0"" origin=""0,0"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = ""!"" code=""0021"" bm=""0"" origin=""14,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""&quot;"" code=""0022"" bm=""0"" origin=""53,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""#"" code=""0023"" bm=""0"" origin=""92,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""$"" code=""0024"" bm=""0"" origin=""131,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""%"" code=""0025"" bm=""0"" origin=""170,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""&amp;"" code=""0026"" bm=""0"" origin=""209,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""'"" code=""0027"" bm=""0"" origin=""248,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""("" code=""0028"" bm=""0"" origin=""287,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "")"" code=""0029"" bm=""0"" origin=""326,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""*"" code=""002a"" bm=""0"" origin=""365,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""+"" code=""002b"" bm=""0"" origin=""404,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "","" code=""002c"" bm=""0"" origin=""443,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""-"" code=""002d"" bm=""0"" origin=""482,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""."" code=""002e"" bm=""0"" origin=""521,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""/"" code=""002f"" bm=""0"" origin=""560,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""0"" code=""0030"" bm=""0"" origin=""599,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""1"" code=""0031"" bm=""0"" origin=""638,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""2"" code=""0032"" bm=""0"" origin=""677,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""3"" code=""0033"" bm=""0"" origin=""716,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""4"" code=""0034"" bm=""0"" origin=""755,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""5"" code=""0035"" bm=""0"" origin=""794,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""6"" code=""0036"" bm=""0"" origin=""833,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""7"" code=""0037"" bm=""0"" origin=""872,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""8"" code=""0038"" bm=""0"" origin=""911,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""9"" code=""0039"" bm=""0"" origin=""950,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "":"" code=""003a"" bm=""0"" origin=""0,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "";"" code=""003b"" bm=""0"" origin=""39,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""&lt;"" code=""003c"" bm=""0"" origin=""78,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""="" code=""003d"" bm=""0"" origin=""117,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""&gt;"" code=""003e"" bm=""0"" origin=""156,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""?"" code=""003f"" bm=""0"" origin=""195,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""@"" code=""0040"" bm=""0"" origin=""234,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""A"" code=""0041"" bm=""0"" origin=""273,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""B"" code=""0042"" bm=""0"" origin=""312,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""C"" code=""0043"" bm=""0"" origin=""351,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""D"" code=""0044"" bm=""0"" origin=""390,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""E"" code=""0045"" bm=""0"" origin=""429,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""F"" code=""0046"" bm=""0"" origin=""468,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""G"" code=""0047"" bm=""0"" origin=""507,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""H"" code=""0048"" bm=""0"" origin=""546,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""I"" code=""0049"" bm=""0"" origin=""585,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""J"" code=""004a"" bm=""0"" origin=""624,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""K"" code=""004b"" bm=""0"" origin=""663,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""L"" code=""004c"" bm=""0"" origin=""702,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""M"" code=""004d"" bm=""0"" origin=""741,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""N"" code=""004e"" bm=""0"" origin=""780,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""O"" code=""004f"" bm=""0"" origin=""819,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""P"" code=""0050"" bm=""0"" origin=""858,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Q"" code=""0051"" bm=""0"" origin=""897,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""R"" code=""0052"" bm=""0"" origin=""936,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""S"" code=""0053"" bm=""0"" origin=""975,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""T"" code=""0054"" bm=""0"" origin=""0,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""U"" code=""0055"" bm=""0"" origin=""39,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""V"" code=""0056"" bm=""0"" origin=""78,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""W"" code=""0057"" bm=""0"" origin=""117,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""X"" code=""0058"" bm=""0"" origin=""156,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Y"" code=""0059"" bm=""0"" origin=""195,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Z"" code=""005a"" bm=""0"" origin=""234,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""["" code=""005b"" bm=""0"" origin=""273,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""\"" code=""005c"" bm=""0"" origin=""312,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""]"" code=""005d"" bm=""0"" origin=""351,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""^"" code=""005e"" bm=""0"" origin=""390,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""_"" code=""005f"" bm=""0"" origin=""429,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""`"" code=""0060"" bm=""0"" origin=""468,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""a"" code=""0061"" bm=""0"" origin=""507,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""b"" code=""0062"" bm=""0"" origin=""546,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""c"" code=""0063"" bm=""0"" origin=""585,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""d"" code=""0064"" bm=""0"" origin=""624,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""e"" code=""0065"" bm=""0"" origin=""663,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""f"" code=""0066"" bm=""0"" origin=""702,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""g"" code=""0067"" bm=""0"" origin=""741,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""h"" code=""0068"" bm=""0"" origin=""780,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""i"" code=""0069"" bm=""0"" origin=""819,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""j"" code=""006a"" bm=""0"" origin=""858,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""k"" code=""006b"" bm=""0"" origin=""897,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""l"" code=""006c"" bm=""0"" origin=""936,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""m"" code=""006d"" bm=""0"" origin=""975,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""n"" code=""006e"" bm=""0"" origin=""0,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""o"" code=""006f"" bm=""0"" origin=""39,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""p"" code=""0070"" bm=""0"" origin=""78,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""q"" code=""0071"" bm=""0"" origin=""117,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""r"" code=""0072"" bm=""0"" origin=""156,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""s"" code=""0073"" bm=""0"" origin=""195,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""t"" code=""0074"" bm=""0"" origin=""234,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""u"" code=""0075"" bm=""0"" origin=""273,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""v"" code=""0076"" bm=""0"" origin=""312,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""w"" code=""0077"" bm=""0"" origin=""351,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""x"" code=""0078"" bm=""0"" origin=""390,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""y"" code=""0079"" bm=""0"" origin=""429,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""z"" code=""007a"" bm=""0"" origin=""468,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""{"" code=""007b"" bm=""0"" origin=""507,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""|"" code=""007c"" bm=""0"" origin=""546,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""}"" code=""007d"" bm=""0"" origin=""585,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""~"" code=""007e"" bm=""0"" origin=""624,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""00a0"" bm=""0"" origin=""663,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¡"" code=""00a1"" bm=""0"" origin=""702,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¢"" code=""00a2"" bm=""0"" origin=""741,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""£"" code=""00a3"" bm=""0"" origin=""780,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¤"" code=""00a4"" bm=""0"" origin=""819,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¥"" code=""00a5"" bm=""0"" origin=""858,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¦"" code=""00a6"" bm=""0"" origin=""897,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""§"" code=""00a7"" bm=""0"" origin=""936,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¨"" code=""00a8"" bm=""0"" origin=""975,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""©"" code=""00a9"" bm=""0"" origin=""0,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ª"" code=""00aa"" bm=""0"" origin=""39,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""«"" code=""00ab"" bm=""0"" origin=""78,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¬"" code=""00ac"" bm=""0"" origin=""117,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""­"" code=""00ad"" bm=""0"" origin=""156,192"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = ""®"" code=""00ae"" bm=""0"" origin=""170,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¯"" code=""00af"" bm=""0"" origin=""209,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""°"" code=""00b0"" bm=""0"" origin=""248,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""±"" code=""00b1"" bm=""0"" origin=""287,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""²"" code=""00b2"" bm=""0"" origin=""326,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""³"" code=""00b3"" bm=""0"" origin=""365,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""´"" code=""00b4"" bm=""0"" origin=""404,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""µ"" code=""00b5"" bm=""0"" origin=""443,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¶"" code=""00b6"" bm=""0"" origin=""482,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""·"" code=""00b7"" bm=""0"" origin=""521,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¸"" code=""00b8"" bm=""0"" origin=""560,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¹"" code=""00b9"" bm=""0"" origin=""599,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""º"" code=""00ba"" bm=""0"" origin=""638,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""»"" code=""00bb"" bm=""0"" origin=""677,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¼"" code=""00bc"" bm=""0"" origin=""716,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""½"" code=""00bd"" bm=""0"" origin=""755,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¾"" code=""00be"" bm=""0"" origin=""794,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""¿"" code=""00bf"" bm=""0"" origin=""833,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""À"" code=""00c0"" bm=""0"" origin=""872,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Á"" code=""00c1"" bm=""0"" origin=""911,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Â"" code=""00c2"" bm=""0"" origin=""950,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ã"" code=""00c3"" bm=""0"" origin=""0,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ä"" code=""00c4"" bm=""0"" origin=""39,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Å"" code=""00c5"" bm=""0"" origin=""78,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Æ"" code=""00c6"" bm=""0"" origin=""117,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ç"" code=""00c7"" bm=""0"" origin=""156,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""È"" code=""00c8"" bm=""0"" origin=""195,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""É"" code=""00c9"" bm=""0"" origin=""234,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ê"" code=""00ca"" bm=""0"" origin=""273,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ë"" code=""00cb"" bm=""0"" origin=""312,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ì"" code=""00cc"" bm=""0"" origin=""351,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Í"" code=""00cd"" bm=""0"" origin=""390,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Î"" code=""00ce"" bm=""0"" origin=""429,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ï"" code=""00cf"" bm=""0"" origin=""468,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ð"" code=""00d0"" bm=""0"" origin=""507,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ñ"" code=""00d1"" bm=""0"" origin=""546,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ò"" code=""00d2"" bm=""0"" origin=""585,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ó"" code=""00d3"" bm=""0"" origin=""624,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ô"" code=""00d4"" bm=""0"" origin=""663,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Õ"" code=""00d5"" bm=""0"" origin=""702,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ö"" code=""00d6"" bm=""0"" origin=""741,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""×"" code=""00d7"" bm=""0"" origin=""780,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ø"" code=""00d8"" bm=""0"" origin=""819,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ù"" code=""00d9"" bm=""0"" origin=""858,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ú"" code=""00da"" bm=""0"" origin=""897,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Û"" code=""00db"" bm=""0"" origin=""936,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ü"" code=""00dc"" bm=""0"" origin=""975,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ý"" code=""00dd"" bm=""0"" origin=""0,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Þ"" code=""00de"" bm=""0"" origin=""39,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ß"" code=""00df"" bm=""0"" origin=""78,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""à"" code=""00e0"" bm=""0"" origin=""117,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""á"" code=""00e1"" bm=""0"" origin=""156,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""â"" code=""00e2"" bm=""0"" origin=""195,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ã"" code=""00e3"" bm=""0"" origin=""234,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ä"" code=""00e4"" bm=""0"" origin=""273,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""å"" code=""00e5"" bm=""0"" origin=""312,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""æ"" code=""00e6"" bm=""0"" origin=""351,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ç"" code=""00e7"" bm=""0"" origin=""390,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""è"" code=""00e8"" bm=""0"" origin=""429,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""é"" code=""00e9"" bm=""0"" origin=""468,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ê"" code=""00ea"" bm=""0"" origin=""507,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ë"" code=""00eb"" bm=""0"" origin=""546,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ì"" code=""00ec"" bm=""0"" origin=""585,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""í"" code=""00ed"" bm=""0"" origin=""624,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""î"" code=""00ee"" bm=""0"" origin=""663,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ï"" code=""00ef"" bm=""0"" origin=""702,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ð"" code=""00f0"" bm=""0"" origin=""741,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ñ"" code=""00f1"" bm=""0"" origin=""780,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ò"" code=""00f2"" bm=""0"" origin=""819,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ó"" code=""00f3"" bm=""0"" origin=""858,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ô"" code=""00f4"" bm=""0"" origin=""897,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""õ"" code=""00f5"" bm=""0"" origin=""936,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ö"" code=""00f6"" bm=""0"" origin=""975,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""÷"" code=""00f7"" bm=""0"" origin=""0,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ø"" code=""00f8"" bm=""0"" origin=""39,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ù"" code=""00f9"" bm=""0"" origin=""78,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ú"" code=""00fa"" bm=""0"" origin=""117,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""û"" code=""00fb"" bm=""0"" origin=""156,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ü"" code=""00fc"" bm=""0"" origin=""195,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ý"" code=""00fd"" bm=""0"" origin=""234,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""þ"" code=""00fe"" bm=""0"" origin=""273,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ÿ"" code=""00ff"" bm=""0"" origin=""312,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ā"" code=""0100"" bm=""0"" origin=""351,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ā"" code=""0101"" bm=""0"" origin=""390,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ă"" code=""0102"" bm=""0"" origin=""429,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ă"" code=""0103"" bm=""0"" origin=""468,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ą"" code=""0104"" bm=""0"" origin=""507,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ą"" code=""0105"" bm=""0"" origin=""546,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ć"" code=""0106"" bm=""0"" origin=""585,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ć"" code=""0107"" bm=""0"" origin=""624,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĉ"" code=""0108"" bm=""0"" origin=""663,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĉ"" code=""0109"" bm=""0"" origin=""702,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ċ"" code=""010a"" bm=""0"" origin=""741,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ċ"" code=""010b"" bm=""0"" origin=""780,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Č"" code=""010c"" bm=""0"" origin=""819,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""č"" code=""010d"" bm=""0"" origin=""858,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ď"" code=""010e"" bm=""0"" origin=""897,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ď"" code=""010f"" bm=""0"" origin=""936,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Đ"" code=""0110"" bm=""0"" origin=""975,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""đ"" code=""0111"" bm=""0"" origin=""0,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ē"" code=""0112"" bm=""0"" origin=""39,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ē"" code=""0113"" bm=""0"" origin=""78,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĕ"" code=""0114"" bm=""0"" origin=""117,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĕ"" code=""0115"" bm=""0"" origin=""156,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ė"" code=""0116"" bm=""0"" origin=""195,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ė"" code=""0117"" bm=""0"" origin=""234,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ę"" code=""0118"" bm=""0"" origin=""273,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ę"" code=""0119"" bm=""0"" origin=""312,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ě"" code=""011a"" bm=""0"" origin=""351,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ě"" code=""011b"" bm=""0"" origin=""390,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĝ"" code=""011c"" bm=""0"" origin=""429,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĝ"" code=""011d"" bm=""0"" origin=""468,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ğ"" code=""011e"" bm=""0"" origin=""507,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ğ"" code=""011f"" bm=""0"" origin=""546,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ġ"" code=""0120"" bm=""0"" origin=""585,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ġ"" code=""0121"" bm=""0"" origin=""624,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ģ"" code=""0122"" bm=""0"" origin=""663,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ģ"" code=""0123"" bm=""0"" origin=""702,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĥ"" code=""0124"" bm=""0"" origin=""741,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĥ"" code=""0125"" bm=""0"" origin=""780,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ħ"" code=""0126"" bm=""0"" origin=""819,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ħ"" code=""0127"" bm=""0"" origin=""858,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĩ"" code=""0128"" bm=""0"" origin=""897,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĩ"" code=""0129"" bm=""0"" origin=""936,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ī"" code=""012a"" bm=""0"" origin=""975,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ī"" code=""012b"" bm=""0"" origin=""0,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĭ"" code=""012c"" bm=""0"" origin=""39,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĭ"" code=""012d"" bm=""0"" origin=""78,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Į"" code=""012e"" bm=""0"" origin=""117,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""į"" code=""012f"" bm=""0"" origin=""156,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""İ"" code=""0130"" bm=""0"" origin=""195,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ı"" code=""0131"" bm=""0"" origin=""234,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĳ"" code=""0132"" bm=""0"" origin=""273,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĳ"" code=""0133"" bm=""0"" origin=""312,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĵ"" code=""0134"" bm=""0"" origin=""351,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĵ"" code=""0135"" bm=""0"" origin=""390,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ķ"" code=""0136"" bm=""0"" origin=""429,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ķ"" code=""0137"" bm=""0"" origin=""468,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĸ"" code=""0138"" bm=""0"" origin=""507,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ĺ"" code=""0139"" bm=""0"" origin=""546,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ĺ"" code=""013a"" bm=""0"" origin=""585,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ļ"" code=""013b"" bm=""0"" origin=""624,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ļ"" code=""013c"" bm=""0"" origin=""663,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ľ"" code=""013d"" bm=""0"" origin=""702,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ľ"" code=""013e"" bm=""0"" origin=""741,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŀ"" code=""013f"" bm=""0"" origin=""780,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŀ"" code=""0140"" bm=""0"" origin=""819,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ł"" code=""0141"" bm=""0"" origin=""858,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ł"" code=""0142"" bm=""0"" origin=""897,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ń"" code=""0143"" bm=""0"" origin=""936,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ń"" code=""0144"" bm=""0"" origin=""975,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ņ"" code=""0145"" bm=""0"" origin=""0,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ņ"" code=""0146"" bm=""0"" origin=""39,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ň"" code=""0147"" bm=""0"" origin=""78,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ň"" code=""0148"" bm=""0"" origin=""117,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŉ"" code=""0149"" bm=""0"" origin=""156,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŋ"" code=""014a"" bm=""0"" origin=""195,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŋ"" code=""014b"" bm=""0"" origin=""234,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ō"" code=""014c"" bm=""0"" origin=""273,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ō"" code=""014d"" bm=""0"" origin=""312,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŏ"" code=""014e"" bm=""0"" origin=""351,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŏ"" code=""014f"" bm=""0"" origin=""390,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ő"" code=""0150"" bm=""0"" origin=""429,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ő"" code=""0151"" bm=""0"" origin=""468,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Œ"" code=""0152"" bm=""0"" origin=""507,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""œ"" code=""0153"" bm=""0"" origin=""546,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŕ"" code=""0154"" bm=""0"" origin=""585,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŕ"" code=""0155"" bm=""0"" origin=""624,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŗ"" code=""0156"" bm=""0"" origin=""663,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŗ"" code=""0157"" bm=""0"" origin=""702,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ř"" code=""0158"" bm=""0"" origin=""741,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ř"" code=""0159"" bm=""0"" origin=""780,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ś"" code=""015a"" bm=""0"" origin=""819,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ś"" code=""015b"" bm=""0"" origin=""858,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŝ"" code=""015c"" bm=""0"" origin=""897,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŝ"" code=""015d"" bm=""0"" origin=""936,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ş"" code=""015e"" bm=""0"" origin=""975,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ş"" code=""015f"" bm=""0"" origin=""0,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Š"" code=""0160"" bm=""0"" origin=""39,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""š"" code=""0161"" bm=""0"" origin=""78,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ţ"" code=""0162"" bm=""0"" origin=""117,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ţ"" code=""0163"" bm=""0"" origin=""156,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ť"" code=""0164"" bm=""0"" origin=""195,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ť"" code=""0165"" bm=""0"" origin=""234,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŧ"" code=""0166"" bm=""0"" origin=""273,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŧ"" code=""0167"" bm=""0"" origin=""312,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ũ"" code=""0168"" bm=""0"" origin=""351,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ũ"" code=""0169"" bm=""0"" origin=""390,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ū"" code=""016a"" bm=""0"" origin=""429,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ū"" code=""016b"" bm=""0"" origin=""468,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŭ"" code=""016c"" bm=""0"" origin=""507,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŭ"" code=""016d"" bm=""0"" origin=""546,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ů"" code=""016e"" bm=""0"" origin=""585,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ů"" code=""016f"" bm=""0"" origin=""624,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ű"" code=""0170"" bm=""0"" origin=""663,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ű"" code=""0171"" bm=""0"" origin=""702,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ų"" code=""0172"" bm=""0"" origin=""741,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ų"" code=""0173"" bm=""0"" origin=""780,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŵ"" code=""0174"" bm=""0"" origin=""819,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŵ"" code=""0175"" bm=""0"" origin=""858,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ŷ"" code=""0176"" bm=""0"" origin=""897,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ŷ"" code=""0177"" bm=""0"" origin=""936,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ÿ"" code=""0178"" bm=""0"" origin=""975,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ź"" code=""0179"" bm=""0"" origin=""0,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ź"" code=""017a"" bm=""0"" origin=""39,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ż"" code=""017b"" bm=""0"" origin=""78,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ż"" code=""017c"" bm=""0"" origin=""117,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ž"" code=""017d"" bm=""0"" origin=""156,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ž"" code=""017e"" bm=""0"" origin=""195,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ſ"" code=""017f"" bm=""0"" origin=""234,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƀ"" code=""0180"" bm=""0"" origin=""273,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɓ"" code=""0181"" bm=""0"" origin=""312,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƃ"" code=""0182"" bm=""0"" origin=""351,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƃ"" code=""0183"" bm=""0"" origin=""390,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƅ"" code=""0184"" bm=""0"" origin=""429,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƅ"" code=""0185"" bm=""0"" origin=""468,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɔ"" code=""0186"" bm=""0"" origin=""507,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƈ"" code=""0187"" bm=""0"" origin=""546,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƈ"" code=""0188"" bm=""0"" origin=""585,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɖ"" code=""0189"" bm=""0"" origin=""624,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɗ"" code=""018a"" bm=""0"" origin=""663,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƌ"" code=""018b"" bm=""0"" origin=""702,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƌ"" code=""018c"" bm=""0"" origin=""741,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƍ"" code=""018d"" bm=""0"" origin=""780,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǝ"" code=""018e"" bm=""0"" origin=""819,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ə"" code=""018f"" bm=""0"" origin=""858,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɛ"" code=""0190"" bm=""0"" origin=""897,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƒ"" code=""0191"" bm=""0"" origin=""936,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƒ"" code=""0192"" bm=""0"" origin=""975,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɠ"" code=""0193"" bm=""0"" origin=""0,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɣ"" code=""0194"" bm=""0"" origin=""39,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƕ"" code=""0195"" bm=""0"" origin=""78,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɩ"" code=""0196"" bm=""0"" origin=""117,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɨ"" code=""0197"" bm=""0"" origin=""156,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƙ"" code=""0198"" bm=""0"" origin=""195,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƙ"" code=""0199"" bm=""0"" origin=""234,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƚ"" code=""019a"" bm=""0"" origin=""273,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƛ"" code=""019b"" bm=""0"" origin=""312,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɯ"" code=""019c"" bm=""0"" origin=""351,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɲ"" code=""019d"" bm=""0"" origin=""390,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƞ"" code=""019e"" bm=""0"" origin=""429,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ɵ"" code=""019f"" bm=""0"" origin=""468,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ơ"" code=""01a0"" bm=""0"" origin=""507,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ơ"" code=""01a1"" bm=""0"" origin=""546,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƣ"" code=""01a2"" bm=""0"" origin=""585,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƣ"" code=""01a3"" bm=""0"" origin=""624,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƥ"" code=""01a4"" bm=""0"" origin=""663,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƥ"" code=""01a5"" bm=""0"" origin=""702,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ʀ"" code=""01a6"" bm=""0"" origin=""741,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƨ"" code=""01a7"" bm=""0"" origin=""780,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƨ"" code=""01a8"" bm=""0"" origin=""819,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ʃ"" code=""01a9"" bm=""0"" origin=""858,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƪ"" code=""01aa"" bm=""0"" origin=""897,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƫ"" code=""01ab"" bm=""0"" origin=""936,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƭ"" code=""01ac"" bm=""0"" origin=""975,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƭ"" code=""01ad"" bm=""0"" origin=""0,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ʈ"" code=""01ae"" bm=""0"" origin=""39,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ư"" code=""01af"" bm=""0"" origin=""78,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ư"" code=""01b0"" bm=""0"" origin=""117,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ʊ"" code=""01b1"" bm=""0"" origin=""156,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ʋ"" code=""01b2"" bm=""0"" origin=""195,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƴ"" code=""01b3"" bm=""0"" origin=""234,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƴ"" code=""01b4"" bm=""0"" origin=""273,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƶ"" code=""01b5"" bm=""0"" origin=""312,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƶ"" code=""01b6"" bm=""0"" origin=""351,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ʒ"" code=""01b7"" bm=""0"" origin=""390,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƹ"" code=""01b8"" bm=""0"" origin=""429,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƹ"" code=""01b9"" bm=""0"" origin=""468,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƺ"" code=""01ba"" bm=""0"" origin=""507,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƻ"" code=""01bb"" bm=""0"" origin=""546,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƽ"" code=""01bc"" bm=""0"" origin=""585,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƽ"" code=""01bd"" bm=""0"" origin=""624,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƾ"" code=""01be"" bm=""0"" origin=""663,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ƿ"" code=""01bf"" bm=""0"" origin=""702,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǀ"" code=""01c0"" bm=""0"" origin=""741,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǁ"" code=""01c1"" bm=""0"" origin=""780,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǂ"" code=""01c2"" bm=""0"" origin=""819,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǃ"" code=""01c3"" bm=""0"" origin=""858,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǆ"" code=""01c4"" bm=""0"" origin=""897,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǅ"" code=""01c5"" bm=""0"" origin=""936,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǆ"" code=""01c6"" bm=""0"" origin=""975,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǉ"" code=""01c7"" bm=""0"" origin=""0,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǈ"" code=""01c8"" bm=""0"" origin=""39,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǉ"" code=""01c9"" bm=""0"" origin=""78,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǌ"" code=""01ca"" bm=""0"" origin=""117,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǋ"" code=""01cb"" bm=""0"" origin=""156,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǌ"" code=""01cc"" bm=""0"" origin=""195,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǎ"" code=""01cd"" bm=""0"" origin=""234,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǎ"" code=""01ce"" bm=""0"" origin=""273,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǐ"" code=""01cf"" bm=""0"" origin=""312,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǐ"" code=""01d0"" bm=""0"" origin=""351,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǒ"" code=""01d1"" bm=""0"" origin=""390,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǒ"" code=""01d2"" bm=""0"" origin=""429,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǔ"" code=""01d3"" bm=""0"" origin=""468,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǔ"" code=""01d4"" bm=""0"" origin=""507,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǖ"" code=""01d5"" bm=""0"" origin=""546,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǖ"" code=""01d6"" bm=""0"" origin=""585,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǘ"" code=""01d7"" bm=""0"" origin=""624,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǘ"" code=""01d8"" bm=""0"" origin=""663,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǚ"" code=""01d9"" bm=""0"" origin=""702,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǚ"" code=""01da"" bm=""0"" origin=""741,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǜ"" code=""01db"" bm=""0"" origin=""780,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǜ"" code=""01dc"" bm=""0"" origin=""819,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǝ"" code=""01dd"" bm=""0"" origin=""858,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǟ"" code=""01de"" bm=""0"" origin=""897,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǟ"" code=""01df"" bm=""0"" origin=""936,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǡ"" code=""01e0"" bm=""0"" origin=""975,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǡ"" code=""01e1"" bm=""0"" origin=""0,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǣ"" code=""01e2"" bm=""0"" origin=""39,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǣ"" code=""01e3"" bm=""0"" origin=""78,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǥ"" code=""01e4"" bm=""0"" origin=""117,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǥ"" code=""01e5"" bm=""0"" origin=""156,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǧ"" code=""01e6"" bm=""0"" origin=""195,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǧ"" code=""01e7"" bm=""0"" origin=""234,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǩ"" code=""01e8"" bm=""0"" origin=""273,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǩ"" code=""01e9"" bm=""0"" origin=""312,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǫ"" code=""01ea"" bm=""0"" origin=""351,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǫ"" code=""01eb"" bm=""0"" origin=""390,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǭ"" code=""01ec"" bm=""0"" origin=""429,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǭ"" code=""01ed"" bm=""0"" origin=""468,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǯ"" code=""01ee"" bm=""0"" origin=""507,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǯ"" code=""01ef"" bm=""0"" origin=""546,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǰ"" code=""01f0"" bm=""0"" origin=""585,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǳ"" code=""01f1"" bm=""0"" origin=""624,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǲ"" code=""01f2"" bm=""0"" origin=""663,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǳ"" code=""01f3"" bm=""0"" origin=""702,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǵ"" code=""01f4"" bm=""0"" origin=""741,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǵ"" code=""01f5"" bm=""0"" origin=""780,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƕ"" code=""01f6"" bm=""0"" origin=""819,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƿ"" code=""01f7"" bm=""0"" origin=""858,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǹ"" code=""01f8"" bm=""0"" origin=""897,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǹ"" code=""01f9"" bm=""0"" origin=""936,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǻ"" code=""01fa"" bm=""0"" origin=""975,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǻ"" code=""01fb"" bm=""0"" origin=""0,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǽ"" code=""01fc"" bm=""0"" origin=""39,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǽ"" code=""01fd"" bm=""0"" origin=""78,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ǿ"" code=""01fe"" bm=""0"" origin=""117,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ǿ"" code=""01ff"" bm=""0"" origin=""156,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȁ"" code=""0200"" bm=""0"" origin=""195,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȁ"" code=""0201"" bm=""0"" origin=""234,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȃ"" code=""0202"" bm=""0"" origin=""273,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȃ"" code=""0203"" bm=""0"" origin=""312,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȅ"" code=""0204"" bm=""0"" origin=""351,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȅ"" code=""0205"" bm=""0"" origin=""390,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȇ"" code=""0206"" bm=""0"" origin=""429,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȇ"" code=""0207"" bm=""0"" origin=""468,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȉ"" code=""0208"" bm=""0"" origin=""507,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȉ"" code=""0209"" bm=""0"" origin=""546,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȋ"" code=""020a"" bm=""0"" origin=""585,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȋ"" code=""020b"" bm=""0"" origin=""624,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȍ"" code=""020c"" bm=""0"" origin=""663,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȍ"" code=""020d"" bm=""0"" origin=""702,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȏ"" code=""020e"" bm=""0"" origin=""741,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȏ"" code=""020f"" bm=""0"" origin=""780,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȑ"" code=""0210"" bm=""0"" origin=""819,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȑ"" code=""0211"" bm=""0"" origin=""858,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȓ"" code=""0212"" bm=""0"" origin=""897,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȓ"" code=""0213"" bm=""0"" origin=""936,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȕ"" code=""0214"" bm=""0"" origin=""975,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȕ"" code=""0215"" bm=""0"" origin=""0,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȗ"" code=""0216"" bm=""0"" origin=""39,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȗ"" code=""0217"" bm=""0"" origin=""78,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ș"" code=""0218"" bm=""0"" origin=""117,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ș"" code=""0219"" bm=""0"" origin=""156,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ț"" code=""021a"" bm=""0"" origin=""195,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ț"" code=""021b"" bm=""0"" origin=""234,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȝ"" code=""021c"" bm=""0"" origin=""273,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȝ"" code=""021d"" bm=""0"" origin=""312,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȟ"" code=""021e"" bm=""0"" origin=""351,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȟ"" code=""021f"" bm=""0"" origin=""390,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ƞ"" code=""0220"" bm=""0"" origin=""429,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȥ"" code=""0224"" bm=""0"" origin=""468,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȥ"" code=""0225"" bm=""0"" origin=""507,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȧ"" code=""0226"" bm=""0"" origin=""546,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȧ"" code=""0227"" bm=""0"" origin=""585,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȩ"" code=""0228"" bm=""0"" origin=""624,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȩ"" code=""0229"" bm=""0"" origin=""663,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȫ"" code=""022a"" bm=""0"" origin=""702,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȫ"" code=""022b"" bm=""0"" origin=""741,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȭ"" code=""022c"" bm=""0"" origin=""780,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȭ"" code=""022d"" bm=""0"" origin=""819,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȯ"" code=""022e"" bm=""0"" origin=""858,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȯ"" code=""022f"" bm=""0"" origin=""897,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȱ"" code=""0230"" bm=""0"" origin=""936,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȱ"" code=""0231"" bm=""0"" origin=""975,864"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ȳ"" code=""0232"" bm=""0"" origin=""0,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȳ"" code=""0233"" bm=""0"" origin=""39,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ȷ"" code=""0237"" bm=""0"" origin=""78,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɐ"" code=""0250"" bm=""0"" origin=""117,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɑ"" code=""0251"" bm=""0"" origin=""156,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɒ"" code=""0252"" bm=""0"" origin=""195,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɓ"" code=""0253"" bm=""0"" origin=""234,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɔ"" code=""0254"" bm=""0"" origin=""273,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɖ"" code=""0256"" bm=""0"" origin=""312,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɗ"" code=""0257"" bm=""0"" origin=""351,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɘ"" code=""0258"" bm=""0"" origin=""390,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ə"" code=""0259"" bm=""0"" origin=""429,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɛ"" code=""025b"" bm=""0"" origin=""468,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɜ"" code=""025c"" bm=""0"" origin=""507,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɟ"" code=""025f"" bm=""0"" origin=""546,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɠ"" code=""0260"" bm=""0"" origin=""585,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɡ"" code=""0261"" bm=""0"" origin=""624,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɥ"" code=""0265"" bm=""0"" origin=""663,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɦ"" code=""0266"" bm=""0"" origin=""702,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɧ"" code=""0267"" bm=""0"" origin=""741,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɨ"" code=""0268"" bm=""0"" origin=""780,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɩ"" code=""0269"" bm=""0"" origin=""819,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɪ"" code=""026a"" bm=""0"" origin=""858,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɫ"" code=""026b"" bm=""0"" origin=""897,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɭ"" code=""026d"" bm=""0"" origin=""936,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɯ"" code=""026f"" bm=""0"" origin=""975,912"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɰ"" code=""0270"" bm=""0"" origin=""0,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɱ"" code=""0271"" bm=""0"" origin=""39,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɲ"" code=""0272"" bm=""0"" origin=""78,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɳ"" code=""0273"" bm=""0"" origin=""117,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɴ"" code=""0274"" bm=""0"" origin=""156,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɵ"" code=""0275"" bm=""0"" origin=""195,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɹ"" code=""0279"" bm=""0"" origin=""234,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɺ"" code=""027a"" bm=""0"" origin=""273,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɻ"" code=""027b"" bm=""0"" origin=""312,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɼ"" code=""027c"" bm=""0"" origin=""351,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɽ"" code=""027d"" bm=""0"" origin=""390,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɾ"" code=""027e"" bm=""0"" origin=""429,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ɿ"" code=""027f"" bm=""0"" origin=""468,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʀ"" code=""0280"" bm=""0"" origin=""507,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʁ"" code=""0281"" bm=""0"" origin=""546,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʂ"" code=""0282"" bm=""0"" origin=""585,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʃ"" code=""0283"" bm=""0"" origin=""624,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʄ"" code=""0284"" bm=""0"" origin=""663,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʅ"" code=""0285"" bm=""0"" origin=""702,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʇ"" code=""0287"" bm=""0"" origin=""741,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʈ"" code=""0288"" bm=""0"" origin=""780,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʉ"" code=""0289"" bm=""0"" origin=""819,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʌ"" code=""028c"" bm=""0"" origin=""858,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʍ"" code=""028d"" bm=""0"" origin=""897,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʎ"" code=""028e"" bm=""0"" origin=""936,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʐ"" code=""0290"" bm=""0"" origin=""975,960"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʑ"" code=""0291"" bm=""1"" origin=""0,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʒ"" code=""0292"" bm=""1"" origin=""39,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʓ"" code=""0293"" bm=""1"" origin=""78,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʔ"" code=""0294"" bm=""1"" origin=""117,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʕ"" code=""0295"" bm=""1"" origin=""156,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʖ"" code=""0296"" bm=""1"" origin=""195,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʗ"" code=""0297"" bm=""1"" origin=""234,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʘ"" code=""0298"" bm=""1"" origin=""273,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʙ"" code=""0299"" bm=""1"" origin=""312,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʚ"" code=""029a"" bm=""1"" origin=""351,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʛ"" code=""029b"" bm=""1"" origin=""390,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʜ"" code=""029c"" bm=""1"" origin=""429,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʝ"" code=""029d"" bm=""1"" origin=""468,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʞ"" code=""029e"" bm=""1"" origin=""507,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʟ"" code=""029f"" bm=""1"" origin=""546,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʠ"" code=""02a0"" bm=""1"" origin=""585,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʡ"" code=""02a1"" bm=""1"" origin=""624,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʢ"" code=""02a2"" bm=""1"" origin=""663,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʰ"" code=""02b0"" bm=""1"" origin=""702,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʱ"" code=""02b1"" bm=""1"" origin=""741,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʲ"" code=""02b2"" bm=""1"" origin=""780,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʳ"" code=""02b3"" bm=""1"" origin=""819,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʴ"" code=""02b4"" bm=""1"" origin=""858,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʵ"" code=""02b5"" bm=""1"" origin=""897,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʶ"" code=""02b6"" bm=""1"" origin=""936,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʷ"" code=""02b7"" bm=""1"" origin=""975,0"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʸ"" code=""02b8"" bm=""1"" origin=""0,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʹ"" code=""02b9"" bm=""1"" origin=""39,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʺ"" code=""02ba"" bm=""1"" origin=""78,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʻ"" code=""02bb"" bm=""1"" origin=""117,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʼ"" code=""02bc"" bm=""1"" origin=""156,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʽ"" code=""02bd"" bm=""1"" origin=""195,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʾ"" code=""02be"" bm=""1"" origin=""234,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ʿ"" code=""02bf"" bm=""1"" origin=""273,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˀ"" code=""02c0"" bm=""1"" origin=""312,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˁ"" code=""02c1"" bm=""1"" origin=""351,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˂"" code=""02c2"" bm=""1"" origin=""390,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˃"" code=""02c3"" bm=""1"" origin=""429,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˄"" code=""02c4"" bm=""1"" origin=""468,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˅"" code=""02c5"" bm=""1"" origin=""507,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˆ"" code=""02c6"" bm=""1"" origin=""546,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˇ"" code=""02c7"" bm=""1"" origin=""585,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˈ"" code=""02c8"" bm=""1"" origin=""624,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˉ"" code=""02c9"" bm=""1"" origin=""663,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˊ"" code=""02ca"" bm=""1"" origin=""702,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˋ"" code=""02cb"" bm=""1"" origin=""741,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˌ"" code=""02cc"" bm=""1"" origin=""780,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˍ"" code=""02cd"" bm=""1"" origin=""819,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˎ"" code=""02ce"" bm=""1"" origin=""858,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˏ"" code=""02cf"" bm=""1"" origin=""897,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ː"" code=""02d0"" bm=""1"" origin=""936,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˑ"" code=""02d1"" bm=""1"" origin=""975,48"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˒"" code=""02d2"" bm=""1"" origin=""0,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˓"" code=""02d3"" bm=""1"" origin=""39,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˔"" code=""02d4"" bm=""1"" origin=""78,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˕"" code=""02d5"" bm=""1"" origin=""117,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˖"" code=""02d6"" bm=""1"" origin=""156,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˗"" code=""02d7"" bm=""1"" origin=""195,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˘"" code=""02d8"" bm=""1"" origin=""234,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˙"" code=""02d9"" bm=""1"" origin=""273,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˚"" code=""02da"" bm=""1"" origin=""312,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˛"" code=""02db"" bm=""1"" origin=""351,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˜"" code=""02dc"" bm=""1"" origin=""390,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˝"" code=""02dd"" bm=""1"" origin=""429,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˞"" code=""02de"" bm=""1"" origin=""468,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˟"" code=""02df"" bm=""1"" origin=""507,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˠ"" code=""02e0"" bm=""1"" origin=""546,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˡ"" code=""02e1"" bm=""1"" origin=""585,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˢ"" code=""02e2"" bm=""1"" origin=""624,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˣ"" code=""02e3"" bm=""1"" origin=""663,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˤ"" code=""02e4"" bm=""1"" origin=""702,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˥"" code=""02e5"" bm=""1"" origin=""741,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˦"" code=""02e6"" bm=""1"" origin=""780,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˧"" code=""02e7"" bm=""1"" origin=""819,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˨"" code=""02e8"" bm=""1"" origin=""858,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˩"" code=""02e9"" bm=""1"" origin=""897,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˪"" code=""02ea"" bm=""1"" origin=""936,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˫"" code=""02eb"" bm=""1"" origin=""975,96"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˬ"" code=""02ec"" bm=""1"" origin=""0,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˭"" code=""02ed"" bm=""1"" origin=""39,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ˮ"" code=""02ee"" bm=""1"" origin=""78,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˯"" code=""02ef"" bm=""1"" origin=""117,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˰"" code=""02f0"" bm=""1"" origin=""156,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˱"" code=""02f1"" bm=""1"" origin=""195,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˲"" code=""02f2"" bm=""1"" origin=""234,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˳"" code=""02f3"" bm=""1"" origin=""273,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˴"" code=""02f4"" bm=""1"" origin=""312,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˵"" code=""02f5"" bm=""1"" origin=""351,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˶"" code=""02f6"" bm=""1"" origin=""390,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˷"" code=""02f7"" bm=""1"" origin=""429,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˸"" code=""02f8"" bm=""1"" origin=""468,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˹"" code=""02f9"" bm=""1"" origin=""507,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˺"" code=""02fa"" bm=""1"" origin=""546,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˻"" code=""02fb"" bm=""1"" origin=""585,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˼"" code=""02fc"" bm=""1"" origin=""624,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˽"" code=""02fd"" bm=""1"" origin=""663,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˾"" code=""02fe"" bm=""1"" origin=""702,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""˿"" code=""02ff"" bm=""1"" origin=""741,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ѐ"" code=""0400"" bm=""1"" origin=""780,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ё"" code=""0401"" bm=""1"" origin=""819,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ђ"" code=""0402"" bm=""1"" origin=""858,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ѓ"" code=""0403"" bm=""1"" origin=""897,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Є"" code=""0404"" bm=""1"" origin=""936,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ѕ"" code=""0405"" bm=""1"" origin=""975,144"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""І"" code=""0406"" bm=""1"" origin=""0,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ї"" code=""0407"" bm=""1"" origin=""39,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ј"" code=""0408"" bm=""1"" origin=""78,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Љ"" code=""0409"" bm=""1"" origin=""117,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Њ"" code=""040a"" bm=""1"" origin=""156,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ћ"" code=""040b"" bm=""1"" origin=""195,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ќ"" code=""040c"" bm=""1"" origin=""234,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ѝ"" code=""040d"" bm=""1"" origin=""273,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ў"" code=""040e"" bm=""1"" origin=""312,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Џ"" code=""040f"" bm=""1"" origin=""351,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""А"" code=""0410"" bm=""1"" origin=""390,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Б"" code=""0411"" bm=""1"" origin=""429,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""В"" code=""0412"" bm=""1"" origin=""468,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Г"" code=""0413"" bm=""1"" origin=""507,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Д"" code=""0414"" bm=""1"" origin=""546,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Е"" code=""0415"" bm=""1"" origin=""585,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ж"" code=""0416"" bm=""1"" origin=""624,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""З"" code=""0417"" bm=""1"" origin=""663,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""И"" code=""0418"" bm=""1"" origin=""702,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Й"" code=""0419"" bm=""1"" origin=""741,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""К"" code=""041a"" bm=""1"" origin=""780,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Л"" code=""041b"" bm=""1"" origin=""819,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""М"" code=""041c"" bm=""1"" origin=""858,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Н"" code=""041d"" bm=""1"" origin=""897,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""О"" code=""041e"" bm=""1"" origin=""936,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""П"" code=""041f"" bm=""1"" origin=""975,192"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Р"" code=""0420"" bm=""1"" origin=""0,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""С"" code=""0421"" bm=""1"" origin=""39,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Т"" code=""0422"" bm=""1"" origin=""78,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""У"" code=""0423"" bm=""1"" origin=""117,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ф"" code=""0424"" bm=""1"" origin=""156,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Х"" code=""0425"" bm=""1"" origin=""195,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ц"" code=""0426"" bm=""1"" origin=""234,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ч"" code=""0427"" bm=""1"" origin=""273,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ш"" code=""0428"" bm=""1"" origin=""312,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Щ"" code=""0429"" bm=""1"" origin=""351,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ъ"" code=""042a"" bm=""1"" origin=""390,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ы"" code=""042b"" bm=""1"" origin=""429,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ь"" code=""042c"" bm=""1"" origin=""468,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Э"" code=""042d"" bm=""1"" origin=""507,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ю"" code=""042e"" bm=""1"" origin=""546,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Я"" code=""042f"" bm=""1"" origin=""585,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""а"" code=""0430"" bm=""1"" origin=""624,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""б"" code=""0431"" bm=""1"" origin=""663,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""в"" code=""0432"" bm=""1"" origin=""702,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""г"" code=""0433"" bm=""1"" origin=""741,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""д"" code=""0434"" bm=""1"" origin=""780,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""е"" code=""0435"" bm=""1"" origin=""819,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ж"" code=""0436"" bm=""1"" origin=""858,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""з"" code=""0437"" bm=""1"" origin=""897,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""и"" code=""0438"" bm=""1"" origin=""936,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""й"" code=""0439"" bm=""1"" origin=""975,240"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""к"" code=""043a"" bm=""1"" origin=""0,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""л"" code=""043b"" bm=""1"" origin=""39,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""м"" code=""043c"" bm=""1"" origin=""78,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""н"" code=""043d"" bm=""1"" origin=""117,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""о"" code=""043e"" bm=""1"" origin=""156,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""п"" code=""043f"" bm=""1"" origin=""195,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""р"" code=""0440"" bm=""1"" origin=""234,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""с"" code=""0441"" bm=""1"" origin=""273,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""т"" code=""0442"" bm=""1"" origin=""312,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""у"" code=""0443"" bm=""1"" origin=""351,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ф"" code=""0444"" bm=""1"" origin=""390,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""х"" code=""0445"" bm=""1"" origin=""429,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ц"" code=""0446"" bm=""1"" origin=""468,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ч"" code=""0447"" bm=""1"" origin=""507,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ш"" code=""0448"" bm=""1"" origin=""546,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""щ"" code=""0449"" bm=""1"" origin=""585,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ъ"" code=""044a"" bm=""1"" origin=""624,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ы"" code=""044b"" bm=""1"" origin=""663,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ь"" code=""044c"" bm=""1"" origin=""702,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""э"" code=""044d"" bm=""1"" origin=""741,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ю"" code=""044e"" bm=""1"" origin=""780,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""я"" code=""044f"" bm=""1"" origin=""819,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ѐ"" code=""0450"" bm=""1"" origin=""858,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ё"" code=""0451"" bm=""1"" origin=""897,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ђ"" code=""0452"" bm=""1"" origin=""936,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ѓ"" code=""0453"" bm=""1"" origin=""975,288"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""є"" code=""0454"" bm=""1"" origin=""0,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ѕ"" code=""0455"" bm=""1"" origin=""39,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""і"" code=""0456"" bm=""1"" origin=""78,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ї"" code=""0457"" bm=""1"" origin=""117,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ј"" code=""0458"" bm=""1"" origin=""156,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""љ"" code=""0459"" bm=""1"" origin=""195,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""њ"" code=""045a"" bm=""1"" origin=""234,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ћ"" code=""045b"" bm=""1"" origin=""273,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ќ"" code=""045c"" bm=""1"" origin=""312,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ѝ"" code=""045d"" bm=""1"" origin=""351,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ў"" code=""045e"" bm=""1"" origin=""390,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""џ"" code=""045f"" bm=""1"" origin=""429,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ѱ"" code=""0470"" bm=""1"" origin=""468,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ѱ"" code=""0471"" bm=""1"" origin=""507,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ѳ"" code=""0472"" bm=""1"" origin=""546,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""҄"" code=""0484"" bm=""1"" origin=""585,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""҅"" code=""0485"" bm=""1"" origin=""624,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""҆"" code=""0486"" bm=""1"" origin=""663,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҋ"" code=""048a"" bm=""1"" origin=""702,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҋ"" code=""048b"" bm=""1"" origin=""741,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҍ"" code=""048c"" bm=""1"" origin=""780,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҍ"" code=""048d"" bm=""1"" origin=""819,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҏ"" code=""048e"" bm=""1"" origin=""858,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҏ"" code=""048f"" bm=""1"" origin=""897,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ґ"" code=""0490"" bm=""1"" origin=""936,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ґ"" code=""0491"" bm=""1"" origin=""975,336"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ғ"" code=""0492"" bm=""1"" origin=""0,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ғ"" code=""0493"" bm=""1"" origin=""39,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҕ"" code=""0494"" bm=""1"" origin=""78,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҕ"" code=""0495"" bm=""1"" origin=""117,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Җ"" code=""0496"" bm=""1"" origin=""156,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""җ"" code=""0497"" bm=""1"" origin=""195,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҙ"" code=""0498"" bm=""1"" origin=""234,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҙ"" code=""0499"" bm=""1"" origin=""273,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Қ"" code=""049a"" bm=""1"" origin=""312,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""қ"" code=""049b"" bm=""1"" origin=""351,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҝ"" code=""049c"" bm=""1"" origin=""390,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҝ"" code=""049d"" bm=""1"" origin=""429,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҟ"" code=""049e"" bm=""1"" origin=""468,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҟ"" code=""049f"" bm=""1"" origin=""507,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҡ"" code=""04a0"" bm=""1"" origin=""546,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҡ"" code=""04a1"" bm=""1"" origin=""585,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ң"" code=""04a2"" bm=""1"" origin=""624,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ң"" code=""04a3"" bm=""1"" origin=""663,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҥ"" code=""04a4"" bm=""1"" origin=""702,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҥ"" code=""04a5"" bm=""1"" origin=""741,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҧ"" code=""04a6"" bm=""1"" origin=""780,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҧ"" code=""04a7"" bm=""1"" origin=""819,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҩ"" code=""04a8"" bm=""1"" origin=""858,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҩ"" code=""04a9"" bm=""1"" origin=""897,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҫ"" code=""04aa"" bm=""1"" origin=""936,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҫ"" code=""04ab"" bm=""1"" origin=""975,384"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҭ"" code=""04ac"" bm=""1"" origin=""0,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҭ"" code=""04ad"" bm=""1"" origin=""39,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ү"" code=""04ae"" bm=""1"" origin=""78,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ү"" code=""04af"" bm=""1"" origin=""117,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ұ"" code=""04b0"" bm=""1"" origin=""156,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ұ"" code=""04b1"" bm=""1"" origin=""195,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҳ"" code=""04b2"" bm=""1"" origin=""234,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҳ"" code=""04b3"" bm=""1"" origin=""273,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҵ"" code=""04b4"" bm=""1"" origin=""312,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҵ"" code=""04b5"" bm=""1"" origin=""351,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҷ"" code=""04b6"" bm=""1"" origin=""390,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҷ"" code=""04b7"" bm=""1"" origin=""429,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҹ"" code=""04b8"" bm=""1"" origin=""468,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҹ"" code=""04b9"" bm=""1"" origin=""507,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Һ"" code=""04ba"" bm=""1"" origin=""546,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""һ"" code=""04bb"" bm=""1"" origin=""585,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҽ"" code=""04bc"" bm=""1"" origin=""624,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҽ"" code=""04bd"" bm=""1"" origin=""663,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ҿ"" code=""04be"" bm=""1"" origin=""702,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ҿ"" code=""04bf"" bm=""1"" origin=""741,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӏ"" code=""04c0"" bm=""1"" origin=""780,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӂ"" code=""04c1"" bm=""1"" origin=""819,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӂ"" code=""04c2"" bm=""1"" origin=""858,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӄ"" code=""04c3"" bm=""1"" origin=""897,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӄ"" code=""04c4"" bm=""1"" origin=""936,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӆ"" code=""04c5"" bm=""1"" origin=""975,432"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӆ"" code=""04c6"" bm=""1"" origin=""0,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӈ"" code=""04c7"" bm=""1"" origin=""39,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӈ"" code=""04c8"" bm=""1"" origin=""78,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӊ"" code=""04c9"" bm=""1"" origin=""117,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӊ"" code=""04ca"" bm=""1"" origin=""156,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӌ"" code=""04cb"" bm=""1"" origin=""195,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӌ"" code=""04cc"" bm=""1"" origin=""234,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӎ"" code=""04cd"" bm=""1"" origin=""273,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӎ"" code=""04ce"" bm=""1"" origin=""312,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӏ"" code=""04cf"" bm=""1"" origin=""351,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӑ"" code=""04d0"" bm=""1"" origin=""390,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӑ"" code=""04d1"" bm=""1"" origin=""429,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӓ"" code=""04d2"" bm=""1"" origin=""468,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӓ"" code=""04d3"" bm=""1"" origin=""507,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӕ"" code=""04d4"" bm=""1"" origin=""546,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӕ"" code=""04d5"" bm=""1"" origin=""585,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӗ"" code=""04d6"" bm=""1"" origin=""624,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӗ"" code=""04d7"" bm=""1"" origin=""663,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ә"" code=""04d8"" bm=""1"" origin=""702,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ә"" code=""04d9"" bm=""1"" origin=""741,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӛ"" code=""04da"" bm=""1"" origin=""780,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӛ"" code=""04db"" bm=""1"" origin=""819,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӝ"" code=""04dc"" bm=""1"" origin=""858,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӝ"" code=""04dd"" bm=""1"" origin=""897,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӟ"" code=""04de"" bm=""1"" origin=""936,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӟ"" code=""04df"" bm=""1"" origin=""975,480"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӡ"" code=""04e0"" bm=""1"" origin=""0,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӡ"" code=""04e1"" bm=""1"" origin=""39,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӣ"" code=""04e2"" bm=""1"" origin=""78,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӣ"" code=""04e3"" bm=""1"" origin=""117,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӥ"" code=""04e4"" bm=""1"" origin=""156,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӥ"" code=""04e5"" bm=""1"" origin=""195,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӧ"" code=""04e6"" bm=""1"" origin=""234,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӧ"" code=""04e7"" bm=""1"" origin=""273,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ө"" code=""04e8"" bm=""1"" origin=""312,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ө"" code=""04e9"" bm=""1"" origin=""351,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӫ"" code=""04ea"" bm=""1"" origin=""390,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӫ"" code=""04eb"" bm=""1"" origin=""429,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӭ"" code=""04ec"" bm=""1"" origin=""468,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӭ"" code=""04ed"" bm=""1"" origin=""507,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӯ"" code=""04ee"" bm=""1"" origin=""546,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӯ"" code=""04ef"" bm=""1"" origin=""585,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӱ"" code=""04f0"" bm=""1"" origin=""624,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӱ"" code=""04f1"" bm=""1"" origin=""663,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӳ"" code=""04f2"" bm=""1"" origin=""702,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӳ"" code=""04f3"" bm=""1"" origin=""741,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӵ"" code=""04f4"" bm=""1"" origin=""780,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӵ"" code=""04f5"" bm=""1"" origin=""819,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӷ"" code=""04f6"" bm=""1"" origin=""858,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӷ"" code=""04f7"" bm=""1"" origin=""897,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""Ӹ"" code=""04f8"" bm=""1"" origin=""936,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ӹ"" code=""04f9"" bm=""1"" origin=""975,528"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2000"" bm=""1"" origin=""0,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2001"" bm=""1"" origin=""39,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2002"" bm=""1"" origin=""78,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2003"" bm=""1"" origin=""92,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2004"" bm=""1"" origin=""106,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2005"" bm=""1"" origin=""145,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2006"" bm=""1"" origin=""184,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2007"" bm=""1"" origin=""223,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2008"" bm=""1"" origin=""262,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""2009"" bm=""1"" origin=""301,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""200a"" bm=""1"" origin=""315,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""​"" code=""200b"" bm=""1"" origin=""354,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‌"" code=""200c"" bm=""1"" origin=""368,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‍"" code=""200d"" bm=""1"" origin=""382,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‎"" code=""200e"" bm=""1"" origin=""396,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‏"" code=""200f"" bm=""1"" origin=""410,576"" size=""14x8"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‐"" code=""2010"" bm=""1"" origin=""424,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‑"" code=""2011"" bm=""1"" origin=""463,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‒"" code=""2012"" bm=""1"" origin=""502,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""–"" code=""2013"" bm=""1"" origin=""541,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""—"" code=""2014"" bm=""1"" origin=""580,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""―"" code=""2015"" bm=""1"" origin=""619,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‖"" code=""2016"" bm=""1"" origin=""658,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‗"" code=""2017"" bm=""1"" origin=""697,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‘"" code=""2018"" bm=""1"" origin=""736,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""’"" code=""2019"" bm=""1"" origin=""775,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‚"" code=""201a"" bm=""1"" origin=""814,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‛"" code=""201b"" bm=""1"" origin=""853,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""“"" code=""201c"" bm=""1"" origin=""892,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""”"" code=""201d"" bm=""1"" origin=""931,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""„"" code=""201e"" bm=""1"" origin=""970,576"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‟"" code=""201f"" bm=""1"" origin=""0,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""†"" code=""2020"" bm=""1"" origin=""39,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‡"" code=""2021"" bm=""1"" origin=""78,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""•"" code=""2022"" bm=""1"" origin=""117,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‣"" code=""2023"" bm=""1"" origin=""156,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""․"" code=""2024"" bm=""1"" origin=""195,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‥"" code=""2025"" bm=""1"" origin=""234,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""…"" code=""2026"" bm=""1"" origin=""273,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‧"" code=""2027"" bm=""1"" origin=""312,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""
"" code=""2028"" bm=""1"" origin=""351,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""
"" code=""2029"" bm=""1"" origin=""390,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‪"" code=""202a"" bm=""1"" origin=""429,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‫"" code=""202b"" bm=""1"" origin=""468,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‬"" code=""202c"" bm=""1"" origin=""507,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‭"" code=""202d"" bm=""1"" origin=""546,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‮"" code=""202e"" bm=""1"" origin=""585,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""202f"" bm=""1"" origin=""624,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‰"" code=""2030"" bm=""1"" origin=""663,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‱"" code=""2031"" bm=""1"" origin=""702,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""′"" code=""2032"" bm=""1"" origin=""741,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""″"" code=""2033"" bm=""1"" origin=""780,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‴"" code=""2034"" bm=""1"" origin=""819,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‵"" code=""2035"" bm=""1"" origin=""858,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‶"" code=""2036"" bm=""1"" origin=""897,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‷"" code=""2037"" bm=""1"" origin=""936,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‸"" code=""2038"" bm=""1"" origin=""975,624"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‹"" code=""2039"" bm=""1"" origin=""0,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""›"" code=""203a"" bm=""1"" origin=""39,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""※"" code=""203b"" bm=""1"" origin=""78,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‼"" code=""203c"" bm=""1"" origin=""117,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‽"" code=""203d"" bm=""1"" origin=""156,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‾"" code=""203e"" bm=""1"" origin=""195,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""‿"" code=""203f"" bm=""1"" origin=""234,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁀"" code=""2040"" bm=""1"" origin=""273,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁁"" code=""2041"" bm=""1"" origin=""312,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁂"" code=""2042"" bm=""1"" origin=""351,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁃"" code=""2043"" bm=""1"" origin=""390,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁅"" code=""2045"" bm=""1"" origin=""429,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁆"" code=""2046"" bm=""1"" origin=""468,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁇"" code=""2047"" bm=""1"" origin=""507,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁈"" code=""2048"" bm=""1"" origin=""546,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁉"" code=""2049"" bm=""1"" origin=""585,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁊"" code=""204a"" bm=""1"" origin=""624,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁋"" code=""204b"" bm=""1"" origin=""663,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁌"" code=""204c"" bm=""1"" origin=""702,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁍"" code=""204d"" bm=""1"" origin=""741,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁎"" code=""204e"" bm=""1"" origin=""780,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁏"" code=""204f"" bm=""1"" origin=""819,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁐"" code=""2050"" bm=""1"" origin=""858,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁑"" code=""2051"" bm=""1"" origin=""897,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁒"" code=""2052"" bm=""1"" origin=""936,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁓"" code=""2053"" bm=""1"" origin=""975,672"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁔"" code=""2054"" bm=""1"" origin=""0,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁕"" code=""2055"" bm=""1"" origin=""39,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁖"" code=""2056"" bm=""1"" origin=""78,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁗"" code=""2057"" bm=""1"" origin=""117,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁘"" code=""2058"" bm=""1"" origin=""156,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁙"" code=""2059"" bm=""1"" origin=""195,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁚"" code=""205a"" bm=""1"" origin=""234,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁛"" code=""205b"" bm=""1"" origin=""273,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁜"" code=""205c"" bm=""1"" origin=""312,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁝"" code=""205d"" bm=""1"" origin=""351,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁞"" code=""205e"" bm=""1"" origin=""390,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = "" "" code=""205f"" bm=""1"" origin=""429,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁠"" code=""2060"" bm=""1"" origin=""468,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁡"" code=""2061"" bm=""1"" origin=""507,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁢"" code=""2062"" bm=""1"" origin=""546,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁣"" code=""2063"" bm=""1"" origin=""585,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁤"" code=""2064"" bm=""1"" origin=""624,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁰"" code=""2070"" bm=""1"" origin=""663,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ⁱ"" code=""2071"" bm=""1"" origin=""702,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁴"" code=""2074"" bm=""1"" origin=""741,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁵"" code=""2075"" bm=""1"" origin=""780,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁶"" code=""2076"" bm=""1"" origin=""819,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁷"" code=""2077"" bm=""1"" origin=""858,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁸"" code=""2078"" bm=""1"" origin=""897,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁹"" code=""2079"" bm=""1"" origin=""936,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁺"" code=""207a"" bm=""1"" origin=""975,720"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁻"" code=""207b"" bm=""1"" origin=""0,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁼"" code=""207c"" bm=""1"" origin=""39,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁽"" code=""207d"" bm=""1"" origin=""78,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""⁾"" code=""207e"" bm=""1"" origin=""117,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ⁿ"" code=""207f"" bm=""1"" origin=""156,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₀"" code=""2080"" bm=""1"" origin=""195,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₁"" code=""2081"" bm=""1"" origin=""234,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₂"" code=""2082"" bm=""1"" origin=""273,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₃"" code=""2083"" bm=""1"" origin=""312,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₄"" code=""2084"" bm=""1"" origin=""351,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₅"" code=""2085"" bm=""1"" origin=""390,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₆"" code=""2086"" bm=""1"" origin=""429,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₇"" code=""2087"" bm=""1"" origin=""468,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₈"" code=""2088"" bm=""1"" origin=""507,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₉"" code=""2089"" bm=""1"" origin=""546,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₊"" code=""208a"" bm=""1"" origin=""585,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₋"" code=""208b"" bm=""1"" origin=""624,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₌"" code=""208c"" bm=""1"" origin=""663,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₍"" code=""208d"" bm=""1"" origin=""702,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₎"" code=""208e"" bm=""1"" origin=""741,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ₐ"" code=""2090"" bm=""1"" origin=""780,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ₑ"" code=""2091"" bm=""1"" origin=""819,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ₒ"" code=""2092"" bm=""1"" origin=""858,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ₓ"" code=""2093"" bm=""1"" origin=""897,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""ₔ"" code=""2094"" bm=""1"" origin=""936,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₣"" code=""20a3"" bm=""1"" origin=""975,768"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₤"" code=""20a4"" bm=""1"" origin=""0,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₥"" code=""20a5"" bm=""1"" origin=""39,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₦"" code=""20a6"" bm=""1"" origin=""78,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₩"" code=""20a9"" bm=""1"" origin=""117,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₪"" code=""20aa"" bm=""1"" origin=""156,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₫"" code=""20ab"" bm=""1"" origin=""195,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""€"" code=""20ac"" bm=""1"" origin=""234,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₭"" code=""20ad"" bm=""1"" origin=""273,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₮"" code=""20ae"" bm=""1"" origin=""312,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₱"" code=""20b1"" bm=""1"" origin=""351,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₲"" code=""20b2"" bm=""1"" origin=""390,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₳"" code=""20b3"" bm=""1"" origin=""429,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₴"" code=""20b4"" bm=""1"" origin=""468,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₵"" code=""20b5"" bm=""1"" origin=""507,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₸"" code=""20b8"" bm=""1"" origin=""546,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""₹"" code=""20b9"" bm=""1"" origin=""585,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""™"" code=""2122"" bm=""1"" origin=""624,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""−"" code=""2212"" bm=""1"" origin=""663,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""∙"" code=""2219"" bm=""1"" origin=""702,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = ""□"" code=""25a1"" bm=""1"" origin=""741,816"" size=""39x48"" aw=""24"" lsb=""-7"" />
<glyph ch = """" code=""e001"" bm=""1"" origin=""780,816"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e002"" bm=""1"" origin=""833,816"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e003"" bm=""1"" origin=""886,816"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e004"" bm=""1"" origin=""939,816"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e005"" bm=""1"" origin=""0,868"" size=""53x52"" aw=""41"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e006"" bm=""1"" origin=""53,868"" size=""53x52"" aw=""41"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e007"" bm=""1"" origin=""106,868"" size=""53x52"" aw=""32"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e008"" bm=""1"" origin=""159,868"" size=""53x52"" aw=""32"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e009"" bm=""1"" origin=""212,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e00a"" bm=""1"" origin=""265,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e00b"" bm=""1"" origin=""318,868"" size=""53x52"" aw=""34"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e00c"" bm=""1"" origin=""371,868"" size=""53x52"" aw=""34"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e00d"" bm=""1"" origin=""424,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e00e"" bm=""1"" origin=""477,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e00f"" bm=""1"" origin=""530,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e010"" bm=""1"" origin=""583,868"" size=""53x52"" aw=""41"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e011"" bm=""1"" origin=""636,868"" size=""53x52"" aw=""32"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e012"" bm=""1"" origin=""689,868"" size=""53x52"" aw=""41"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e013"" bm=""1"" origin=""742,868"" size=""53x52"" aw=""32"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e014"" bm=""1"" origin=""795,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e015"" bm=""1"" origin=""848,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e016"" bm=""1"" origin=""901,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e017"" bm=""1"" origin=""954,868"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e018"" bm=""1"" origin=""0,920"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e019"" bm=""1"" origin=""53,920"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e020"" bm=""1"" origin=""106,920"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
<glyph ch = """" code=""e021"" bm=""1"" origin=""159,920"" size=""53x52"" aw=""40"" lsb=""-7"" forcewhite=""true"" />
</glyphs>
</font>
";
    }
}
