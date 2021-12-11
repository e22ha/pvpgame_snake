using System.Threading;
using System.Media;
using System;
using System.IO;
using System.Reflection;

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
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(appDir, @".wav/icon_06.wav");
            player.SoundLocation = fullPath;
            player.Play();
        }

    }
}
