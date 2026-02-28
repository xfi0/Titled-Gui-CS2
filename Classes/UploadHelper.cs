using System.Text.Json.Nodes;

namespace Titled_Gui.Classes
{
    internal class UploadHelper
    {
        public static async Task SaveUpload(string fileName, string newPath)
        {
            string folder = Path.Combine(AppContext.BaseDirectory, "Uploads");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, fileName + ".json");

            JsonArray array;

            if (File.Exists(fullPath))
            {
                string existing = await File.ReadAllTextAsync(fullPath);
                array = JsonNode.Parse(existing)?.AsArray() ?? new();
            }
            else
            {
                array = new JsonArray();
            }

            array.Add(newPath);

            await File.WriteAllTextAsync(fullPath, array.ToString());
        }

        private static readonly Dictionary<string, bool> hasLoaded = new();
        public static void LoadUploads(string fileName, ref List<string> items)
        {
            try
            {
                if (hasLoaded.ContainsKey(fileName))
                    return;

                string folder = Path.Combine(AppContext.BaseDirectory, "Uploads");
                string fileNameWithJson = fileName.Contains(".json") ? fileName : fileName + ".json";

                string filePath = Path.Combine(folder, fileNameWithJson);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                HashSet<string> stuff = new();

                foreach (string line in File.ReadAllLines(filePath))
                {
                    if (line == "[" || line == "]"||string.IsNullOrWhiteSpace(line))
                        continue;
                    if (stuff.Add(line.Replace(",", "").Replace(@"""", "")))
                    {
                        items.Add(line.Replace(",", "").Replace(@"""", "")); // only add the unique lines
                    }
                }
                hasLoaded.Add(fileName, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Load Uploads Exception: " + ex);
            }

            return;
        }
    }
}
