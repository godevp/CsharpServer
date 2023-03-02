using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using CsharpServer;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Server
{
    #region Variables
    
    //Common
    private IPAddress serverIP = IPAddress.Parse("192.168.0.189");
    int serverPort = 20001;
    static int bufferSize = 1024;
    byte[] bytesToSend;
    EndPoint clientEndPoint;
    Int32 recvBytes;
    IPAddress clientIP;
    
    //UDP Part
    Thread udpThread;
    private Socket udpSocket;
    
    //TCP Part
    Thread tcpThread;
    public bool StartUpdateOfClients = false;
    private const int MAX_PLAYERS = 100;
    private TcpClient[] clients = new TcpClient[MAX_PLAYERS];
    private TcpListener tcpListener;
    
    //Players
    List<Player> playerList;

    #endregion
    
    /// <summary>
    ///Starts the server by initializing a TCP listener on a specified IP address and port.****Later can add UDP
    /// </summary>
    public void Start()
    {
        Console.WriteLine("Server start");
        tcpThread = new Thread(new ThreadStart(ListenForClients));
        tcpListener = new TcpListener(serverIP, serverPort);
        tcpListener.Start();
        playerList = new List<Player>();
        playerList.Add(new Player("123","123"));
        Console.WriteLine("Server started on {0}:{1}", serverIP.ToString(), serverPort);
    }
    /// <summary>
    /// Listens for incoming TCP client connections, accepts them, finds an available slot in the clients array, and starts a new thread to handle the client.
    /// </summary>
    public void ListenForClients()
    {
        // Wait for a client to connect
            TcpClient client = tcpListener.AcceptTcpClient();

            // Find an available slot in the clients array
            int playerIndex = -1;
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (clients[i] == null)
                {
                    playerIndex = i;
                    break;
                }
            }

            // If no available slot was found, disconnect the client and continue listening
            if (playerIndex == -1)
            {
                Console.WriteLine("Maximum number of players reached, disconnecting client {0}", client.Client.RemoteEndPoint);
                client.Close();
                return;
            }

            // Add the client to the clients array and start a new thread to handle the client
            clients[playerIndex] = client;
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
            clientThread.Start(playerIndex);
    }
    /// <summary>
    /// Handles the TCP client connection by reading data from the client, checking if the client has disconnected,
    /// converting the received data to a string, and passing the data to TCPMessageProcessing.HandleMessage() for processing.
    /// </summary>
    /// <param name="playerIndexObj"></param>
    private void HandleClient(object playerIndexObj)
    {
        int playerIndex = (int)playerIndexObj;
        TcpClient client = clients[playerIndex];
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[bufferSize];
        int bytesReceived;
        
        byte[] welcomeMessageBytes = Encoding.ASCII.GetBytes("Welcome to the MMO game server!");
        stream.Write(welcomeMessageBytes, 0, welcomeMessageBytes.Length);

        while (true)
        {
            try
            {
                // Receive data from the client
                bytesReceived = stream.Read(buffer, 0, buffer.Length);

                // If no data was received, the client has disconnected
                if (bytesReceived == 0)
                {
                    Console.WriteLine("Client {0} disconnected", client.Client.RemoteEndPoint);
                    
                    //need to say that the account isn't connected anymore
                    playerList.Any(player => player.Disconnect(client));
                    
                    clients[playerIndex] = null;
                    client.Close();
                    return;
                }
                
                // Convert the received data to a string
                string data = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                Console.WriteLine(data);
  
                string[] messageSplitter = data.Split(';');
                if (messageSplitter.Length > 1)
                {
                    string log = messageSplitter[0], pas = messageSplitter[1];
                    if (playerList.Any(player => (player.getLogin() == log && player.isConnected)))
                    {
                        //message processing part
                        List<TcpClient> clientsList = clients.ToList();
                        TCPMessageProcessing.HandleMessage(client, data, clientsList);
                    }
                    else
                    {
                        //login part
                        if (playerList.Any(player => player.isLoginValid(log,pas,client)))
                        {
                            byte[] successMessageBytes = Encoding.ASCII.GetBytes("Logged in successfully");
                            stream.Write(successMessageBytes, 0, successMessageBytes.Length);
                        }
                        else
                        {
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred while handling client {0}: {1}", client.Client.RemoteEndPoint, ex.Message);
                clients[playerIndex] = null;
                client.Close();
                return;
            }
        }
    }
}