using System;
using System.Collections.Generic;

namespace WpfApp1
{
    public class Food
    {
        private readonly int _foodDelay;
        private readonly int _maxFood;
        private readonly Random _rnd;
        public readonly List<List<Cell>> _arena;

        private int tick;

        public int FoodCount { get; set; }

        public Food(List<List<Cell>> arena, int foodDelay, int maxFood)
        {
            _rnd = new Random();
            _arena = arena;
            _foodDelay = foodDelay;
            _maxFood = maxFood;
        }

        public void Update()
        {
            if (tick >= _foodDelay && FoodCount < _maxFood)
            {
                tick = 0;
                while (true)
                {
                    Coords coords = new Coords(_rnd.Next(_arena[0].Count), _rnd.Next(_arena.Count));
                    if (_arena[coords.Y][coords.X].State == CellState.Empty)
                    {
                        _arena[coords.Y][coords.X].State = CellState.Food;
                        FoodCount++;
                        break;
                    }
                }
            }
            else
                tick++;
        }
    }
}
