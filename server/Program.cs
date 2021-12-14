using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace server
{
    class Program
    {
        static string ip = "127.0.0.1";
        static int port = 8888;
        static void Main(string[] args)
        {
            Server server = new(ip, port);
            Console.WriteLine("Hello World!");
            LoadServer(server);
            if (Console.ReadLine() == "q")
            {
                server.stop(server.Room);
            }

        }
        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private static void LoadServer(Server server)
        {
            Game game = new();
            Console.WriteLine(FreeTcpPort().ToString());
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Set & load server...");

            Console.WriteLine("IP?...");
            ip = Console.ReadLine();
            if (ip == "") ip = "127.0.0.1";


            Console.WriteLine("Port?...");
            string port_str = Console.ReadLine();
            if (port_str == "") port = 8888;
            else port = int.Parse(port_str);

            server = new(ip, port);
            server.start();

            server.AddNewPlayer += game.AddPlayer;
            server.UpdateDirection += game.UpdateDir;
            server.AllPlayerReady += game.gStart;
            game.nextFrame += server.sendField;
            game.foodEat += server.sendSound_eat;

            //while (true)
            //{
            //    if (game.snakes.Count > 0) break;
            //}

            //game.gStart();

        }
    }
}
