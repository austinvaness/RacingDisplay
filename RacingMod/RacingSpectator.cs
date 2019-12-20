using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using System.Linq;
using System.Text;
using Draygo.API;
using Sandbox.Game;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using Sandbox.Game.Entities;

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
            if (infoHud == null)
                return;

            if (activeRacersHud != null && config.HideHud.IsKeybindPressed())
                ToggleUI();

            if (!Spec.Enabled)
                return;

            if (followedPlayer != null && SpecCam != null && !MyAPIGateway.Session.IsCameraControlledObject)
            {
                IMyEntity e = followedPlayer.Character;
                IMyEntity curr = Spec.GetTarget();
                if (e == null)
                {
                    if (curr != null)
                    {
                        ClearFollow();
                        MyAPIGateway.Utilities.ShowNotification("Stopped spectating racer.", defaultMsgMs);
                    }
                    else if (followedPos == Vector3D.Zero)
                    {
                        ClearFollow();
                        RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, followedPlayer.SteamUserId, SpecCam.Position, 0));
                    }
                    else
                    {
                        SpecCam.Position = followedPos;
                        if(Runtime % 100 == 0)
                            RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, followedPlayer.SteamUserId, SpecCam.Position, 0));
                    }
                }
                else
                {
                    if(curr == null)
                    {
                        if (followedEntity == null)
                        {
                            followedPos = Vector3D.Zero;
                            followedEntity = e;
                            if (followedEntity != null)
                                SetTarget(e);
                            infoHudText.Clear().Append(followedPlayer.DisplayName);
                            UpdateInfo();
                        }
                        else
                        {
                            ClearFollow();
                            MyAPIGateway.Utilities.ShowNotification("Stopped spectating racer.", defaultMsgMs);
                        }
                    }
                    else if (curr.EntityId != followedEntity.EntityId)
                    {
                        ClearFollow();
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
            }

            if (SpecCam != null && !MyAPIGateway.Session.IsCameraControlledObject && !MyAPIGateway.Gui.ChatEntryVisible
                && !MyAPIGateway.Gui.IsCursorVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None)
            {
                if (config.NextPlayer.IsKeybindPressed())
                {
                    // Next
                    ulong currentId = 0;
                    if (followedEntity != null)
                        currentId = followedPlayer.SteamUserId;
                    RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, currentId, SpecCam.Position, 1));
                }
                else if (config.PrevPlayer.IsKeybindPressed())
                {
                    // Previous
                    ulong currentId = 0;
                    if (followedEntity != null)
                        currentId = followedPlayer.SteamUserId;
                    RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, currentId, SpecCam.Position, -1));
                }
                else if (config.LookAt.IsKeybindPressed())
                {
                    FaceFollowed(true);
                }
            }
        }

        private void ClearFollow ()
        {
            infoHudText.Clear();
            followedEntity = null;
            followedPlayer = null;
        }

        private void SetTarget (IMyEntity e)
        {
            Spec.SetTarget(e);
            FaceFollowed(false);
        }

        private void FaceFollowed (bool alwaysTurn)
        {
            IMyEntity e = Spec.GetTarget();
            if (e != null && SpecCam != null)
            {
                // Clamp
                Vector3D ePos = e.GetPosition();

                Vector3D diff = SpecCam.Position - ePos;
                double len2 = diff.LengthSquared();
                if (len2 < 1)
                    return;

                if (len2 > (maxCamDistance * maxCamDistance))
                    SpecCam.Position = ePos + ((diff / Math.Sqrt(len2)) * maxCamDistance);
                else if(!alwaysTurn)
                    return;

                Vector3D? up = null;
                MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(ePos);
                if (planet != null)
                {
                    MySphericalNaturalGravityComponent gravComp = planet.Components.Get<MyGravityProviderComponent>() as MySphericalNaturalGravityComponent;
                    if (gravComp != null && gravComp.IsPositionInRange(ePos))
                        up = ePos - planet.PositionComp.WorldAABB.Center;
                }

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
            if (p == null)
                return;

            IMyEntity curr = Spec.GetTarget();
            if(curr == null)
            {
                if(followedEntity != null)
                {
                    ClearFollow();
                    return;
                }
            }
            else
            {
                if(followedEntity == null || curr.EntityId != followedEntity.EntityId)
                    return;
            }

            infoHudText.Clear().Append(p.DisplayName).Append("\nloading");
            followedPlayer = p;
            followedPos = pos;
            followedEntity = p.Character;

            if (followedEntity == null)
                Spec.SetTarget(null);
            else
                SetTarget(followedEntity);

            UpdateInfo();
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
            if(info.direction == 0)
            {
                IMyPlayer p = GetPlayer(info.current);
                if (p != null)
                    SendNextRacerInfo(info.requestor, p, p.GetPosition());
            }
            else if (info.current == 0 || GetPlayer(info.current) == null || !activePlayers.Contains(info.current))
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

        private void SendNextRacerInfo(ulong requestor, IMyPlayer newRacer, Vector3 racerPos)
        {
            if (MyAPIGateway.Session.Player?.SteamUserId == requestor)
            {
                SetFollow(newRacer);
            }
            else
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new NextRacerInfo(newRacer.SteamUserId, racerPos));
                MyAPIGateway.Multiplayer.SendMessageTo(packetSpecResponse, data, requestor);
            }
        }
    }
}
