using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

public class Account
{
    //Login properties
    public string login = "123";
    public string password = "123";
    private bool isConnected = false;

    private TcpClient clientCopy;
    
    public Account(string login, string password)
    {
        this.login = login;
        this.password = password;
    }


    public bool isLoginValid(string login, string password, TcpClient client)
    {
        if (isConnected)
        {
            return false;
        }


        if (login != this.login)
        {
            return false;
        }


        if (password != this.password)
        {
            return false;
        }

        clientCopy = client;
        isConnected = true;
        return true;
    }

    public TcpClient GetTcpClient()
    {
        return clientCopy;
    }
    public string getLogin()
    {
        return login;
    }

    public bool GetIsConnected()
    {
        return isConnected;
    }
    public bool Disconnect(TcpClient client)
    {
        if (client == clientCopy)
        {
            clientCopy = null;
            isConnected = false;
            return true;
        }

        return false;
    }

    public bool PasswordIsValid(string password)
    {
        return (this.password == password);
    }
}



