﻿using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using ProtoBuf;
using KlimeDraygo.RelativeSpectator.API;

namespace avaness.RacingMod
{
    public partial class RacingSession
    {
        IMyPlayer followedPlayer;
        bool hasFollower;
        Vector3D followedPos;

        MySpectator SpecCam => MyAPIGateway.Session.CameraController as MySpectator;

        void Spectator ()
        {
            if (infoHud == null)
                return;

            if (activeRacersHud != null && config.HideHud.IsKeybindPressed() && !MyAPIGateway.Gui.ChatEntryVisible)
                ToggleUI();

            if (!Spec.Enabled)
                return;

            if (infoHud.Visible && MyAPIGateway.Session.IsCameraControlledObject)
                infoHud.Visible = false;
            else if (!infoHud.Visible && !MyAPIGateway.Session.IsCameraControlledObject)
                infoHud.Visible = true;


            if (followedPlayer != null)
            {
                IMyEntity target = Spec.GetTarget();
                IMyEntity character = followedPlayer.Character;

                if (hasFollower)
                {
                    // Character was within render distance
                    if(character == null || target == null || target.EntityId != character.EntityId)
                        RaceClear();
                }
                else
                {
                    // Character has not yet been found
                    if(target == null)
                    {
                        // There is no target
                        if(character == null)
                        {
                            // Character is still not in render distance
                            SpecCam.Position = followedPos;
                            if (Runticks % 100 == 0)
                                RequestNextRacer(followedPlayer.SteamUserId, SpecCam.Position, 0);
                        }
                        else
                        {
                            // Character is now within render distance
                            Spec.SetTarget(character);
                            hasFollower = true;
                            infoHudText.Clear().Append(followedPlayer.DisplayName);
                            UpdateInfoHud();
                        }
                    }
                    else
                    {
                        // The target is not the current player
                        RaceClear();
                    }
                }
            }

            if (SpecCam != null && !MyAPIGateway.Session.IsCameraControlledObject && !MyAPIGateway.Gui.ChatEntryVisible
                && !MyAPIGateway.Gui.IsCursorVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None)
            {
                if (config.StopSpec.IsKeybindPressed())
                {
                    RaceClear();
                }
                else if (config.NextPlayer.IsKeybindPressed())
                {
                    // Next
                    ulong currentId = 0;
                    if (followedPlayer?.Character != null)
                        currentId = followedPlayer.SteamUserId;
                    RequestNextRacer(currentId, SpecCam.Position, 1);
                }
                else if (config.PrevPlayer.IsKeybindPressed())
                {
                    // Previous
                    ulong currentId = 0;
                    if (followedPlayer?.Character != null)
                        currentId = followedPlayer.SteamUserId;
                    RequestNextRacer(currentId, SpecCam.Position, -1);
                }

            }
        }

        private void SpecClear()
        {
            Spec.SetMode(SpecCamAPI.CameraMode.None);
            Spec.SetTarget(null);
        }

        private void RaceClear()
        {
            followedPlayer = null;
            infoHudText.Clear();
        }

        /// <summary>
        /// Called only on server!!!
        /// </summary>
        private void SetFollow (IMyPlayer p)
        {
            if (p == null)
                RaceClear();
            else
                SetFollow(p, p.GetPosition());
        }

        private void SetFollow (ulong id, Vector3D pos)
        {
            SetFollow(RacingTools.GetPlayer(id), pos);
        }

        private void SetFollow (IMyPlayer p, Vector3D pos)
        {
            if (p == null || pos == Vector3D.Zero)
            {
                RaceClear();
                return;
            }

            infoHudText.Clear().Append(p.DisplayName);
            followedPlayer = p;
            followedPos = pos;
            if (p.Character == null)
            {
                infoHudText.AppendLine().Append("loading");
                SpecClear();
                hasFollower = false;
            }
            else
            {
                Spec.SetTarget(p.Character);
                hasFollower = true;
            }
            UpdateInfoHud();
        }


        private void RequestNextRacer (ulong currentId, Vector3 cameraPos, sbyte direction)
        {
            CurrentRacerInfo info = new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, currentId, cameraPos, direction);
            if (RacingConstants.IsServer)
            {
                AddSpecRequest(info);
            }
            else
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(info);
                MyAPIGateway.Multiplayer.SendMessageToServer(RacingConstants.packetSpecRequest, data);
            }
        }

        private void ReceiveSpecResponse (byte [] obj)
        {
            try
            {
                NextRacerInfo info = MyAPIGateway.Utilities.SerializeFromBinary<NextRacerInfo>(obj);
                SetFollow(info.steamId, info.position);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void ReceiveSpecRequest (byte [] data)
        {
            if (activePlayers.Count != 0)
                try
                {
                    AddSpecRequest(MyAPIGateway.Utilities.SerializeFromBinary<CurrentRacerInfo>(data));
                }
                catch (Exception e)
                {
                    RacingTools.ShowError(e, GetType());
                }
        }

        void AddSpecRequest (CurrentRacerInfo info)
        {
            if(info.direction == 0)
            {
                IMyPlayer p = RacingTools.GetPlayer(info.current);
                if (p == null)
                    SendNextRacerInfo(info.requestor);
                else
                    SendNextRacerInfo(info.requestor, p, p.GetPosition());
            }
            else if (info.current == 0 || RacingTools.GetPlayer(info.current) == null || !activePlayers.Contains(info.current))
            {
                IMyPlayer p;
                if (GetClosestRacer(info.camera, out p))
                    SendNextRacerInfo(info.requestor, p, p.GetPosition());
            }
            else if (info.direction == 1)
            {
                HashSet<ulong> list;
                if (!nextPlayerRequests.TryGetValue(info.current, out list))
                    list = new HashSet<ulong>();
                list.Add(info.requestor);
                nextPlayerRequests [info.current] = list;
            }
            else if(info.direction == -1)
            {
                HashSet<ulong> list;
                if (!prevPlayerRequests.TryGetValue(info.current, out list))
                    list = new HashSet<ulong>();
                list.Add(info.requestor);
                prevPlayerRequests [info.current] = list;
            }
        }



        private void SendNextSpectatorResponse (ulong oldRacer, IMyPlayer newRacer)
        {
            HashSet<ulong> requestors;
            if (nextPlayerRequests.TryRemove(oldRacer, out requestors) && requestors.Count > 0)
            {
                SendSpectatorResponse(requestors, newRacer);
                requestors.Clear();
                nextPlayerRequests [oldRacer] = requestors;
            }
        }

        private void SendPrevSpectatorResponse (ulong oldRacer, IMyPlayer newRacer)
        {
            HashSet<ulong> requestors;
            if (prevPlayerRequests.TryRemove(oldRacer, out requestors) && requestors.Count > 0)
            {
                SendSpectatorResponse(requestors, newRacer);
                requestors.Clear();
                prevPlayerRequests [oldRacer] = requestors;
            }
        }

        private void SendSpectatorResponse (IEnumerable<ulong> requestors, IMyPlayer newRacer)
        {
            Vector3D racerPos = newRacer.GetPosition();
            foreach (ulong id in requestors)
                SendNextRacerInfo(id, newRacer, racerPos);
        }

        private void SendNextRacerInfo(ulong requestor)
        {
            if (MyAPIGateway.Session.Player?.SteamUserId == requestor)
            {
                SetFollow(null);
            }
            else
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new NextRacerInfo(0, Vector3D.Zero));
                MyAPIGateway.Multiplayer.SendMessageTo(RacingConstants.packetSpecResponse, data, requestor);
            }
        }

        private void SendNextRacerInfo(ulong requestor, IMyPlayer newRacer, Vector3 racerPos)
        {
            if (MyAPIGateway.Session.Player?.SteamUserId == requestor)
            {
                SetFollow(newRacer);
            }
            else
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new NextRacerInfo(newRacer.SteamUserId, racerPos));
                MyAPIGateway.Multiplayer.SendMessageTo(RacingConstants.packetSpecResponse, data, requestor);
            }
        }


        /// <summary>
        /// Use ONLY on server, not client!
        /// </summary>
        bool GetClosestRacer (Vector3D position, out IMyPlayer closest)
        {
            double minDistance = double.PositiveInfinity;
            closest = null;

            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp, (p) => activePlayers.Contains(p.SteamUserId));
            foreach (IMyPlayer p in playersTemp)
            {
                double dist = Vector3D.DistanceSquared(p.GetPosition(), position);
                if (dist < minDistance)
                {
                    closest = p;
                    minDistance = dist;
                }
            }
            return closest != null;
        }

        [ProtoContract]
        public class NextRacerInfo
        {
            [ProtoMember(1)]
            public ulong steamId;
            [ProtoMember(2)]
            public Vector3 position;

            public NextRacerInfo ()
            {
            }

            public NextRacerInfo (ulong steamId, Vector3 position)
            {
                this.steamId = steamId;
                this.position = position;
            }
        }

        [ProtoContract]
        public class CurrentRacerInfo
        {
            [ProtoMember(1)]
            public ulong requestor;
            [ProtoMember(2)]
            public ulong current;
            [ProtoMember(3)]
            public Vector3 camera;
            [ProtoMember(4)]
            public sbyte direction;

            public CurrentRacerInfo ()
            {
            }

            public CurrentRacerInfo (ulong requestor, ulong current, Vector3 camera, sbyte direction)
            {
                this.requestor = requestor;
                this.current = current;
                this.camera = camera;
                this.direction = direction;
            }
        }
    }
}
