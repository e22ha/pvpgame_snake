using System;

namespace console_snake
{
    class Program
    {
        static void Main(string[] args)
        {
            Sounds sounds = new();
            sounds.LoadTypewriterSound();
            Console.ReadKey();
        }
    }
}
