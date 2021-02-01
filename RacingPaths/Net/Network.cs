using Sandbox.ModAPI;
using System;
using System.Collections.Generic;

namespace avaness.RacingPaths.Net
{
    public class Network
    {
        public const byte packetRaceStart = 0;
        public const byte packetRaceEnd = 1;


        private const ushort packetId = 33982;

        private readonly Dictionary<byte, Action<byte[]>> receivers = new Dictionary<byte, Action<byte[]>>();

        public Network()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(packetId, ReceiveData);
        }

        public void Unload()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(packetId, ReceiveData);
            receivers.Clear();
        }

        public void Register(byte id, Action<byte[]> func)
        {
            receivers.Add(id, func);
        }

        public void Unregister(byte id)
        {
            receivers.Remove(id);
        }

        /// <summary>
        /// Returns a byte array that can be used in vanilla SendMessageTo commands.
        /// </summary>
        public byte[] Prep<T>(byte id, T obj) where T : class
        {
            //if (debug && id != 0)
            //    MyLog.Default.WriteLineAndConsole($"Prepped packet of {typeof(T)} with id {id}");
            return GetData(id, obj);
        }


        public void SendTo(byte[] preppedData, ulong player)
        {
            MyAPIGateway.Multiplayer.SendMessageTo(packetId, preppedData, player);
        }

        public void SendTo<T>(byte id, T obj, ulong player) where T : class
        {
            //if (debug && id != 0)
            //    MyLog.Default.WriteLineAndConsole($"Sent {typeof(T)} packet with id {id} to {player}");
            MyAPIGateway.Multiplayer.SendMessageTo(packetId, GetData(id, obj), player);
        }

        public void SendToServer<T>(byte id, T obj) where T : class
        {
            //if (debug && id != 0)
            //    MyLog.Default.WriteLineAndConsole($"Sent {typeof(T)} packet with id {id} to Server");
            MyAPIGateway.Multiplayer.SendMessageToServer(packetId, GetData(id, obj));
        }

        public void SendToOthers<T>(byte id, T obj) where T : class
        {
            if (!RacingPathsSession.IsServer)
                throw new Exception();
            //if (debug && id != 0)
            //    MyLog.Default.WriteLineAndConsole($"Sent {typeof(T)} packet with id {id} to Clients");
            MyAPIGateway.Multiplayer.SendMessageToOthers(packetId, GetData(id, obj));
        }

        private byte[] GetData<T>(byte id, T obj) where T : class
        {
            byte[] data;
            if (obj == null)
                return new byte[1] { id };
            if (typeof(T) == typeof(byte[]))
                data = (byte[])(object)obj;
            else
                data = MyAPIGateway.Utilities.SerializeToBinary(obj);
            byte[] newData = new byte[data.Length + 1];
            newData[0] = id;
            if(data.Length > 0)
                Array.Copy(data, 0, newData, 1, data.Length);
            return newData;
        }

        private void ReceiveData(ushort packetId, byte[] data, ulong sender, bool fromServer)
        {
            if (data.Length == 0)
                return;

            byte id = data[0];
            Action<byte[]> func;
            if(data.Length == 1)
            if (receivers.TryGetValue(id, out func))
            {
                //if (debug && id != 0)
                //    MyLog.Default.WriteLineAndConsole($"Received packet with id {id}.");

                if(data.Length == 1)
                {
                    func.Invoke(new byte[0]);
                }
                else
                {
                    byte[] newData = new byte[data.Length - 1];
                    Array.Copy(data, 1, newData, 0, newData.Length);
                    func.Invoke(newData);
                }
            }
        }
    }
}
