using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;
using SpaceEngineers.Game.ModAPI;
using avaness.RacingMod.Hud;
using avaness.RacingMod.Net;
using avaness.RacingMod.Race;
using VRage;
using VRage.Game.ModAPI;

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
        public RacingHud Hud;
        public NodeManager Nodes;

        public bool HasTextHudAPI { get; private set; }

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
            if(Net == null)
                Net = new Network();

            if (RacingConstants.IsPlayer)
            {
                if(Hud == null)
                    Hud = new RacingHud(config, MapSettings);
                if (MyAPIGateway.Session.Player == null)
                    return;
            }

            CheckTextAPI();

            MyVisualScriptLogicProvider.RemoveBoardScreen("RacingDisplay", 0);

            if (RacingConstants.IsServer)
            {
                MyVisualScriptLogicProvider.RemoveGPSForAll(RacingConstants.gateWaypointName);
                MapSettings.Copy(RacingMapSettings.LoadFile());
                if(Hud != null)
                    Hud.OnEnabled += Hud_OnEnabled;
                race.Init();
            }
            else
            {
                int gateWaypointGps = MyAPIGateway.Session.GPS.Create(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, Vector3D.Zero, true).Hash;
                MyAPIGateway.Session.GPS.RemoveLocalGps(gateWaypointGps);
                Net.SendToServer(RacingConstants.packetSettingsInit, BitConverter.GetBytes(MyAPIGateway.Session.Player.SteamUserId));
            }

            Net.Register(RacingConstants.packetSettings, ReceiveSettings);
            Net.Register(RacingConstants.packetSettingsInit, ReceiveSettingsInit);
            Beacon.BeaconStorage.Register();

            config.Copy(RacingPreferences.LoadFile());

            cmds = new RacingCommands(race);

            MyLog.Default.WriteLineAndConsole("Racing Display started.");
            running = true;
        }

        private void CheckTextAPI()
        {
            foreach(var mod in MyAPIGateway.Session.Mods)
            {
                if(mod.PublishedFileId == 758597413)
                {
                    HasTextHudAPI = true;
                    return;
                }
            }
            HasTextHudAPI = false;
            MyLog.Default.WriteLineAndConsole("[WARNING] Text Hud API was not found! Alternate hud will be used.");
        }

        private void Hud_OnEnabled()
        {
            race.Bind(Hud);
            Hud.OnEnabled -= Hud_OnEnabled;
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
                if (RacingConstants.IsServer)
                    race.Update();

                if (Hud != null && config != null && config.HideHud.IsKeybindPressed())
                    Hud.ToggleUI();
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        protected override void UnloadData()
        {
            if(Hud != null)
            {
                Hud.Unload();
                Hud.OnEnabled -= Hud_OnEnabled;
            }
            race.Unload();
            Nodes.Unload();
            if(MapSettings != null)
                MapSettings.Unload();
            Net?.Unload();
            cmds?.Unload();
            api?.Unload();
        }

        public void SendAPIEvent(IMyPlayer p, API.RacingDisplayAPI.PlayerEvent pEvent)
        {
            api?.SendEvent(p, pEvent);
        }
    }
}
