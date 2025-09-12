using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titled_Gui.Modules.Legit
{
    internal class HitSound : Classes.ThreadService
    {
        public static void PlaySound(string SoundPath)
        {
            if (string.IsNullOrEmpty(SoundPath)) return;

            try
            {
                using var file = new AudioFileReader("..\\..\\..\\..\\Resources\\Cream.wav");
                using var player = new WaveOutEvent();
                //if (player.PlaybackState != PlaybackState.Playing)
                //{
                player.Init(file);
                player.Volume = 0.3f;
                player.Play();
                //}
                if (player.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep((int)file.Length);
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        protected override void FrameAction()
        {
            //PlaySound("a");
        }
    }
}
