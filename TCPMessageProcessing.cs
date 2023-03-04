namespace CsharpServer;
using System;
using System.Net.Sockets;
using System.Text;
public struct TCPHostToClient
{
    public const int LOGGED_SUCCESSFULLY = 1;
    public const int LOGIN_DENIED = 2;

}

public struct TCPClientToHost
{

    
}
public class TCPMessageProcessing
{
    public static void StartMessageProcessing(TcpClient sender, string message, List<TcpClient> clients, List<Account> accountList)
    {
        string[] messageSplitter = message.Split(':');
        if (messageSplitter.Length > 1)
        {
            string log = messageSplitter[0], pas = messageSplitter[1];
            if (accountList.Any(account => (account.getLogin() == log && account.isConnected && account.clientCopy == sender)))
            {
                //message processing part
                MessageProcessing(sender, message, clients);
            }
            else
            { 
                //login part
                LoginPart(sender,message,clients,accountList);
            }
        }
    }
    
    /// <summary>
    /// The logic for login.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="clients"></param>
    /// <param name="accountList"></param>
    private static void LoginPart(TcpClient sender, string message, List<TcpClient> clients, List<Account> accountList)
    {
        string[] messageSplitter = message.Split(':');
        string log = messageSplitter[0], pas = messageSplitter[1];
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
                errorMessage = "account with this login not exists in our database.";
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
                else if(tempAccount.isConnected)
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
    
    
    
    /// <summary>
    /// Gets the message from the sender and decides how to reply
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="clients"></param>
    private static void MessageProcessing(TcpClient sender, string message, List<TcpClient> clients)
    {
        NetworkStream senderStream = sender.GetStream();
        string response = "";
        string[] splitter = message.Split(":");
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