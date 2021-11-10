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
        public void LoadTypewriterSound()
        {
            Thread backgroundSound = new Thread(new ThreadStart(PlayKey));
            backgroundSound.IsBackground = true;
            backgroundSound.Start();
        }

        public void LoadCarriageReturn()
        {
            Thread backgroundSound = new Thread(new ThreadStart(PlayCarriageReturn));
            backgroundSound.IsBackground = true;
            backgroundSound.Start();
        }

        private static void PlayKey()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = @"D:\Music\Саунд дизайн\Daruma Audio – SFX Library (CC BY)\Keyboard\keyboard_01.wav";
            player.Play();
        }

        private static void PlayCarriageReturn()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = @"D:\Music\Саунд дизайн\Daruma Audio – SFX Library (CC BY)\Keyboard\keyboard_02.wav";
            player.PlaySync();
        }
    }
}
