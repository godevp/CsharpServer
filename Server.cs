using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Server
{
    #region Variables
    Socket UDPServerSocket;
    string localIP = "192.168.0.156";
    int localPort = 20001;
    static int bufferSize = 1024;
    private readonly byte[] buffer = new byte[bufferSize];
    byte[] bytesToSend;
    EndPoint clientEndPoint;
    Int32 recvBytes;
    IPAddress clientIP;
    Thread UpdateThread;
    Thread udpReceivingThread;
    #endregion
    /// <summary>
    ///Start is called before the first frame update
    /// </summary>
    public void Start()
    {
        Console.WriteLine("Server start");
        UDPServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        UDPServerSocket.Bind(new IPEndPoint(IPAddress.Parse(localIP), localPort));
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
        UpdateThread = new Thread(new ThreadStart(Update));
        StartThreadOnBackground(UpdateThread);
        udpReceivingThread = new Thread(new ThreadStart(ReceiveUDPData));
        StartThreadOnBackground(udpReceivingThread);
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
    /// Different calculations on server can be imported here.
    /// </summary>
    public void Update()
    {
       
    }
    /// <summary>
    /// Receiving message from clients with UDP protocol.
    /// </summary>
    private void ReceiveUDPData()
    {
        recvBytes = UDPServerSocket.ReceiveFrom(buffer, ref clientEndPoint);
        if(recvBytes != 0)
        {
            string message = System.Text.Encoding.ASCII.GetString(buffer, 0, recvBytes);
            clientIP = ((IPEndPoint)clientEndPoint).Address;
            Console.WriteLine(clientIP + " : " + message);
            SendMessageToUDPClient("Connected to UDP Server", clientEndPoint);
        }  
    }
    /// <summary>
    /// Sending message to the clients with UDP protocol
    /// </summary>
    /// <param name="message"></param>
    /// <param name="client"></param>
    public void SendMessageToUDPClient(string message, EndPoint client)
    {
        bytesToSend = System.Text.Encoding.ASCII.GetBytes(message);
        UDPServerSocket.SendTo(bytesToSend, client);
    }
    #endregion


}