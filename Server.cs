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
    public const int THE_NAME_IS_USED = 2;
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
    private Socket udpSocket;
    string localIP = "10.0.246.195";
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
        udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpSocket.Bind(new IPEndPoint(IPAddress.Parse(localIP), localPort));
        
        //listeners = new List<TcpListener>();
        playerList= new List<Player>();
        clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        startThreads();
    }
    #region Threads
    /// <summary>
    /// Function which calls all the existing threads to perform.
    /// Each thread updates every frame.
    /// @@@ Put it in infinite loop to run.
    /// </summary>
    public void startThreads()
    {
        
        udpThread = new Thread(new ThreadStart(ReceiveUDPDataSocket1));
        StartThreadOnBackground(udpThread);
        // UpdateThread = new Thread(new ThreadStart(ReceiveUDPDataSocket2));
        // StartThreadOnBackground(UpdateThread);
    }

    public void runThreads()
    {
        ReceiveUDPDataSocket1();
        //ReceiveUDPDataSocket2();
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
        MessageProcessingFromSocket(udpSocket);
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
            
            string fullMessage = System.Text.Encoding.ASCII.GetString(buffer, 0, recvBytes);
            clientIP = ((IPEndPoint)clientEndPoint).Address;
            Console.WriteLine(clientIP + " : " + fullMessage);
            
            string[] splitter = fullMessage.Split(':');
            
            int z = 0;
            
            string clientName = splitter[0];
            
            int identifier = 0;
            if ( splitter.Length > 1 && int.TryParse(splitter[1], out z))
                identifier = int.Parse(splitter[1]);
            
            int msgOrderNumber = 0;
            if (splitter.Length > 2 && int.TryParse(splitter[2], out z))
                msgOrderNumber = int.Parse(splitter[2]);
            
            
            switch (identifier)
            {
                case ClientToUdpHost.CONNECT:
                    bool playerExists = false;
                    foreach (Player player in playerList)
                    {
                        if (player.name == clientName)
                        {
                            playerExists = true;
                            Console.WriteLine("Player already exists: " + clientName);
                            SendMessageToUDPClient(UdpHostToClient.THE_NAME_IS_USED.ToString() + ':' + "the name already exists on server", clientEndPoint, socket);
                            break;
                        }
                    }
                    if (!playerExists)
                    {
                        playerList.Add(new Player(clientEndPoint, clientName));
                        Console.WriteLine("Added player to list: " + clientName);
                        SendMessageToUDPClient(UdpHostToClient.CLIENT_CONNECTED.ToString() + ':' + "you connected to server", clientEndPoint, socket);
                    }

                    clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    break;
                
                case ClientToUdpHost.DISCONNECT:
                    Console.WriteLine("Removing : " + clientName);
                    playerList.RemoveAll(player => player.name == clientName);
                    break;

                default: break;
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
    
    public void MessageRecevingWithoutThreads()
    {
        MessageProcessingFromSocket(udpSocket);
    }
    #endregion


}