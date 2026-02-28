using NAudio.Wave;

namespace Titled_Gui.Classes
{
    internal class PlaySound
    {
        /// <summary>
        /// plays a sound from resources, provide file extension
        /// </summary>
        public static void PlaySoundFile(string name, float volume)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return;

                string path = Path.Combine(AppContext.BaseDirectory, "Resources", $"{name.Trim()}");

                if (!File.Exists(path))
                    Console.WriteLine($"File not found: {path}");

                AudioFileReader file = new(path.Trim());
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
            catch (Exception ex)
            {
                Console.WriteLine("Play Sound File Exception: " + ex);
            }
        }

        /// <summary>
        /// plays a sound from resources, provide file extension. use a full path
        /// </summary>
        public static void PlaySoundFileNonRelative(string name, float volume)
        {
            if (string.IsNullOrEmpty(name)) 
                return;


            if (!File.Exists(name.Trim()))
            {
                Console.WriteLine($"File not found: {name.Trim()}");
                return;
            }

            AudioFileReader file = new(name.Trim());
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

        public static void PlaySoundWithCheck(string name, float volume)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return;

                name = name.Trim();
                name = name.Trim('"');
                name = name.TrimEnd(',');
                name = name.Replace(@"\\", @"\");
                string path = CheckIfRelative(name, "Resources")
                    ? name
                    : Path.Combine(AppContext.BaseDirectory, "Resources", name.Replace(" ", "") + ".wav");

                path = path.Trim('"');
                if (!File.Exists(path))
                {
                    Console.WriteLine($"File not found: {path} \n");
                    return;
                }

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
            catch (Exception ex)
            {
                Console.WriteLine("Play Sound File With Check Exception: " + ex);
            }
        }

        private static bool CheckIfRelative(string fileName, string relativeFolderToCheck)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, relativeFolderToCheck)))
            {
                Console.WriteLine("Directory Does Not Exist: " +
                                  Path.Combine(AppContext.BaseDirectory, relativeFolderToCheck));
                return false;
            }

      
            return !File.Exists(Path.Combine(AppContext.BaseDirectory, relativeFolderToCheck, fileName.Replace(" ", "") + ".wav"));
        } 
    }
}
