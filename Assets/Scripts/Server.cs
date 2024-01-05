// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public delegate void PacketHandler(int fromClient, Packet packet);

    public static readonly Dictionary<int, Client> Clients = new();
    public static Dictionary<int, PacketHandler> PacketHandlers;

    private static TcpListener _tcpListener;
    private static UdpClient _udpListener;
    public static int MaxPlayers { get; private set; }
    private static int Port { get; set; }

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log("Starting server...");
        InitializeServerData();

        _tcpListener = new TcpListener(IPAddress.Any, Port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

        _udpListener = new UdpClient(Port);
        _udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on port {Port}.");
    }

    private static void TcpConnectCallback(IAsyncResult result)
    {
        var client = _tcpListener.EndAcceptTcpClient(result);
        _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");

        for (var i = 1; i <= MaxPlayers; i++)
            if (Clients[i].tcp.Socket == null)
            {
                Clients[i].tcp.Connect(client);
                return;
            }

        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var data = _udpListener.EndReceive(result, ref clientEndPoint);
            _udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4) return;

            using var packet = new Packet(data);
            var clientId = packet.ReadInt();

            if (clientId == 0) return;

            if (Clients[clientId].udp.EndPoint == null)
            {
                Clients[clientId].udp.Connect(clientEndPoint);
                return;
            }

            if (Clients[clientId].udp.EndPoint.ToString() == clientEndPoint.ToString())
                Clients[clientId].udp.HandleData(packet);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex}");
        }
    }

    public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
                _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to {clientEndPoint} via UDP: {ex}");
        }
    }

    private static void InitializeServerData()
    {
        for (var i = 1; i <= MaxPlayers; i++) Clients.Add(i, new Client(i));

        PacketHandlers = new Dictionary<int, PacketHandler>
        {
            { (int)ClientPackets.WelcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.PlayerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.PlayerShoot, ServerHandle.PlayerShoot }
        };
        Debug.Log("Initialized packets.");
    }

    public static void Stop()
    {
        _tcpListener.Stop();
        _udpListener.Close();
    }
}