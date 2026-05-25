using NAudio.Wave;
using System.Reflection;
using Vortice.Win32;

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

                PlaySoundInternal(volume, name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Play Sound File Exception: " + ex);
            }
        }

        /// <summary>
        /// plays a sound from resources, provide file extension. file name cannot contain spaces, add traliing "." to the folder.
        /// </summary>
        public static void PlaySoundFileEmbedded(string name, string folder, float volume)
        {
            try
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(folder))
                    return;

                name = name.Trim();
                name = name.Trim('"');
                name = name.TrimEnd(',');
                name = name.Replace(@"\\", @"\");
                name = name.Replace(" ", "");

                Assembly asm = Assembly.GetExecutingAssembly();

                Stream stream = asm.GetManifestResourceStream("Titled_Gui.Resources.sounds." + folder + name) ?? throw new Exception("Sound was not found: " + name);

                byte[] sound = new byte[stream.Length];
                stream.Read(sound, 0, sound.Length);

                string cachePath = Path.Combine(Configs.titledDocumentsFolder, "Cache", "Sounds");
                string filePath = Path.Combine(Configs.titledDocumentsFolder, "Cache", "Sounds", name);

                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);

                if (!File.Exists(filePath))
                    File.WriteAllBytes(filePath, sound);

                PlaySoundInternal(volume, filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Play Sound File Exception: " + ex);
            }
        }

        private static void PlaySoundInternal(float volume, string filePath)
        {
            AudioFileReader file = new(filePath.Trim());
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

            PlaySoundInternal(volume, name);
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

                PlaySoundInternal(volume, name);
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
