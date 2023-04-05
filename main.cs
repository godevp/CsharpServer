using System;
using System.Threading;
class MyMain
{
    private static Thread thread1 = new Thread(new ThreadStart(RunListeningForClients));

    private static Server server = new Server();
    static public void Main(String[] args)
    {
        server.Start();
        thread1.Start();
    }

    private static void RunListeningForClients()
    {
        
        while (true)
        {
            //Console.WriteLine(thread1.IsAlive);
            server.ListenForClients();
        } 
    }

}