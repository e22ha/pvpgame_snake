using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace server
{
    class Game
    {
        static readonly int x = 45;
        static readonly int y = 45;

        static Walls walls;
        static FoodFactory foodFactory;
        static Timer time;
        public List<Snake> snakes = new();
        static Snake snake;

        //событие при получении нового игрока
        public void AddPlayer(object sender, EventArgs e)
        {
            string data = sender.ToString();
            Snake snake = new(snakes.Count + 1 * 10, snakes.Count + 1 * 5, 3, data);
            snakes.Add(snake);
        }

        //событие обновления позиции
        public void UpdateDir(object sender, EventArgs e)
        {
            string[] data = sender.ToString().Split(".");
            foreach (var s in snakes)
            {
                if (s.guid == data[2])
                {
                    //LeftArrow 37 Клавиша СТРЕЛКА ВЛЕВО.
                    //UpArrow 38 Клавиша СТРЕЛКА ВВЕРХ.
                    //RightArrow 39 Клавиша СТРЕЛКА ВПРАВО.
                    //DownArrow	40 Клавиша СТРЕЛКА ВНИЗ.
                    if (data[1] == "RightArrow")
                        s.Rotation(ConsoleKey.RightArrow);
                    else if (data[1] == "LeftArrow")
                        s.Rotation(ConsoleKey.LeftArrow);
                    else if (data[1] == "UpArrow")
                        s.Rotation(ConsoleKey.UpArrow);
                    else if (data[1] == "DownArrow")
                        s.Rotation(ConsoleKey.DownArrow);
                }
            }

        }


        public void gStart(object sender, EventArgs e)
        {
            foodFactory = new FoodFactory(x, y, '@');
            foodFactory.CreateFood();
            walls = new Walls(x, y, '#');

            time = new Timer(Loop, null, 0, 1000);
            

        }

        public event EventHandler nextFrame;
        public event EventHandler foodEat;
        public event EventHandler Lose;
        bool snakeEatSnake(Snake snake) 
        {
            foreach (Snake s in snakes) 
            {
                if (s == snake) break;
                if (s.IsHitS(snake.GetHead()))
                {
                    return true;
                }
            }

            return false;
        }
        void Loop(object obj)
        {

            foreach (var s in snakes)
            {
                if (walls.IsHit(s.GetHead()) || s.IsHit(s.GetHead()) || snakeEatSnake(s))//bool func for check all snake hit yourself & check all snake hit snake & check all snake hit walss
                {
                    time.Dispose();
                    Console.WriteLine("Game is out");
                    Lose?.Invoke(s.guid, null);
                }
                else if (s.Eat(foodFactory.food))
                {
                    foodFactory.CreateFood();
                    foodEat?.Invoke(s.guid, null);
                }
                else
                {
                    s.Move();
                }

            }

            List<object> l = new();
            l.Add(foodFactory.food.PointToString());
            foreach (var s in snakes)
            {
                string points = "";
                foreach (var point in s.Points)
                {
                    points = string.Concat(points, point.PointToString(), "|");
                }
                l.Add(points);
            }
            if (snakes.Count != 0) nextFrame?.Invoke(l, null);
        }
    }



    struct Point
    {
        public string PointToString()
        {
            string data = this.x + "," + this.y;
            return data;
        }
        public int x { get; set; }
        public int y { get; set; }
        public char ch { get; set; }

        public static implicit operator Point((int, int, char) value) =>
              new Point { x = value.Item1, y = value.Item2, ch = value.Item3 };

        public static bool operator ==(Point a, Point b) =>
                (a.x == b.x && a.y == b.y) ? true : false;
        public static bool operator !=(Point a, Point b) =>
                (a.x != b.x || a.y != b.y) ? true : false;
    }

    class Walls
    {
        private char ch;
        private List<Point> wall = new List<Point>();

        public Walls(int x, int y, char ch)
        {
            this.ch = ch;

            DrawHorizontal(x, 0);
            DrawHorizontal(x, y);
            DrawVertical(0, y);
            DrawVertical(x, y);
        }

        private void DrawHorizontal(int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                Point p = (i, y, ch);

                wall.Add(p);
            }
        }

        private void DrawVertical(int x, int y)
        {
            for (int i = 0; i < y; i++)
            {
                Point p = (x, i, ch);

                wall.Add(p);
            }
        }

        public bool IsHit(Point p)
        {
            foreach (var w in wall)
            {
                if (p == w)
                {
                    return true;
                }
            }
            return false;
        }
    }// class Walls

    enum Direction
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    class Snake
    {
        private List<Point> snake;

        private Direction direction;
        private int step = 1;
        private Point tail;
        private Point head;
        public string guid;


        bool rotate = true;

        public Snake(int x, int y, int length, string guid)
        {
            direction = Direction.RIGHT;

            snake = new List<Point>();
            for (int i = x - length; i < x; i++)
            {
                Point p = (i, y, '*');
                snake.Add(p);
            }

            this.guid = guid;
        }

        public List<Point> Points => snake;

        public Point GetHead() => snake.Last();

        public void Move()
        {
            head = GetNextPoint();
            snake.Add(head);

            tail = snake.First();
            snake.Remove(tail);




            rotate = true;
        }

        public bool Eat(Point p)
        {
            head = GetNextPoint();
            if (head == p)
            {
                snake.Add(head);
                return true;
            }
            return false;
        }

        public Point GetNextPoint()
        {
            Point p = GetHead();

            switch (direction)
            {
                case Direction.LEFT:
                    p.x -= step;
                    break;
                case Direction.RIGHT:
                    p.x += step;
                    break;
                case Direction.UP:
                    p.y -= step;
                    break;
                case Direction.DOWN:
                    p.y += step;
                    break;
            }
            return p;
        }

        public void Rotation(ConsoleKey key)
        {
            if (rotate)
            {
                switch (direction)
                {
                    case Direction.LEFT:
                    case Direction.RIGHT:
                        if (key == ConsoleKey.DownArrow)
                            direction = Direction.DOWN;
                        else if (key == ConsoleKey.UpArrow)
                            direction = Direction.UP;
                        break;
                    case Direction.UP:
                    case Direction.DOWN:
                        if (key == ConsoleKey.LeftArrow)
                            direction = Direction.LEFT;
                        else if (key == ConsoleKey.RightArrow)
                            direction = Direction.RIGHT;
                        break;
                }
                rotate = false;
            }

        }

        public bool IsHit(Point p)
        {
            for (int i = snake.Count - 2; i > 0; i--)
            {
                if (snake[i] == p)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsHitS(Point p)
        {
            for (int i = snake.Count - 1; i > 0; i--)
            {
                if (snake[i] == p)
                {
                    return true;
                }
            }
            return false;
        }
    }//class Snake

    class FoodFactory
    {
        int x;
        int y;
        char ch;
        public Point food { get; private set; }

        Random random = new Random();

        public FoodFactory(int x, int y, char ch)
        {
            this.x = x;
            this.y = y;
            this.ch = ch;
        }

        public Point CreateFood()
        {
            food = (random.Next(2, x - 2), random.Next(2, y - 2), ch);
            return food;
        }
    }
}

