using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class Game
    {
        private const int _delay = 300;

        private readonly MainViewModel _viewModel;
        private readonly Snake _snake;
        private readonly Food _food;
        private Direction _direction;
        private CancellationTokenSource _cts = null;
        private bool _addDelay;

        public List<List<Cell>> Arena { get; }

        public Direction Direction
        {
            get => _direction;
            set
            {
                if (value != _direction && (int)value % 2 != (int)_direction % 2)
                {
                    _direction = value;
                    _addDelay = true;
                    Update();
                }
            }
        }

        public Game(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            int width = 40;
            int height = 30;
            Arena = new List<List<Cell>>();
            for (int i = 0; i < height; i++)
            {
                List<Cell> row = new List<Cell>();
                for (int j = 0; j < width; j++)
                {
                    row.Add(new Cell());
                }
                Arena.Add(row);
            }
            _food = new Food(Arena, 10, 2);
            _snake = new Snake(this, _food, new Coords(Arena[0].Count / 2, Arena.Count / 2), 1, Direction.Right);
        }

        public void Start()
        {
            if (_cts == null)
                Run();
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private async void Run()
        {
            using (_cts = new CancellationTokenSource())
            {
                try
                {
                    while (true)
                    {
                        if (_snake.Died)
                        {
                            _viewModel.EndGame();
                            break;
                        }
                        else
                            Update();

                        await Task.Delay(_delay, _cts.Token);
                        if (_addDelay)
                        {
                            _addDelay = false;
                            await Task.Delay(_delay / 2, _cts.Token);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            _cts = null;

        }

        public void GiveScore()
        {
            _viewModel.Score += 10;
        }

        public void Update()
        {
            _snake.Move(Direction);
            _food.Update();
        }
    }
}
