using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Utils;

namespace avaness.RacingMod.Net
{
    public class Network
    {
        private const ushort packetId = 1337;

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
            return GetData(id, obj);
        }


        public void SendTo(byte[] preppedData, ulong player)
        {
            MyAPIGateway.Multiplayer.SendMessageTo(packetId, preppedData, player);
        }

        public void SendTo<T>(byte id, T obj, ulong player) where T : class
        {
            MyAPIGateway.Multiplayer.SendMessageTo(packetId, GetData(id, obj), player);
        }

        public void SendToServer<T>(byte id, T obj) where T : class
        {
            MyAPIGateway.Multiplayer.SendMessageToServer(packetId, GetData(id, obj));
        }

        public void SendToOthers<T>(byte id, T obj) where T : class
        {
            MyAPIGateway.Multiplayer.SendMessageToOthers(packetId, GetData(id, obj));
        }

        private byte[] GetData<T>(byte id, T obj) where T : class
        {
            byte[] data;
            if (typeof(T) == typeof(byte[]))
                data = (byte[])(object)obj;
            else
                data = MyAPIGateway.Utilities.SerializeToBinary(obj);
            byte[] newData = new byte[data.Length + 1];
            newData[0] = id;
            Array.Copy(data, 0, newData, 1, data.Length);
            return newData;
        }

        private void ReceiveData(ushort packetId, byte[] data, ulong sender, bool fromServer)
        {
            if (data.Length == 0)
                return;

            byte id = data[0];
            Action<byte[]> func;
            if (receivers.TryGetValue(id, out func))
            {
                byte[] newData = new byte[data.Length - 1];
                Array.Copy(data, 1, newData, 0, newData.Length);
                func.Invoke(newData);
            }
        }
    }
}
