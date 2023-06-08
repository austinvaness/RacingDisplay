using ProtoBuf;
using Sandbox.Graphics;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Paths
{
    public class ServerRaceRecorder : IRaceRecorder
    {
        private readonly IMyPlayer p;

        public ServerRaceRecorder(IMyPlayer p)
        {
            this.p = p;
        }

        // 0
        public void StartTrack() 
        {
            SendPacket(0);
        }

        // 1
        public void EndTrack()
        {
            SendPacket(1);
        }

        // 2
        public void LeftTrack()
        {
            SendPacket(2);
        }

        // 3 
        public void ClearData()
        {
            SendPacket(3);
        }

        private void SendPacket(byte id) 
        {
            new Packet(id).Send(p.SteamUserId);
        }

        [ProtoContract]
        public class Packet
        {
            [ProtoMember(1)]
            public byte methodId;

            public Packet()
            {

            }

            public Packet(byte methodId)
            {
                this.methodId = methodId;
            }

            public static void Received(ulong sender, byte[] data)
            {
                try
                {
                    Packet p = MyAPIGateway.Utilities.SerializeFromBinary<ServerRaceRecorder.Packet>(data);
                    if (RacingSession.Instance.Recorder == null)
                        RacingSession.Instance.Recorder = new ClientRaceRecorder();
                    p.Received(RacingSession.Instance.Recorder);
                }
                catch(Exception e)
                {
                    RacingTools.ShowError(e, typeof(Packet));
                }
            }

            public void Received(IRaceRecorder rec)
            {
                switch (methodId)
                {
                    case 0:
                        rec.StartTrack();
                        break;
                    case 1:
                        rec.EndTrack();
                        break;
                    case 2:
                        rec.LeftTrack();
                        break;
                    case 3:
                        rec.ClearData();
                        break;
                }
            }

            public void Send(ulong steamId)
            {
                if(MyAPIGateway.Session.Player?.SteamUserId == steamId)
                {
                    Received(RacingSession.Instance.Recorder);
                }
                else
                {
                    RacingSession.Instance.Net.SendTo(RacingConstants.packetRec, this, steamId);
                }
            }
        }
    }
}
