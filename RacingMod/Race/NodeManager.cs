using avaness.RacingMod.Beacon;
using Sandbox.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingMod.Race
{
    public class NodeManager : IEnumerable<RacingBeacon>
    {
        public enum RacerState : byte
        {
            On, Finish, Reset
        }

        private readonly RacingMapSettings mapSettings;
        private readonly Track race;
        private readonly List<RacingBeacon> nodes = new List<RacingBeacon>();
        private double [] nodeDistances = { };
        private double startEndDist;

        public NodeManager(RacingMapSettings mapSettings, Track race)
        {
            this.mapSettings = mapSettings;
            this.race = race;
            mapSettings.LoopedChanged += MapSettings_LoopedChanged;
        }

        public void Unload()
        {
            mapSettings.LoopedChanged -= MapSettings_LoopedChanged;
        }

        public Vector3D GetCoords (int index)
        {
            return nodes [index].GetCoords();
        }

        private RacingBeacon End => nodes.Last();
        private RacingBeacon Start => nodes.First();

        private void MapSettings_LoopedChanged (bool looped)
        {
            if(nodeDistances.Length > 0)
            {
                TotalDistance = nodeDistances.Last();
                if (looped)
                    TotalDistance += startEndDist;
            }
        }

        /// <summary>
        /// Distance of one lap around the track.
        /// </summary>
        public double TotalDistance { get; private set; }

        public int Count => nodes.Count;

        public bool OnTrack(IMyPlayer p)
        {
            int start, end;
            double partial;
            GetClosestSegment(p.GetPosition(), out start, out end, out partial);
            return end > 0 && end < nodes.Count;
        }

        public bool RegisterNode (RacingBeacon node)
        {
            if (!RacingTools.AddSorted(nodes, node))
            {
                // checkpoint with this id already existed
                return false;
            }

            RebuildNodeInformation();
            return true;
        }

        public void RemoveNode (RacingBeacon node)
        {
            int index = nodes.FindIndex(other => other.Entity.EntityId == node.Entity.EntityId);
            if (index >= 0)
                nodes.RemoveAt(index);

            RebuildNodeInformation();
        }

        /// <summary>
        /// Gets the closest node segment to a position.
        /// </summary>
        /// <param name="position">The position to find the closest segment to.</param>
        /// <param name="start">The start index of the segment, or -1 if the position is before the track.</param>
        /// <param name="end">The end index of the segment, or nodes.Count if the position is after the track.</param>
        /// <param name="partialDist">The projected distance of the position along the segment from the start node.</param>
        /// <returns>True if there are greater than 2 nodes, false otherwise.</returns>
        private bool GetClosestSegment (Vector3D position, out int start, out int end, out double partialDist)
        {
            start = 0;
            end = -1;
            partialDist = 0;
            double minLinear2 = double.PositiveInfinity;

            if (nodes.Count < 2)
                return false;

            int closest = 0;
            long minDistanceGrid = nodes [0].Beacon.CubeGrid.EntityId;
            double minDistance2 = Vector3D.DistanceSquared(nodes [0].GetCoords(), position);

            for (int i = 1; i < nodes.Count; i++)
            {
                Vector3D node = nodes [i].GetCoords();
                Vector3D segment = nodes [i - 1].GetCoords() - node;
                double segmentLen = segment.Length();
                if (segmentLen <= 0)
                    continue;
                segment /= segmentLen;

                long beaconGrid = nodes [i].Beacon.CubeGrid.EntityId;
                Vector3D endToRacer = position - node;
                double dist2 = endToRacer.LengthSquared();
                bool verifyGrid = true;
                if (dist2 < minDistance2 && beaconGrid != minDistanceGrid)
                {
                    verifyGrid = false;
                    minDistanceGrid = beaconGrid;
                    minDistance2 = dist2;
                    closest = i;
                }

                double scaler = RacingTools.ScalarProjection(endToRacer, segment);
                if (scaler <= 0 || scaler > segmentLen || (verifyGrid && beaconGrid == minDistanceGrid))
                    continue;
                double linear2 = (endToRacer - (scaler * segment)).LengthSquared();

                if (linear2 < minLinear2)
                {
                    start = i - 1;
                    end = i;
                    partialDist = segmentLen - scaler;
                    minLinear2 = linear2;
                }
            }

            if (minDistance2 < minLinear2)
            {
                if (closest > nodes.Count - 1)
                    closest = nodes.Count - 1; // Racer is past the last node
                if (closest <= 0)
                    closest = -1;
                start = closest;
                end = closest + 1;
                partialDist = 0;
            }
            return true;
        }

        public void DrawDebug ()
        {
            if (nodes.Count == 0)
                return;

            Vector4 color1 = new Color(255, 255, 255, 1).ToVector4();
            Vector4 color2 = Color.White.ToVector4();
            MyStringId mat = MyStringId.GetOrCompute("Square");

            for (int i = 1; i < nodes.Count; i++)
            {
                Vector3D origin = nodes [i - 1].GetCoords();
                Vector3 direction = nodes [i].GetCoords() - origin;
                float len = direction.Length();
                if (len <= 0)
                    continue;
                direction /= len;
                MyTransparentGeometry.AddLineBillboard(mat, color1, origin, direction, len, 1, BlendTypeEnum.PostPP);
                MyTransparentGeometry.AddLineBillboard(mat, color2, origin, direction, len, 1, BlendTypeEnum.AdditiveTop);
            }
            if(mapSettings.Looped)
            {
                Vector3D origin = End.GetCoords();
                Vector3 direction = Start.GetCoords() - origin;
                float len = direction.Length();
                if (len > 0)
                {
                    direction /= len;
                    MyTransparentGeometry.AddLineBillboard(mat, color1, origin, direction, len, 1, BlendTypeEnum.PostPP);
                    MyTransparentGeometry.AddLineBillboard(mat, color2, origin, direction, len, 1, BlendTypeEnum.AdditiveTop);
                }
            }

            Start.DrawDebug();

            if (nodes.Count > 1)
            {
                foreach (RacingBeacon b in nodes.Skip(1))
                {
                    if(b.IsCheckpoint)
                        b.DrawDebug();
                }
            }
        }

        private void RebuildNodeInformation ()
        {
            // rebuild position cache
            Vector3D [] nodePositions = nodes.Select(b => b.GetCoords()).ToArray();

            // rebuild distance cache
            nodeDistances = new double [nodePositions.Length];
            if (nodeDistances.Length == 0)
                return;

            race.UpdateAllRacerNodes();

            double cumulative = 0;
            nodeDistances [0] = 0;
            Vector3D prev = nodePositions [0];
            for (int i = 1; i < nodePositions.Length; i++)
            {
                Vector3D v = nodePositions [i];
                cumulative += Vector3D.Distance(prev, v);
                nodeDistances [i] = cumulative;
                prev = nodePositions [i];
            }
            startEndDist = Vector3D.Distance(Start.GetCoords(), End.GetCoords());
            if (mapSettings.Looped)
                cumulative += startEndDist;
            TotalDistance = cumulative;
        }

        public bool ResetPosition(StaticRacerInfo info)
        {
            int start, end;
            double partial;
            if(GetClosestSegment(info.Racer.GetPosition(), out start, out end, out partial))
            {
                info.SetNextNode(Math.Min(end, nodes.Count - 1), true);
                return true;
            }
            return false;
        }

        public void JoinedRace(StaticRacerInfo info)
        {
            info.Reset();
            info.SetNextNode(0, true);
            if (OnTrack(info.Racer) && !Start.Contains(info.Racer))
            {
                // Joined the race after the start line
                NewOnTrack(info);
            }
            else
            {
                // Joined the race before the start line
                info.NextNode = 0;
            }
        }

        public RacerState Update(StaticRacerInfo info)
        {
            if (info.OnTrack)
            {
                if(RacingTools.GetCockpit(info.Racer) != null)
                    return UpdateInfo(info);
                return RacerState.On;
            }
            else
            {
                return WaitForTrack(info);
            }

        }

        /// <summary>
        /// Called when a racer is marked as being not on the track.
        /// </summary>
        private RacerState WaitForTrack(StaticRacerInfo info)
        {
            if (Start.Contains(info.Racer))
            {
                // Player is inside start line
                info.InStart = true;
                info.NextNode = 0;
                return RacerState.On;
            }
            else if (info.InStart && OnTrack(info.Racer))
            {
                // Player is outside of the start line and was in it previously
                NewOnTrack(info);
                return RacerState.On;
            }
            else
            {
                // Player is outside the start line and has never been in it
                info.InStart = false;
                info.NextNode = 0;
                return RacerState.On;
            }
        }

        /// <summary>
        /// Called when a racer is new to the track.
        /// </summary>
        private void NewOnTrack (StaticRacerInfo info, bool resetPos = true)
        {
            info.Timer.Reset(false);
            info.OnTrack = true;
            info.InStart = false;
            if(info.Recorder != null && mapSettings.TimedMode)
                info.Recorder.StartTrack();
            if (resetPos)
            {
                ResetPosition(info);
                if (info.NextNode == 0 || info.NextNode == nodes.Count)
                    info.NextNode = 1;
            }
            if (mapSettings.NumLaps > 1)
                MyVisualScriptLogicProvider.ShowNotification($"Lap {info.Laps + 1} / {mapSettings.NumLaps}", RacingConstants.defaultMsgMs, "White", info.Racer.IdentityId);
        }

        private RacerState UpdateInfo (StaticRacerInfo info)
        {
            double lapDistance = info.Laps * TotalDistance;
            int nextNode = info.NextNode;

            int start;
            int end;
            double partial;
            GetClosestSegment(info.Racer.GetPosition(), out start, out end, out partial);

            if(mapSettings.Looped)
            {
                info.Missed = false;
                if (start < 0 || end == nodes.Count)
                {
                    // Between last node and finish/start
                    if (nextNode == nodes.Count - 1 && NodeCleared(info.Racer, nodes.Count - 1))
                        info.NextNode = 0;
                    if(nextNode > 0)
                    {
                        if(nextNode > 1)
                            info.Missed = true;
                        info.Distance = lapDistance + nodeDistances [nextNode];
                    }
                    else
                    {
                        info.Missed = false;
                        info.Distance = lapDistance + nodeDistances.Last() + LastSegDistance(info.Racer);
                        if(nextNode == 0 && Start.Contains(info.Racer))
                            return RacerLapped(info, lapDistance);
                    }
                }
                else
                {
                    // On Track
                    info.Distance = lapDistance + nodeDistances [start] + partial;
                    if ((start == nextNode && NodeCleared(info.Racer, nextNode))
                        || (end == nextNode && CheckpointCleared(info.Racer, nextNode)))
                    {
                        info.Missed = false;
                        if(nextNode == 0)
                            return RacerLapped(info, lapDistance);
                        if (nextNode == nodes.Count - 1)
                            info.NextNode = 0;
                        else
                            info.NextNode = ++nextNode;
                    }
                    else if(start >= nextNode)
                    {
                        if(end == nodes.Count - 1 && nextNode == 0)
                        {
                            info.Missed = false;
                            info.NextNode = 0;
                        }
                        else
                        {
                            if (nextNode > 0 || end < nodes.Count - 1)
                                info.Missed = true;
                            info.Distance = lapDistance + nodeDistances [nextNode];
                        }
                    }

                }
                return RacerState.On;
            }
            else
            {
                if (start < 0)
                {
                    // Before start
                    if (nextNode > 0)
                    {
                        info.Distance = lapDistance + nodeDistances [nextNode];
                        info.Missed = true;
                        return RacerState.On;
                    }
                    else if (CheckpointCleared(info.Racer, 0))
                    {
                        info.NextNode = 1;
                    }
                    info.Distance = lapDistance;
                    info.Missed = false;
                    return RacerState.On;
                }
                else if (start == nodes.Count - 1)
                {
                    // After end
                    if (start == nextNode)
                    {
                        // The target was the last node
                        if (NodeCleared(info.Racer, nextNode))
                        {
                            // Finish line reached
                            return RacerLapped(info, lapDistance);
                        }
                        else
                        {
                            // Finish line was missed
                            info.Distance = lapDistance + nodeDistances [nodes.Count - 1];
                            info.Missed = true;
                            return RacerState.On;
                        }
                    }
                    else if (nextNode > 0)
                    {
                        // Has been on the track previously
                        info.Distance = lapDistance + nodeDistances [nextNode];
                        info.Missed = true;
                        return RacerState.On;
                    }
                    else
                    {
                        // The racer has not yet entered the track, display the first node
                        info.Missed = false;
                        info.NextNode = 0;
                        info.Distance = lapDistance;
                    }
                    return RacerState.On;
                }
                else
                {
                    // On the track
                    if (end <= nextNode)
                    {
                        // Racer is in expected position.
                        if (end == nextNode) // Collision check before node
                        {
                            if (CheckpointCleared(info.Racer, end))
                            {
                                if (end == nodes.Count - 1)
                                    return RacerLapped(info, lapDistance); // Reached finish line
                                info.NextNode = end + 1;
                            }
                        }

                        info.Distance = lapDistance + nodeDistances [start] + partial;
                        info.Missed = false;
                    }
                    else if (start == nextNode && NodeCleared(info.Racer, nextNode)) // Collision check after node
                    {
                        // Racer has moved past one node.
                        info.Missed = false;
                        info.NextNode = end;
                        info.Distance = lapDistance + nodeDistances [start] + partial;
                    }
                    else
                    {
                        // Racer has moved past multiple nodes, clamp them.
                        info.Missed = true;
                        info.Distance = lapDistance + nodeDistances [nextNode];
                    }
                    return RacerState.On;
                }
            }

        }

        private double LastSegDistance (IMyPlayer racer)
        {
            Vector3D lastCoords = End.GetCoords();
            Vector3D guide = Start.GetCoords() - lastCoords;
            double distance = guide.LengthSquared();
            if (distance <= 0)
                return 0;
            distance = Math.Sqrt(distance);
            guide /= distance;
            Vector3D racerV = racer.GetPosition() - lastCoords;
            double result = RacingTools.ScalarProjection(racerV, guide);
            if (result > distance)
                return distance;
            if (result <= 0)
                return 0;
            return result;
        }

        /// <summary>
        /// Returns true if the node at index is a checkpoint, and contains the player.
        /// </summary>
        private bool CheckpointCleared (IMyPlayer p, int index)
        {
            RacingBeacon node = nodes [index];
            return node.IsCheckpoint && node.Contains(p);
        }

        /// <summary>
        /// Returns true if the node at index is a node type, or contains player.
        /// </summary>
        private bool NodeCleared (IMyPlayer p, int index)
        {
            RacingBeacon node = nodes [index];
            return !node.IsCheckpoint || node.Contains(p);
        }

        private RacerState RacerLapped(StaticRacerInfo info, double lapDistance)
        {
            info.Missed = false;
            info.Laps++;
            info.Distance = lapDistance + nodeDistances[nodes.Count - 1];
            if (info.Laps >= mapSettings.NumLaps)
            {
                info.Laps = 0;
                if (info.MarkTime() && mapSettings.TimedMode)
                {
                    string time = info.BestTime.ToString(RacingConstants.timerFormating);
                    MyVisualScriptLogicProvider.ShowNotificationToAll($"{info.Racer.DisplayName} just finished with time {time}", RacingConstants.defaultMsgMs, "White");
                }

                if (mapSettings.TimedMode && info.AutoJoin)
                {
                    NewOnTrack(info, false);
                    if (mapSettings.Looped)
                        info.NextNode = 1;
                    else
                        info.NextNode = 0;
                    return RacerState.Reset;
                }
                else
                {
                    return RacerState.Finish;
                }
            }
            else
            {
                if (mapSettings.Looped)
                    info.NextNode = 1;
                else
                    info.NextNode = 0;
            }
            MyVisualScriptLogicProvider.ShowNotification($"Lap {info.Laps + 1} / {mapSettings.NumLaps}", RacingConstants.defaultMsgMs, "White", info.Racer.IdentityId);
            return RacerState.On;
        }

        public IEnumerator<RacingBeacon> GetEnumerator ()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return nodes.GetEnumerator();
        }
    }
}
