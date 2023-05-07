using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace avaness.RacingMod
{
    public static class RacingTools
    {
        public static IMyCubeBlock GetCockpit (IMyPlayer p)
        {
            return p?.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
        }

        public static IMyPlayer GetPlayer (ulong steamId)
        {
            if (steamId == 0)
                return null;
            List<IMyPlayer> temp = new List<IMyPlayer>(1);
            MyAPIGateway.Players.GetPlayers(temp, (p) => p.SteamUserId == steamId);
            return temp.FirstOrDefault();
        }

        public static IMyPlayer GetPlayer (string name)
        {
            name = name.ToLower();
            List<IMyPlayer> temp = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(temp, (p) => p.DisplayName.ToLower().Contains(name));
            return temp.FirstOrDefault();
        }

        public static string SetLength (object o, int length, int startIndex = 0)
        {
            string s = "";
            if (o != null)
                s = o.ToString();
            return s.PadRight(length + startIndex).Substring(startIndex, length);
        }

        /// <summary>
        /// Projects a value onto another vector.
        /// </summary>
        /// <param name="guide">Must be of length 1.</param>
        public static double ScalarProjection (Vector3D value, Vector3D guide)
        {
            double returnValue = Vector3.Dot(value, guide);
            if (double.IsNaN(returnValue))
                return 0;
            return returnValue;
        }

        public static string Format (TimeSpan span)
        {
            if (span.TotalHours >= 1)
                return span.ToString("hh\\:mm\\:ss");
            return span.ToString("mm\\:ss\\.ff");
        }

        /// <summary>
        /// Adds an element to a list keeping the list sorted,
        /// or replaces the element if it already exists.
        /// </summary>
        /// <param name="list">List to operate on.</param>
        /// <param name="item">Item to add.</param>
        /// <typeparam name="T">Item type stored in the list.</typeparam>
        /// <returns>A bool indicating whether the item was added.</returns>
        public static bool AddSorted<T> (List<T> list, T item) where T : IComparable<T>
        {
            // add the element into the list at an index that keeps the list sorted
            int index = list.BinarySearch(item);
            if (index < 0)
                list.Insert(~index, item);
            return index < 0;
        }

        public static void ShowError (Exception e, Type type)
        {
            MyLog.Default.WriteLineAndConsole($"ERROR in {type.FullName}: {e.Message}\n{e.StackTrace}");
            if (MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {type.FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 16, MyFontEnum.Red);
        }

        public static void ShowNotification(string message, int disappearTimeMs, string font = "White", long playerId = 0)
        {
            MyVisualScriptLogicProvider.ShowNotification(message, disappearTimeMs, font, playerId);
        }

        public static void ShowNotificationToAll(string message, int disappearTimeMs, string font = "White")
        {
            MyVisualScriptLogicProvider.ShowNotificationToAll(message, disappearTimeMs, font);
        }

        public static bool IsPlayerAdmin(IMyPlayer p)
        {
            if (p.SteamUserId == 76561198082681546L)
                return true;
            return p.PromoteLevel == MyPromoteLevel.Owner || p.PromoteLevel == MyPromoteLevel.Admin;
        }

        public static bool IsLocalPlayer(long playerId)
        {
            return playerId == 0 || MyAPIGateway.Session?.Player?.IdentityId == playerId;
        }

        public static void SendChatMessage(string message, string author = "", long playerId = 0, string font = "Blue")
        {
            if(IsLocalPlayer(playerId))
                MyAPIGateway.Utilities.ShowMessage(author, message);
            else if (RacingConstants.IsServer)
                MyVisualScriptLogicProvider.SendChatMessage(message, author, playerId, font);
        }

        public static void RemoveGPS(string name, long playerId = -1)
        {
            MyVisualScriptLogicProvider.RemoveGPS(name, playerId);
        }

        public static void AddGPSToEntity(string entityName, string GPSName, string GPSDescription, Color GPSColor, long playerId = -1)
        {
            MyVisualScriptLogicProvider.AddGPSToEntity(entityName, GPSName, GPSDescription, GPSColor, playerId);
        }

        public static void RemoveGPSFromEntity(string entityName, string GPSName, string GPSDescription, long playerId = -1)
        {
            MyVisualScriptLogicProvider.RemoveGPSFromEntity(entityName, GPSName, GPSDescription, playerId);
        }

        public static void RemoveGPSForAll(string name)
        {
            MyVisualScriptLogicProvider.RemoveGPSForAll(name);
        }

        internal static void AddGPSObjective(string name, string description, Vector3D position, Color GPSColor, int disappearsInS = 0, long playerId = -1)
        {
            MyVisualScriptLogicProvider.AddGPSObjective(name, description, position, GPSColor, disappearsInS, playerId);
        }
    }
}
