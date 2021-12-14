using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace WpfApp1
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private int _score;
        private int _highScore;
        private List<List<Cell>> _arena;
        private Game _game;
        private bool _gameRunning;
        private bool _gameOver;
        private ICommand _moveCommand;
        private ICommand _startCommand;

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
            }
        }

        public int HighScore
        {
            get => _highScore;
            set
            {
                _highScore = value;
                OnPropertyChanged();
            }
        }

        public List<List<Cell>> Arena
        {
            get => _arena;
            set
            {
                _arena = value;
                OnPropertyChanged();
            }
        }

        public bool GameRunning
        {
            get => _gameRunning;
            set
            {
                _gameRunning = value;
                OnPropertyChanged();
            }
        }

        public bool GameOver
        {
            get => _gameOver;
            set
            {
                _gameOver = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartCommand => _startCommand ??= new RelayCommand(parameter =>
        {
            if (!GameRunning)
            {
                if (GameOver)
                    NewGame();
                else
                {
                    GameRunning = true;
                    _game.Start();
                }
            }
            else
            {
                GameRunning = false;
                _game.Stop();
            }
        });

        public ICommand MoveCommand => _moveCommand ??= new RelayCommand(parameter =>
        {
            if (GameRunning && Enum.TryParse(parameter.ToString(), out Direction direction))
            {
                _game.Direction = direction;
            }
        });

        public void EndGame()
        {
            GameRunning = false;
            GameOver = true;
            if (HighScore < Score)
                HighScore = Score;
        }

        private void NewGame()
        {
            if (GameRunning)
                _game.Stop();
            GameOver = false;
            Score = 0;
            _game = new Game(this);
            Arena = _game.Arena;
        }

        public MainViewModel()
        {
            NewGame();
        }
    }  
}
