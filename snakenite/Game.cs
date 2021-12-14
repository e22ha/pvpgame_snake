using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace snakenite
{
    class Game
    {
        public static event EventHandler UpDraw;


        public struct Point
        {
            public int x { get; set; }
            public int y { get; set; }
            public char ch { get; set; }

            public static implicit operator Point((int, int, char) value) =>
                  new Point { x = value.Item1, y = value.Item2, ch = value.Item3 };

            public static bool operator ==(Point a, Point b) =>
                    (a.x == b.x && a.y == b.y) ? true : false;
            public static bool operator !=(Point a, Point b) =>
                    (a.x != b.x || a.y != b.y) ? true : false;


            public void Draw()
            {
                DrawPoint(ch);
            }
            public void Clear()
            {
                DrawPoint(' ');
            }

            private void DrawPoint(char _ch)
            {
                //Console.SetCursorPosition(x, y);
                //Console.Write(_ch);
                string message = x + "," + y + ","+_ch;
                UpDraw?.Invoke(message, null);
            }
        }

        public enum Direction
        {
            LEFT,
            RIGHT,
            UP,
            DOWN
        }
    }
}

