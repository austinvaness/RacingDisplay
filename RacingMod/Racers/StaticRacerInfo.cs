using avaness.RacingMod.Race.Finish;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Racers
{
    public class StaticRacerInfo : IRacer, IFinisher, IEquatable<StaticRacerInfo>
    {
        public ulong Id { get; }
        public IMyPlayer Racer { get; set; }
        public string Name { get; }
        public string FormattedName { get; }
        public Timer Timer { get; }
        public bool InStart;
        public bool OnTrack { get; set; }
        public int Laps { get; set; } = 0;
        public TimeSpan BestTime { get; private set; } = new TimeSpan(0);
        public int RankUpFrame { get; set; } = 0;
        public int Rank { get; set; } = 0;
        /// <summary>
        /// Distance from start
        /// </summary>
        public double Distance { get; set; } = 0;
        public bool Missed { get; set; } = false;
        public bool AutoJoin = false;

        private int nextNode;
        public int NextNode
        {
            get
            {
                return nextNode;
            }
            set
            {
                SetNextNode(value);
            }
        }

        public StaticRacerInfo(IMyPlayer p)
        {
            Id = p.SteamUserId;
            Name = p.DisplayName;
            FormattedName = RacingTools.SetLength(p.DisplayName, RacingConstants.nameWidth);
            Racer = p;
            Timer = new Timer(true);
        }

        public void UpdateTime(SerializableFinisher finisher)
        {
            if (BestTime.Ticks == 0 || BestTime > finisher.BestTime)
                BestTime = finisher.BestTime;
        }

        public void SetNextNode (int value, bool force = false)
        {
            if (nextNode != value || force)
            {
                nextNode = value;
                HideWaypoint();
                MyVisualScriptLogicProvider.AddGPSObjective(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, RacingSession.Instance.Nodes.GetCoords(value),
                    RacingConstants.gateWaypointColor, 0, Racer.IdentityId);
            }
        }

        public void HideWaypoint()
        {
            MyVisualScriptLogicProvider.RemoveGPS(RacingConstants.gateWaypointName, Racer.IdentityId);
        }


        /// <summary>
        /// Returns true if the new time was used.
        /// </summary>
        public bool MarkTime()
        {
            TimeSpan span = Timer.GetTime();
            if (BestTime.Ticks == 0 || span < BestTime)
            {
                BestTime = span;
                return true;
            }
            return false;
        }

        public void RemoveFinish()
        {
            BestTime = new TimeSpan(0);
        }

        public void Reset()
        {
            NextNode = 0;
            InStart = false;
            OnTrack = false;
            Laps = 0;
            Timer.Reset(true);
            Rank = 0;
            Distance = 0;
            Missed = false;
            AutoJoin = false;
        }

        public ulong[] GetSelectedGhosts()
        {
            // TODO
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            return players.Select(p => p.SteamUserId).ToArray();
        }




        public override bool Equals(object obj)
        {
            return Equals(obj as StaticRacerInfo);
        }

        public bool Equals(StaticRacerInfo other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }

        public static bool operator ==(StaticRacerInfo left, StaticRacerInfo right)
        {
            return EqualityComparer<StaticRacerInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(StaticRacerInfo left, StaticRacerInfo right)
        {
            return !(left == right);
        }
    }
}
