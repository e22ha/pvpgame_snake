﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace console_snake
{
    class Program
    {
        static readonly int x = 80;
        static readonly int y = 26;
        static string ip = "127.0.0.1";
        static int port = 8888;

        static char food_ch = '@';
        static public Point food { get; private set; }

        static Walls walls;

        static Timer time;

        static string guid;

        const string parameterForSecondProcess = "--secondary";
        static public Client client = new Client();

        static int Main(string[] args)
        {
            client.DataForUpdate += Client_DataForUpdate;
            if (args.FirstOrDefault() == parameterForSecondProcess)
                return RunClient(args);
            else
                return RunField(args);
        }

        private static void Client_DataForUpdate(object sender, EventArgs e)
        {
            Console.Clear();
            Console.CursorVisible = false;

            walls = new Walls(x, y, '#');

            string[] data = sender.ToString().Split(".");

            //food//
            string[] p_x_y = data[1].Split(",");

            food = (int.Parse(p_x_y[0]), int.Parse(p_x_y[1]), food_ch);
            food.Draw();

            //snakes//
            for (int i = 2; i < data.Length; i++)
            {
                List<Point> points_of_snake = new();
                string[] string_points = data[i].Split("|");
                for (int j = 0; j < string_points.Length - 1; j++)
                {
                    string[] x_y = string_points[j].Split(",");
                    Point p = (int.Parse(x_y[0]), int.Parse(x_y[1]), '*');
                    points_of_snake.Add(p);
                }

                foreach (var item in points_of_snake)
                {
                    item.Draw();
                }
            }

        }

        static int RunField(string[] args)
        {
            try
            {
                client.altcons = AlternateConsole.CreateServer(StartClientProcess);
                client.altcons.SetTitle("Client");
                Console.Title = "Snake_game";
                Console.SetWindowSize(x + 1, y + 1);
                Console.SetBufferSize(x + 1, y + 1);
                Console.CursorVisible = false;



                Console.WriteLine("Are you read(y)?");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    client.altcons.WriteLine("Set & load client...");
                    client.altcons.WriteLine("IP?...");
                    ip = client.altcons.ReadLine();
                    if (ip == "") ip = "127.0.0.1";
                    client.altcons.WriteLine("Port?...");
                    string port_str = client.altcons.ReadLine();
                    if (port_str == "") port = 8888;
                    else port = int.Parse(port_str);
                    client.connect_(ip, port);
                    Guid g = Guid.NewGuid();
                    guid = g.ToString();
                    client.altcons.SetTitle("Client " + guid.Substring(0, 4));
                    Console.Title = "Snake_game " + guid.Substring(0, 4);
                    string ch = "#";
                    string slash = "/";
                    string msg = String.Concat(ch, guid, slash);
                    client.first(msg);
                    client.altcons.WriteLine(msg);
                    Console.Clear();
                    walls = new Walls(x, y, '#');


                    while (true)
                    {
                        if (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);

                            client.generate_msg(guid, key);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"ERROR: second console disconnected: {ex.Message}");
                return 1;
            }
            return 0;
        }

        static void StartClientProcess(string pipeToken)
        {
            var path = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(new ProcessStartInfo(path)
            {
                ArgumentList = { parameterForSecondProcess, pipeToken },
                UseShellExecute = true
            });
        }

        static int RunClient(string[] args)
        {
            if (args.Length != 2)
            {
                return 2;
            }
            try
            {
                AlternateConsole.RunClient(args[1]);
            }
            catch
            {
                return 2;
            }
            return 0;
        }
    }
}
