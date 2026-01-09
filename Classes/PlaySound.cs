using NAudio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace Titled_Gui.Classes
{
    internal class PlaySound
    {
        /// <summary>
        /// plays a sound from resources, DO NOT INCLUDE FILE EXSTENTION
        /// </summary>
        public static void PlaySoundFile(string name, float volume)
        {
            if (string.IsNullOrEmpty(name)) return;
            string path = Path.Combine("..", "..", "..", "..", "Resources", $"{name.Trim()}.wav");

            AudioFileReader file = new(path);
            WaveOutEvent player = new();
            player.Init(file);
            player.Volume = volume;
            player.Play();

            player.PlaybackStopped += (s, e) =>
            {
                file.Dispose();
                player.Dispose();
            };
        }
    }
}
