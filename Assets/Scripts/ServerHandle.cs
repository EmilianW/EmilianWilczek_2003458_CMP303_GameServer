// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        var clientIdCheck = packet.ReadInt();
        var username = packet.ReadString();

        Debug.Log($"{Server.Clients[fromClient].tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
        if (fromClient != clientIdCheck) Debug.Log($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        Server.Clients[fromClient].SendIntoGame(username);
    }

    public static void PlayerMovement(int fromClient, Packet packet)
    {
        var inputs = new bool[packet.ReadInt()];
        for (var i = 0; i < inputs.Length; i++) inputs[i] = packet.ReadBool();
        var rotation = packet.ReadQuaternion();

        Server.Clients[fromClient].Player.SetInput(inputs, rotation);
    }

    public static void PlayerShoot(int fromClient, Packet packet)
    {
        var shootDirection = packet.ReadVector3();

        Server.Clients[fromClient].Player.Shoot(shootDirection);
    }
}