// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Object = UnityEngine.Object;

public class Client
{
    private const int DataBufferSize = 4096;

    private readonly int _id;
    public Player Player;
    public readonly TCP tcp;
    public readonly UDP udp;

    public Client(int clientId)
    {
        _id = clientId;
        tcp = new TCP(_id);
        udp = new UDP(_id);
    }

    public void SendIntoGame(string playerName)
    {
        Player = NetworkManager.Instance.InstantiatePlayer();
        Player.Initialize(_id, playerName);

        foreach (var client in Server.Clients.Values.Where(client => client.Player != null).Where(client => client._id != _id))
            ServerSend.SpawnPlayer(_id, client.Player);

        foreach (var client in Server.Clients.Values.Where(client => client.Player != null))
            ServerSend.SpawnPlayer(client._id, Player);
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.Socket.Client.RemoteEndPoint} has disconnected.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            Object.Destroy(Player.gameObject);
            Player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisconnected(_id);
    }

    public class TCP
    {
        private readonly int _id;
        private byte[] _receiveBuffer;
        private Packet _receivedData;
        public TcpClient Socket;
        private NetworkStream _stream;

        public TCP(int id)
        {
            _id = id;
        }

        public void Connect(TcpClient socket)
        {
            Socket = socket;
            Socket.ReceiveBufferSize = DataBufferSize;
            Socket.SendBufferSize = DataBufferSize;

            _stream = Socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[DataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(_id, "Welcome to the server!");
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (Socket != null) _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to player {_id} via TCP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                var byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Server.Clients[_id].Disconnect();
                    return;
                }

                var data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                _receivedData.Reset(HandleData(data)); // Reset receivedData if all data was handled
                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error receiving TCP data: {ex}");
                Server.Clients[_id].Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            var packetLength = 0;

            _receivedData.SetBytes(data);

            if (_receivedData.UnreadLength() >= 4)
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0) return true;
            }

            while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
            {
                var packetBytes = _receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using var packet = new Packet(packetBytes);
                    var packetId = packet.ReadInt();
                    Server.PacketHandlers[packetId](_id, packet);
                });

                packetLength = 0;
                if (_receivedData.UnreadLength() < 4) continue;
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0) return true;
            }

            return packetLength <= 1;
        }

        public void Disconnect()
        {
            Socket.Close();
            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            Socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint EndPoint;

        private readonly int _id;

        public UDP(int id)
        {
            _id = id;
        }

        public void Connect(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public void SendData(Packet packet)
        {
            Server.SendUDPData(EndPoint, packet);
        }

        public void HandleData(Packet packetData)
        {
            var packetLength = packetData.ReadInt();
            var packetBytes = packetData.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using var packet = new Packet(packetBytes);
                var packetId = packet.ReadInt();
                Server.PacketHandlers[packetId](_id, packet);
            });
        }

        public void Disconnect()
        {
            EndPoint = null;
        }
    }
}