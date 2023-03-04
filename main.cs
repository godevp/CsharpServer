using System;

class MyMain
{
    static public void Main(String[] args)
    {
        Server server = new Server();
        server.Start();

        while (true)
        {
            server.check = true;
            server.ListenForClients();
        }
    }
}