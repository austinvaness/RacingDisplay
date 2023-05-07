using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;
using SpaceEngineers.Game.ModAPI;
using avaness.RacingMod.Paths;
using avaness.RacingMod.Hud;
using avaness.RacingMod.Net;
using avaness.RacingMod.Race;
using avaness.RacingMod.Chat;

namespace avaness.RacingMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingSession : MySessionComponentBase
    {
        public static RacingSession Instance;
        public int Runticks = 0;
        public DateTime Runtime = new DateTime (1);
        public readonly RacingMapSettings MapSettings = new RacingMapSettings();
        public readonly HashSet<IMyTimerBlock> StartTimers = new HashSet<IMyTimerBlock>();
        public Network Net;
        public NodeManager Nodes;

        public ClientRaceRecorder Recorder;

        private RacingHud hud;
        private RacingCommands cmds;
        private readonly Track race;
        private readonly RacingPreferences config = new RacingPreferences();
        private bool running;
        private API.APILogic api;
        //private int gateWaypointGps;


        public RacingSession ()
        {
            Instance = this;
            race = new Track(MapSettings);
            Nodes = new NodeManager(MapSettings, race);
        }

        private void Start()
        {
            if(cmds == null)
            {
                Net = new Network();
                cmds = new RacingCommands(race);
            }

            if (hud == null)
            {
                hud = RacingHud.Create(config, MapSettings);
                race.SetOutputHud(hud);
            }

            if (RacingConstants.IsPlayer && MyAPIGateway.Session.Player == null)
                return;

            if (RacingConstants.IsServer)
            {
                RacingTools.RemoveGPSForAll(RacingConstants.gateWaypointName);
                MapSettings.Copy(RacingMapSettings.LoadFile());
                race.LoadServer();
            }
            else
            {
                int gateWaypointGps = MyAPIGateway.Session.GPS.Create(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, Vector3D.Zero, true).Hash;
                MyAPIGateway.Session.GPS.RemoveLocalGps(gateWaypointGps);
                Net.SendToServer(RacingConstants.packetSettingsInit, BitConverter.GetBytes(MyAPIGateway.Session.Player.SteamUserId));
                Net.Register(RacingConstants.packetRec, ServerRaceRecorder.Packet.Received);
            }
            Net.Register(RacingConstants.packetSettings, ReceiveSettings);
            Net.Register(RacingConstants.packetSettingsInit, ReceiveSettingsInit);
            Beacon.BeaconStorage.Register();

            config.Copy(RacingPreferences.LoadFile());

            if(config.AutoRecord)
            {
                if(RacingConstants.IsServer)
                {
                    if (MyAPIGateway.Session.Player != null)
                        race.EnableRecording(MyAPIGateway.Session.Player.SteamUserId);
                }
                else
                {
                    Net.SendToServer<object>(RacingConstants.packetAutoRec, null);
                }
            }

            MyLog.Default.WriteLineAndConsole("Racing Display started.");
            running = true;
        }

        public override void SaveData()
        {
            if (RacingConstants.IsServer)
            {
                if (MapSettings != null)
                    MapSettings.SaveFile();
                race.SaveData();
            }
        }

        public int TriggerTimers()
        {
            foreach (IMyTimerBlock t in StartTimers)
                t.Trigger();
            return StartTimers.Count;
        }

        private void ReceiveSettings(byte[] data)
        {
            try
            {
                RacingMapSettings.Packet p = MyAPIGateway.Utilities.SerializeFromBinary<RacingMapSettings.Packet>(data);
                p.Perform();
                if (RacingConstants.IsServer)
                    Net.SendToOthers(RacingConstants.packetSettings, data);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void ReceiveSettingsInit(byte[] data)
        {
            try
            {
                if (RacingConstants.IsServer)
                {
                    ulong id = BitConverter.ToUInt64(data, 0);
                    if (id != 0)
                    {
                        byte[] config = MyAPIGateway.Utilities.SerializeToBinary(MapSettings);
                        Net.SendTo(RacingConstants.packetSettingsInit, config, id);
                    }
                }
                else
                {
                    RacingMapSettings config = MyAPIGateway.Utilities.SerializeFromBinary<RacingMapSettings>(data);
                    if (config != null)
                        MapSettings.Copy(config);
                }
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        public override void UpdateAfterSimulation ()
        {
            Runticks++;
            Runtime += RacingConstants.oneTick;

            if (MyAPIGateway.Session == null)
                return;

            // Startup
            if(api == null && Runticks >= 300)
                api = new API.APILogic(race);

            if (!running)
                Start();
            if (!running)
                return;
            // End startup

            try
            {
                if (RacingConstants.IsPlayer)
                {
                    Recorder?.Update();
                }

                if (RacingConstants.IsServer)
                {
                    race.Update();
                }
                /*else if(race.Debug)
                {
                    DebugRecorder();
                }*/

                if (hud != null && config != null && config.HideHud.IsKeybindPressed())
                    hud.ToggleUI();
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }


        /*public void DebugRecorder()
        {
            if (Recorder != null)
                Recorder.Debug();
        }*/

        protected override void UnloadData()
        {
            hud.Unload();
            race.Unload();
            Nodes.Unload();
            if(MapSettings != null)
                MapSettings.Unload();
            if (Net != null)
            {
                Net.Unload();
                cmds.Unload();
            }
            api?.Unload();
        }


    }
}
