using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RacingMod
{
    public partial class RacingSession
    {
        const ushort packetSpecRequest = 1357;
        const ushort packetSpecResponse = 1358;

        IMyPlayer followedPlayer;
        Vector3D followedPos;
        IMyEntity followedEntity;
        MySpectator SpecCam => MyAPIGateway.Session.CameraController as MySpectator;
        const double maxCamDistance = 100;

        void Spectator ()
        {
            if (!Spec.Enabled || infoHud == null)
                return;

            if (followedPlayer != null && SpecCam != null && !MyAPIGateway.Session.IsCameraControlledObject)
            {
                IMyEntity e = GetEntity(followedPlayer);
                if (e == null)
                {
                    if (followedPos == Vector3D.Zero)
                    {
                        infoHudText.Clear();
                        followedEntity = null;
                        followedPlayer = null;
                        MyAPIGateway.Utilities.ShowNotification("Racer for spectator lost.", defaultMsgMs);
                    }
                    else
                    {
                        SpecCam.Position = followedPos;
                    }
                    return;
                }

                IMyEntity curr = Spec.GetTarget();
                if (followedEntity == null)
                {
                    followedPos = Vector3D.Zero;
                    followedEntity = e;
                    if (followedEntity != null)
                        SetTarget(e);
                    infoHudText.Clear().Append(followedPlayer.DisplayName);
                    UpdateInfo();
                }
                else if (curr == null || curr.EntityId != e.EntityId)
                {
                    infoHudText.Clear();
                    followedEntity = null;
                    followedPlayer = null;
                    MyAPIGateway.Utilities.ShowNotification("Stopped spectating racer.", defaultMsgMs);
                }
                else
                {
                    if (followedPos != Vector3D.Zero)
                    {
                        followedPos = Vector3D.Zero;
                        infoHudText.Clear().Append(followedPlayer.DisplayName);
                        UpdateInfo();
                    }
                    if (followedEntity.EntityId != e.EntityId)
                    {
                        followedEntity = e;
                        SetTarget(e);
                    }
                }
            }

            if (SpecCam != null && !MyAPIGateway.Session.IsCameraControlledObject && !MyAPIGateway.Gui.ChatEntryVisible
                && !MyAPIGateway.Gui.IsCursorVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None)
            {
                if (MyAPIGateway.Input.IsNewKeyPressed(config.NextPlayer))
                {
                    // Next
                    ulong currentId = 0;
                    if (followedEntity != null)
                        currentId = followedPlayer.SteamUserId;
                    RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, currentId, SpecCam.Position, true));
                }
                else if (MyAPIGateway.Input.IsNewKeyPressed(config.PrevPlayer))
                {
                    // Previous
                    ulong currentId = 0;
                    if (followedEntity != null)
                        currentId = followedPlayer.SteamUserId;
                    RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, currentId, SpecCam.Position, false));
                }
            }
        }

        private void SetTarget (IMyEntity e)
        {
            Spec.SetTarget(e);
            FaceFollowed();
        }

        private void FaceFollowed ()
        {
            if (followedEntity != null && SpecCam != null)
            {
                // Clamp
                IMyEntity e = followedEntity;
                Vector3D ePos = e.GetPosition();

                Vector3D diff = SpecCam.Position - ePos;
                double len2 = diff.LengthSquared();
                if (len2 > (maxCamDistance * maxCamDistance))
                    SpecCam.Position = ePos + ((diff / Math.Sqrt(len2)) * maxCamDistance);
                else
                    return;

                Vector3D? up = null;

                Vector3? grav = e.Physics?.Gravity;
                if (grav.HasValue)
                    up = -Vector3.Normalize(grav.Value);

                SpecCam.SetTarget(ePos, up);
            }
        }

        /// <summary>
        /// Called only on server!!!
        /// </summary>
        private void SetFollow (IMyPlayer p)
        {
            SetFollow(p, p.GetPosition());
        }

        private void SetFollow (ulong id, Vector3D pos)
        {
            SetFollow(GetPlayer(id), pos);
        }

        private void SetFollow (IMyPlayer p, Vector3D pos)
        {
            if (p != null)
            {
                infoHudText.Clear().Append(p.DisplayName).Append("\nloading");
                followedPlayer = p;
                followedPos = pos;
                followedEntity = GetEntity(p);

                if (followedEntity == null)
                    Spec.SetTarget(null);
                else
                    SetTarget(followedEntity);

                UpdateInfo();
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
                ShowError(e, GetType());
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
                    ShowError(e, GetType());
                }
        }

        void AddSpecRequest (CurrentRacerInfo info)
        {
            if (info.current == 0 || GetPlayer(info.current) == null || !activePlayers.Contains(info.current))
            {
                IMyPlayer p;
                if (GetClosestRacer(info.camera, out p))
                {
                    if (MyAPIGateway.Session.Player?.SteamUserId == info.requestor)
                    {
                        SetFollow(p);
                    }
                    else
                    {
                        byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new NextRacerInfo(p.SteamUserId, p.GetPosition()));
                        MyAPIGateway.Multiplayer.SendMessageTo(packetSpecResponse, data, info.requestor);
                    }
                }
            }
            else if (info.direction)
            {
                HashSet<ulong> list;
                if (!nextPlayerRequests.TryGetValue(info.current, out list))
                    list = new HashSet<ulong>();
                list.Add(info.requestor);
                nextPlayerRequests [info.current] = list;
            }
            else
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
            {
                if (MyAPIGateway.Session.Player?.SteamUserId == id)
                {
                    SetFollow(newRacer);
                }
                else
                {
                    byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new NextRacerInfo(newRacer.SteamUserId, racerPos));
                    MyAPIGateway.Multiplayer.SendMessageTo(packetSpecResponse, data, id);
                }
            }
        }
    }
}
