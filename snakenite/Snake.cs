using System.Collections.Generic;

namespace WpfApp1
{
    public class Snake
    {
        private readonly Queue<Coords> _tail;
        private readonly Food _food;
        private readonly Game _game;

        private Coords _head;
        public int _length;
        public bool Died { get; private set; }
        public Coords Head
        {
            get => _head;
            private set
            {
                _head = value;
                _game.Arena[value.Y][value.X].State = CellState.Snake;
            }
        }

        public Snake(Game game, Food food, Coords head, int length, Direction direction)
        {
            _game = game;
            _food = food;
            _tail = new Queue<Coords>();
            Head = head;
            _length = length;
            while (_tail.Count < _length)
                Move(direction);
        }

        public void Move(Direction direction)
        {
            Coords coords = Head;
            switch (direction)
            {
                case Direction.Right:
                    coords = new Coords(coords.X + 1, coords.Y);
                    break;
                case Direction.Down:
                    coords = new Coords(coords.X, coords.Y + 1);
                    break;
                case Direction.Left:
                    coords = new Coords(coords.X - 1, coords.Y);
                    break;
                case Direction.Up:
                    coords = new Coords(coords.X, coords.Y - 1);
                    break;
            }
            if (!CheckMove(coords))
                return;
            _tail.Enqueue(Head);
            Head = coords;

            while (_tail.Count > _length)
            {
                Coords tail = _tail.Dequeue();
                _game.Arena[tail.Y][tail.X].State = CellState.Empty;
            }
        }

        private bool CheckMove(Coords coords)
        {
            if (coords.X >= _game.Arena[0].Count || coords.X < 0 || coords.Y >= _game.Arena.Count || coords.Y < 0 || _game.Arena[coords.Y][coords.X].State == CellState.Snake)
                Died = true;
            else
            if (_game.Arena[coords.Y][coords.X].State == CellState.Food)
            {
                _food.FoodCount--;
                _length++;
                _game.GiveScore();
            }
            return !Died;
        }
    }
}
