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
                server.stop();
            }

        }

        private static void LoadServer(Server server)
        {
            Game game = new();
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
            game.nextFrame += server.sendField;

            while (true)
            {
                if (game.snakes.Count > 1) break;
            }

            game.gStart();

        }
    }
}
