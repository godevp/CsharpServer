namespace CsharpServer;
using System;
using System.Net.Sockets;
using System.Text;
public struct TCPHostToClient
{
    public const int LOGGED_SUCCESSFULLY = 1;
    public const int LOGIN_DENIED = 2;
    public const int REGISTRATION_FAILED = 3;
    public const int REGISTRATION_APPROVED = 4;
}

public struct TCPClientToHost
{
    public const int DISCONNECT = 1;
    public const int REGISTRATION = 2;
    public const int LOGIN = 3;

}
public class TCPMessageProcessing
{
    public static void StartMessageProcessing(TcpClient sender, string message, List<Account> accountList, TcpClient[] clientsArray, int playerIndex)
    {
        string[] messageSplitter = message.Split(':');
        List<TcpClient> clients = clientsArray.ToList();
        
        if (messageSplitter.Length > 0)
        {
            string log = "";
            if (messageSplitter.Length > 1)
                log = messageSplitter[1];

            if (accountList.Any(account => (account.getLogin() == log && account.GetIsConnected() && account.GetTcpClient() == sender)))
            {
                //message processing part
                MessageProcessing(sender, message,accountList,clientsArray,playerIndex);
            }
            else
            {
                if (int.TryParse(messageSplitter[0], out _))
                {
                    int ident = int.Parse(messageSplitter[0]);
                    if (ident == TCPClientToHost.LOGIN)//check that the identifier is for login
                    {
                        //login part
                        LoginPart(sender,message,accountList);
                    }

                    if (ident == TCPClientToHost.REGISTRATION)
                    {
                        RegistrationPart(sender, message, accountList);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Gets the message from the sender and decides how to reply
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="accountList"></param>
    /// <param name="clientsArray"></param>
    /// <param name="playerIndex"></param>
    private static void MessageProcessing(TcpClient sender, string message,List<Account> accountList,TcpClient[] clientsArray, int playerIndex)
    {
        List<TcpClient> clients = clientsArray.ToList();
        NetworkStream senderStream = sender.GetStream();
        string[] splitter = message.Split(":");
        
        int identifier = 0;
        string userName = splitter[1];
        if (int.TryParse(splitter[0], out _))
        {
            identifier = int.Parse(splitter[0]);
        }

        switch (identifier)
        {
            case TCPClientToHost.DISCONNECT:
            {
                accountList.Any(account => account.Disconnect(sender));
                clientsArray[playerIndex] = null;
            }
                break; 


            default:
                break;
        }
    }

    /// <summary>
    /// The logic for login.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="accountList"></param>
    private static void LoginPart(TcpClient sender, string message, List<Account> accountList)
    {
        string[] messageSplitter = message.Split(':');
        
        string log = messageSplitter[1], pas = messageSplitter[2];
        if (accountList.Any(account => account.isLoginValid(log,pas,sender)))
        {
            byte[] successMessageBytes = Encoding.ASCII.GetBytes(TCPHostToClient.LOGGED_SUCCESSFULLY.ToString());
            var stream = sender.GetStream();
            stream.Write(successMessageBytes, 0, successMessageBytes.Length);
        }
        else
        {
            Account tempAccount;
            string errorMessage = "error";
            
            if (!(accountList.Any(account => account.getLogin() == log)))
            {
                //no login like that found in created accounts.
                errorMessage = "account with this login doesn't exist in our database.";
            }
            else
            {
                tempAccount = accountList.Find(account => account.getLogin() == log);
                Console.WriteLine("LoginExist");
                if(!tempAccount.PasswordIsValid(pas))
                {
                    //the password received for existing login is incorrect.
                    errorMessage = "the password is incorrect.";
                }
                else if(tempAccount.GetIsConnected())
                {
                    //the password received for existing login is incorrect.
                    errorMessage = "the account is in use.";
                }
            }
            byte[] errorMessageBytes = Encoding.ASCII.GetBytes(TCPHostToClient.LOGIN_DENIED.ToString() + ':' + errorMessage);
            var stream = sender.GetStream();
            stream.Write(errorMessageBytes, 0, errorMessageBytes.Length);
        }
    }

    private static void RegistrationPart(TcpClient sender, string message, List<Account> accountList)
    {
        string[] splitter = message.Split(':');
        string log = splitter[1], pas = splitter[2];

        if (accountList.Any(account => account.getLogin() == log))
        {
            SendTCPMessage(TCPHostToClient.REGISTRATION_FAILED.ToString() + ':' + "The username is taken",sender);
            return;
        }

        accountList.Add(new Account(log,pas));
        AccountJSONScript.Save(new ListOfAccounts { accounts = accountList });
        SendTCPMessage(TCPHostToClient.REGISTRATION_APPROVED.ToString() + ':' + log + ':' + pas,sender);
        Console.WriteLine("Registered new account, Username: " + log + " Password: " + pas );
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