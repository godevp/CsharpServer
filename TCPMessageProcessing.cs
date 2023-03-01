namespace CsharpServer;
using System;
using System.Net.Sockets;
using System.Text;
public struct HostToClient
{
    public const int CLIENT_CONNECTED = 1;
    public const int THE_NAME_IS_USED = 2;
    public const int ADD_PLAYER_TO_SCREEN = 3;
    public const int SEND_PLAYERDEST_TO_CLIENT = 4;
    public const int SEND_PLAYER_POS_DEST_TO_CLIENT = 5;

}

public struct ClientToHost
{
    public const int CONNECT = 1;
    public const int DISCONNECT = 2;
    public const int SEND_MY_DESTINATION = 3;
    public const int SEND_MY_POS_AND_DEST = 4;
    
}
public class TCPMessageProcessing
{
    /// <summary>
    /// Gets the message from the sender and decides how to reply
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="clients"></param>
    public static void HandleMessage(TcpClient sender, string message, List<TcpClient> clients)
    {
        NetworkStream senderStream = sender.GetStream();
        string response = "";
        string[] splitter = message.Split(";");
        int c = 0;
        int identifier = 0;
        if (int.TryParse(splitter[1], out c))
        {
            identifier = int.Parse(splitter[1]);
        }

        switch (identifier)
        {
            case 1:
                SendTCPMessage("Sendning to the client who sent message", sender);
                SendTCPMessageToAllOtherClients("Message to all other connected clients", sender, clients);
                break;


            default:
                break;
        }
    }
    /// <summary>
    /// Sends a message(respond) only to the sender
    /// </summary>
    /// <param name="message"></param>
    /// <param name="client"></param>
    private static void SendTCPMessage(string message,TcpClient client)
    {
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        NetworkStream stream = client.GetStream();
        stream.Write(messageBytes, 0, messageBytes.Length);
    }
    /// <summary>
    /// Sends a message to all clients except the sender
    /// </summary>
    /// <param name="message"></param>
    /// <param name="sender"></param>
    /// <param name="clients"></param>
    private static void SendTCPMessageToAllOtherClients(string message,TcpClient sender, List<TcpClient> clients)
    {
        foreach (TcpClient client in clients)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            if (client != sender && client != null)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
    }
}