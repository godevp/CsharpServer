using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
public struct UdpHostToClient
{
    public const int CLIENT_CONNECTED = 1;
}

public struct ClientToUdpHost
{
    public const int CONNECT = 1;
    public const int DISCONNECT = 2;
}
class Server
{
    #region Variables
    List <Socket> udpSockets = new List <Socket> ();
    public const int UDPSocketsAmount = 2;
    Socket UDPServerSocket;
    Socket UDPServerSocket2;
    string localIP = "192.168.0.156";
    int localPort = 20001;
    static int bufferSize = 1024;
    protected readonly byte[] buffer = new byte[bufferSize];
    byte[] bytesToSend;
    EndPoint clientEndPoint;
    Int32 recvBytes;
    IPAddress clientIP;
    Thread UpdateThread;
    Thread udpThread;
    Thread tcpThread;


    //Players
    List<TcpListener> listeners;
    List<Player> playerList;

    #endregion
    /// <summary>
    ///Start is called before the first frame update
    /// </summary>
    public void Start()
    {
        Console.WriteLine("Server start");
        for (int i = 0; i < UDPSocketsAmount; i++)
        {
            udpSockets.Add(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp));
            udpSockets[i].Bind(new IPEndPoint(IPAddress.Parse(localIP), localPort + i));
        }
        listeners = new List<TcpListener>();
        playerList= new List<Player>();
        clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
    }
    #region Threads
    /// <summary>
    /// Function which calls all the existing threads to perform.
    /// Each thread updates every frame.
    /// @@@ Put it in infinite loop to run.
    /// </summary>
    public void RunThreads()
    {
        UpdateThread = new Thread(new ThreadStart(ReceiveUDPDataSocket1));
        StartThreadOnBackground(UpdateThread);
        udpThread = new Thread(new ThreadStart(ReceiveUDPDataSocket2));
        StartThreadOnBackground(udpThread);
    }
    /// <summary>
    /// Setting the thread to background and starting it.
    /// Save space to start the threads in future.
    /// </summary>
    /// <param name="xThread"></param>
    public void StartThreadOnBackground(Thread xThread)
    {
        xThread.IsBackground = true;
        xThread.Start();
    }
    #endregion
    #region Functions to be called in threads.
    /// <summary>
    /// Thread with receving messages from udp socket number 1
    /// </summary>
    public void ReceiveUDPDataSocket1()
    {
        MessageProcessingFromSocket(udpSockets[0]);
    }
    /// <summary>
    /// Thread with receving messages from udp socket number 2
    /// </summary>
    private void ReceiveUDPDataSocket2()
    {
        MessageProcessingFromSocket(udpSockets[1]);
    }

    public void MessageProcessingFromSocket(Socket socket)
    {
        recvBytes = socket.ReceiveFrom(buffer, ref clientEndPoint);
        if (recvBytes != 0)
        {
            
            string message = System.Text.Encoding.ASCII.GetString(buffer, 0, recvBytes);
            clientIP = ((IPEndPoint)clientEndPoint).Address;
            Console.WriteLine(clientIP + " : " + message);
            int z = 0;
            string[] splitter = message.Split(':');
            if (int.TryParse(splitter[0], out z))
            {
                switch (int.Parse(splitter[0]))
                {
                    case ClientToUdpHost.CONNECT:
                        bool playerExists = false;
                        foreach (Player player in playerList)
                        {
                            if (player.name == splitter[1])
                            {
                                playerExists = true;
                                Console.WriteLine("Player already exists: " + player.name);
                                break;
                            }
                        }
                        if (!playerExists)
                        {
                            playerList.Add(new Player(clientEndPoint, splitter[1]));
                            Console.WriteLine("Added player to list: " + splitter[1]);
                        }
                        SendMessageToUDPClient(UdpHostToClient.CLIENT_CONNECTED.ToString() + ':' + "you connected to server", clientEndPoint, socket);
                        clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        
                        break;
                    case ClientToUdpHost.DISCONNECT:
                        Console.WriteLine("PlayerlistCount : " + playerList.Count);
                        playerList.RemoveAll(player => player.name == splitter[1]);
                        Console.WriteLine("PlayerlistCount after delete : " + playerList.Count);
                        break;

                    default: break;
                }
            }
           

        }
    }
    /// <summary>
    /// Sending message to the clients with UDP protocol
    /// </summary>
    /// <param name="message"></param>
    /// <param name="client"></param>
    public void SendMessageToUDPClient(string message, EndPoint client, Socket socket)
    {
        bytesToSend = System.Text.Encoding.ASCII.GetBytes(message);
        socket.SendTo(bytesToSend, client);
    }

    public void SendUDPMessageToAllClients(string message, EndPoint client)
    {
       
    }
    #endregion


}