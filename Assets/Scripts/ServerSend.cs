// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

public class ServerSend
{
    private static void SendTCPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.Clients[toClient].tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.Clients[toClient].udp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (var i = 1; i <= Server.MaxPlayers; i++) Server.Clients[i].tcp.SendData(packet);
    }

    private static void SendTCPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (var i = 1; i <= Server.MaxPlayers; i++)
            if (i != exceptClient) Server.Clients[i].tcp.SendData(packet);
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (var i = 1; i <= Server.MaxPlayers; i++) Server.Clients[i].udp.SendData(packet);
    }

    private static void SendUDPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (var i = 1; i <= Server.MaxPlayers; i++)
            if (i != exceptClient) Server.Clients[i].udp.SendData(packet);
    }

    #region Packets

    public static void Welcome(int toClient, string msg)
    {
        using var packet = new Packet((int)ServerPackets.Welcome);
        
        packet.Write(msg);
        packet.Write(toClient);

        SendTCPData(toClient, packet);
    }

    public static void SpawnPlayer(int toClient, Player player)
    {
        using (var packet = new Packet((int)ServerPackets.SpawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(toClient, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using var packet = new Packet((int)ServerPackets.PlayerPosition);
        
        packet.Write(player.id);
        packet.Write(player.transform.position);

        SendUDPDataToAll(packet);
    }

    public static void PlayerRotation(Player player)
    {
        using var packet = new Packet((int)ServerPackets.PlayerRotation);
        
        packet.Write(player.id);
        packet.Write(player.transform.rotation);

        SendUDPDataToAll(player.id, packet);
    }

    public static void PlayerDisconnected(int playerId)
    {
        using var packet = new Packet((int)ServerPackets.PlayerDisconnected);
        
        packet.Write(playerId);

        SendTCPDataToAll(packet);
    }

    public static void PlayerHealth(Player player)
    {
        using var packet = new Packet((int)ServerPackets.PlayerHealth);
        
        packet.Write(player.id);
        packet.Write(player.health);

        SendTCPDataToAll(packet);
    }

    public static void PlayerRespawned(Player player)
    {
        using var packet = new Packet((int)ServerPackets.PlayerRespawned);
        
        packet.Write(player.id);

        SendTCPDataToAll(packet);
    }

    #endregion
}