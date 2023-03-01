// using System;
// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
// using System.Numerics;
// using System.Text;
// using System.Threading;
// using static System.Runtime.InteropServices.JavaScript.JSType;
// public struct UdpHostToClient
// {
//     public const int CLIENT_CONNECTED = 1;
//     public const int THE_NAME_IS_USED = 2;
//     public const int ADD_PLAYER_TO_SCREEN = 3;
//     public const int SEND_PLAYERDEST_TO_CLIENT = 4;
//     public const int SEND_PLAYER_POS_DEST_TO_CLIENT = 5;
//
// }
//
// public struct ClientToUdpHost
// {
//     public const int CONNECT = 1;
//     public const int DISCONNECT = 2;
//     public const int SEND_MY_DESTINATION = 3;
//     public const int SEND_MY_POS_AND_DEST = 4;
//     
// }
// class Server
// {
//     #region Variables
//     //Common
//     private IPAddress serverIP = IPAddress.Parse("192.168.0.189");
//     int serverPort = 20001;
//     static int bufferSize = 1024;
//     protected readonly byte[] buffer = new byte[bufferSize];
//     byte[] bytesToSend;
//     EndPoint clientEndPoint;
//     Int32 recvBytes;
//     IPAddress clientIP;
//     //TCP Part
//     Thread tcpThread;
//     private const int MAX_PLAYERS = 100;
//     private TcpClient[] clients = new TcpClient[MAX_PLAYERS];
//     
//     //UDP Part
//     Thread udpThread;
//     private Socket udpSocket;
//     
//     //Players
//     List<Player> playerList;
//
//     #endregion
//     
//     /// <summary>
//     ///Start is called before the first frame update
//     /// </summary>
//     public void Start()
//     {
//         Console.WriteLine("Server start");
//         // udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//         // udpSocket.Bind(new IPEndPoint(serverIP, serverPort));
//         
//         // playerList= new List<Player>();
//         // clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
//         
//         
//         
//         
//         startThreads();
//     }
//     #region Threads
//     /// <summary>
//     /// Function which calls all the existing threads to perform.
//     /// Each thread updates every frame.
//     /// @@@ Put it in infinite loop to run.
//     /// </summary>
//     public void startThreads()
//     {
//         udpThread = new Thread(new ThreadStart(ReceiveUDPDataSocket));
//         StartThreadOnBackground(udpThread);
//     }
//
//     public void runThreads()
//     {
//         //ReceiveUDPDataSocket();
//     }
//     /// <summary>
//     /// Setting the thread to background and starting it.
//     /// Save space to start the threads in future.
//     /// </summary>
//     /// <param name="xThread"></param>
//     public void StartThreadOnBackground(Thread xThread)
//     {
//         xThread.IsBackground = true;
//         xThread.Start();
//     }
//     #endregion
//
//     
//     #region Functions to be called in threads.
//     /// <summary>
//     /// Thread with receving messages from udp socket number 1
//     /// </summary>
//     public void ReceiveUDPDataSocket()
//     {
//         MessageProcessingFromSocket(udpSocket);
//     }
//
//     public void MessageProcessingFromSocket(Socket socket)
//     {
//         recvBytes = socket.ReceiveFrom(buffer, ref clientEndPoint);
//         if (recvBytes != 0)
//         {
//             
//             string fullMessage = System.Text.Encoding.ASCII.GetString(buffer, 0, recvBytes);
//             clientIP = ((IPEndPoint)clientEndPoint).Address;
//             Console.WriteLine(clientIP + " : " + fullMessage);
//             
//             string[] splitter = fullMessage.Split(':');
//             
//             int z = 0;
//             
//             string clientName = splitter[0];
//             
//             int identifier = 0;
//             if ( splitter.Length > 1 && int.TryParse(splitter[1], out z))
//                 identifier = int.Parse(splitter[1]);
//
//             switch (identifier)
//             {
//                 case ClientToUdpHost.CONNECT:
//                     bool playerExists = false;
//                     foreach (Player player in playerList)
//                     {
//                         if (player.name == clientName)
//                         {
//                             playerExists = true;
//                             Console.WriteLine("Player already exists: " + clientName);
//                             SendMessageToUDPClient(UdpHostToClient.THE_NAME_IS_USED.ToString() + ':' + "the name already exists on server", clientEndPoint, socket);
//                             break;
//                         }
//                     }
//                     if (!playerExists)
//                     {
//                         Player newPlayer = new Player(clientEndPoint, clientName, splitter[2]);
//                         playerList.Add(newPlayer);
//                         Console.WriteLine("Added player to list: " + clientName);
//                         SendMessageToUDPClient(UdpHostToClient.CLIENT_CONNECTED.ToString() + ':' + "you connected to server", clientEndPoint, socket);
//                         foreach (Player player in playerList)
//                         {
//                             if (player.name != clientName)
//                             {
//                                 //messages to all other connected players
//                                 SendMessageToUDPClient(UdpHostToClient.ADD_PLAYER_TO_SCREEN.ToString() + ':' + clientName + ':' + newPlayer.currentXYZ,
//                                     player.playerEndPoint, socket);
//                             }
//                         }
//                     }
//
//                     clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
//                     break;
//                 
//                 case ClientToUdpHost.DISCONNECT:
//                     Console.WriteLine("Removing : " + clientName);
//                     playerList.RemoveAll(player => player.name == clientName);
//                     break;
//                 
//                 case ClientToUdpHost.SEND_MY_DESTINATION:
//                     foreach (Player player in playerList)
//                     {
//                        // if (player.name != clientName)
//                             SendMessageToUDPClient(UdpHostToClient.SEND_PLAYERDEST_TO_CLIENT.ToString() + ':' + clientName + ':' + splitter[2]
//                                                 , player.playerEndPoint, socket);
//                     }
//                     break;
//                 
//                 case ClientToUdpHost.SEND_MY_POS_AND_DEST:
//                     foreach (Player player in playerList)
//                     {
//                         if (player.name != clientName)
//                             SendMessageToUDPClient(UdpHostToClient.SEND_PLAYER_POS_DEST_TO_CLIENT.ToString() + ':' + clientName + ':' + splitter[2] + ':' + splitter[3]
//                                 , player.playerEndPoint, socket);
//                     }
//                     break;
//
//                 
//
//                 default: break;
//             }
//         }
//     }
//     /// <summary>
//     /// Sending message to the clients with UDP protocol
//     /// </summary>
//     /// <param name="message"></param>
//     /// <param name="client"></param>
//     public void SendMessageToUDPClient(string message, EndPoint client, Socket socket)
//     {
//         bytesToSend = System.Text.Encoding.ASCII.GetBytes(message);
//         socket.SendTo(bytesToSend, client);
//     }
//     #endregion
//
//
// }