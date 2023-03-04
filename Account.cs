using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

public class Account
{
    //Login properties
    private string login = "123";
    private string password = "123";
    public bool isConnected = false;

    public TcpClient clientCopy;
    
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

    public string getLogin()
    {
        return login;
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



