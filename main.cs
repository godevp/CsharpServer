using System;
using System.Threading;
class MyMain
{
    private static Thread thread1 = new Thread(new ThreadStart(RunListeningForClients));
    private static Thread thread2 = new Thread(new ThreadStart(CombatUpdate));
    private static Server server = new Server();
    static public void Main(String[] args)
    {
        server.Start();
        thread1.Start();
        thread2.Start();
    }

    private static void RunListeningForClients()
    {
        while (true)
        {
            server.ListenForClients();
        } 
    }
    private static void CombatUpdate()
    {
        while (true)
        {
            server.CombatSystem();
        } 
    }
}