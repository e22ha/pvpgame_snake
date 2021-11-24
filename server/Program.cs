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
        public static Server server = new(ip, port);
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            LoadServer();
            if (Console.ReadLine() == "q")
            {
                server._stop();
            }

        }

        private static void LoadServer()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Set & load server...");

            Console.WriteLine("IP?...");
            ip = Console.ReadLine();

            Console.WriteLine("Port?...");
            port = Int32.Parse(Console.ReadLine());

            server = new(ip, port);
            server._start();
        }
    }
}
