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
    public const int CANT_CREATE_CHARACTER = 5;
    public const int CHARACTER_CREATED = 6;
    public const int NEW_CHARACTER_JOINED_SERVER = 7;
    public const int YOUR_POSITION = 8;
}

public struct TCPClientToHost
{
    public const int DISCONNECT = 1;
    public const int REGISTRATION = 2;
    public const int LOGIN = 3;
    public const int SAVE_NEW_CHARACTER = 4;
    public const int DELETE_CHARACTER = 5;
    public const int CHARACTER_JOINING_WORLD = 6;
}
public class TCPMessageProcessing
{
    public static char separator = ':';
    
    /// <summary>
    /// The start of message processing, decides what will be next *mainMessageProcessing/login/registration
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="accountList"></param>
    /// <param name="clientsArray"></param>
    /// <param name="playerIndex"></param>
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
        var userAccount = accountList.Find(account => account.GetTcpClient() == sender);
        
        switch (identifier)
        {
            case TCPClientToHost.DISCONNECT:
            {
                accountList.Any(account => account.Disconnect(sender));
                clientsArray[playerIndex] = null;
                 break;
            }
            case TCPClientToHost.SAVE_NEW_CHARACTER:
            {
                int slot = (int.TryParse(splitter[2],out _)) ? int.Parse(splitter[2]) : 0;
                string nameForNewCharacter = splitter[3];
                

                if (accountList.Any(account => account.c1 == nameForNewCharacter || account.c2 == nameForNewCharacter))
                {
                    //send message that this name is taken
                    string[] messageElements = { TCPHostToClient.CANT_CREATE_CHARACTER.ToString(), "The name is taken"};
                    SendTCPMessage(CombineWithSeparator(messageElements,separator.ToString()),sender);
                    break;
                }
                switch (slot)
                {
                    case 1:
                    {
                        userAccount.c1 = nameForNewCharacter;
                        break;
                    }
                    case 2:
                    {
                        userAccount.c2 = nameForNewCharacter;
                        break;
                    }
                }
                AccountJSONScript.Save(new ListOfAccounts { accounts = accountList });
                string[] elements = { TCPHostToClient.CHARACTER_CREATED.ToString(), slot.ToString(), nameForNewCharacter ,"Character created"};
                SendTCPMessage(CombineWithSeparator(elements,separator.ToString()),sender);
                break;
            }
            case TCPClientToHost.DELETE_CHARACTER:
            {
                int slot = (int.TryParse(splitter[2], out _)) ? int.Parse(splitter[2]) : 0;
                switch (slot)
                {
                    case 1:
                    {
                        userAccount.c1 = "";
                        break;
                    }
                    case 2:
                    {
                        userAccount.c2 = "";
                        break;
                    }
                    
                    default: break;
                }
                AccountJSONScript.Save(new ListOfAccounts { accounts = accountList });
                break;
            }
            case TCPClientToHost.CHARACTER_JOINING_WORLD:
            {
                int slot = (int.TryParse(splitter[2], out _)) ? int.Parse(splitter[2]) : 0;
                string posToSend = "";
                string destPosToSend = "";
                switch (slot)
                {
                    case 1:
                    {
                        userAccount.SetCharacterOnline(userAccount.c1);
                        posToSend = userAccount.c1Position;
                        destPosToSend = userAccount.c1DestPos;
                        break;
                    }
                    case 2:
                    {
                        userAccount.SetCharacterOnline(userAccount.c2);
                        posToSend = userAccount.c2Position;
                        destPosToSend = userAccount.c2DestPos;
                        break;
                    }
                    
                    default: break;
                }
                List<string> elementsList = new List<string>();
                elementsList.Add(TCPHostToClient.YOUR_POSITION.ToString());
                elementsList.Add(posToSend);
                
                //todo: update it so we send not only the position 
                string[] elements2 = {TCPHostToClient.NEW_CHARACTER_JOINED_SERVER.ToString(), userAccount.GetCharacterOnline(), posToSend,destPosToSend };
                SendTCPMessageToAllOtherClients(CombineWithSeparator(elements2,separator.ToString()),sender,clients);
                foreach (var acc in accountList)
                {
                    if (acc.GetTcpClient() != sender && acc.GetIsConnected())
                    {
                        elementsList.Add(TCPHostToClient.NEW_CHARACTER_JOINED_SERVER.ToString());
                        elementsList.Add(acc.GetCharacterOnline());
                        if (acc.c1 == acc.GetCharacterOnline())
                        {
                            elementsList.Add(acc.c1Position);
                            elementsList.Add(acc.c1DestPos);
                        }
                        if (acc.c2 == acc.GetCharacterOnline())
                        {
                            elementsList.Add(acc.c2Position);
                            elementsList.Add(acc.c2DestPos);
                        }
                    }
                }
                string [] elementsArray = elementsList.ToArray();
                SendTCPMessage(CombineWithSeparator(elementsArray,separator.ToString()),sender);
                break;
            }
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
            var acc = accountList.Find((account => account.login == log));
            string[] combinedElements = { TCPHostToClient.LOGGED_SUCCESSFULLY.ToString(), acc.c1, acc.c2 };
            byte[] successMessageBytes = Encoding.ASCII.GetBytes(CombineWithSeparator(combinedElements,separator.ToString()));
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
    /// <summary>
    /// Logic for registration new account.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="accountList"></param>
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
    /// Will combine all strings in 1 with a separator which you specify.
    /// </summary>
    /// <param name="variables"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string CombineWithSeparator(string[] variables, string separator)
    {
        return string.Join(separator, variables);
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