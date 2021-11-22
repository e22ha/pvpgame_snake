using System;
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

        static Walls walls;
        static Snake snake;
        static Snake snake1;
        static FoodFactory foodFactory;
        static Timer time;
        const string parameterForSecondProcess = "--secondary";
        static public Client client = new Client();

        static int Main(string[] args)
        {
            if (args.FirstOrDefault() == parameterForSecondProcess)
                return RunClient(args);
            else
                return RunServer(args);
        }

        static int RunServer(string[] args)
        {
            try
            {
                client.altcons = AlternateConsole.CreateServer(StartClientProcess);
                client.altcons.SetTitle("Client");
                client.connect_();
                Console.Title = "Snake_game";
                Console.SetWindowSize(x + 1, y + 1);
                Console.SetBufferSize(x + 1, y + 1);
                Console.CursorVisible = false;


                Console.WriteLine("Are you read(y)?");
                if (Console.ReadKey().Key == ConsoleKey.Y ) {
                    Console.Clear();
                    walls = new Walls(x, y, '#');
                    snake = new Snake(x / 2, y / 2, 3);
                    snake1 = new Snake(x / 4, y / 3, 3);

                    foodFactory = new FoodFactory(x, y, '@');
                    foodFactory.CreateFood();

                    time = new Timer(Loop, null, 0, 200);
                    while (true)
                    {
                        if (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);

                            snake.Rotation(key.Key);
                            snake1.Rotation(ConsoleKey.RightArrow);
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
                return 1;
            }
            return 0;
        }



        static void Loop(object obj)
        {
            if (walls.IsHit(snake.GetHead()) || snake.IsHit(snake.GetHead())|| walls.IsHit(snake1.GetHead()) || snake1.IsHit(snake1.GetHead()) || snake.IsHit(snake1.GetHead()) || snake1.IsHit(snake.GetHead()))
            {
                time.Change(0, Timeout.Infinite);
            }
            else if (snake.Eat(foodFactory.food) || snake1.Eat(foodFactory.food))
            {
                foodFactory.CreateFood();
            }
            else
            {
                snake.Move();
                snake1.Move();
                client.generate_msg(snake,snake1);
            }
        }// Loop()
    }
}
