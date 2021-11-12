using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace console_snake
{
    class Sounds
    {
        public void LoadEatSound()
        {
            Thread backgroundSound = new Thread(new ThreadStart(EatSound));
            backgroundSound.IsBackground = true;
            backgroundSound.Start();
        }

        private static void EatSound()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = @"D:\Code\PiOGI 2\pvpgame_snake\.wav\icon_06.wav";
            player.Play();
        }

    }
}
