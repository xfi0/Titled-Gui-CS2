using System.Text.Json;
using System.Text.Json.Nodes;

namespace Titled_Gui.Classes
{
    internal class UploadHelper
    {
        public static void SaveUpload(string fileName, string newPath)
        {
            string folder = Path.Combine(Configs.titledDocumentsFolder, "Uploads");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, fileName + ".json");

            JsonArray array;

            if (File.Exists(fullPath))
            {
                string existing = File.ReadAllText(fullPath);

                array = string.IsNullOrWhiteSpace(existing) ? new JsonArray() : JsonNode.Parse(existing)?.AsArray() ?? new();
            }
            else
            {
                array = new JsonArray();
            }

            array.Add(newPath);
            Console.WriteLine(array.Count);

            File.WriteAllText(fullPath, array.ToString());
        }

        private static readonly Dictionary<string, bool> hasLoaded = new();
        public static void LoadUploads(string fileName, ref List<string> items)
        {
            try
            {
                if (hasLoaded.ContainsKey(fileName))
                    return;

                string fileNameWithJson = fileName.Contains(".json") ? fileName : fileName + ".json";
                
                string folder = Path.Combine(Configs.titledDocumentsFolder, "Uploads");
                string filePath = Path.Combine(folder, fileNameWithJson);

                if (!Directory.Exists(folder))
                {
                    Console.WriteLine("Uploads Directory not found, creating.");
                    Directory.CreateDirectory(folder);
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    File.WriteAllText(filePath, "[]");
                    return;
                }

                var saved = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filePath));
                if (saved != null)
                {
                    var existing = items.ToHashSet();
                    foreach (var s in saved)
                        if (existing.Add(s))
                            items.Add(s);
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
