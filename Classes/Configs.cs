using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Classes
{
    internal class Configs
    {
        public static string Name = "Titled";
        public static string Version = "1.0.2";
        public static string Author = "github.com/xfi0";
        public static string Link = "https://github.com/xfi0/Titled-Gui-CS2";

        public static string ConfigName = "";
        public static ConcurrentDictionary<string, bool> SavedConfigs = new();
        public static string SelectedConfig = "";

        public static readonly string ConfigDirPath = Path.Combine(AppContext.BaseDirectory, "Configs");
        public static string JsonString = "";

        public static void SaveConfig(string fileName)
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            JObject configData = new()
            {
                ["0"] = new JObject
                {
                    ["Name"] = Name,
                    ["Version"] = Version,
                    ["Author"] = Author,
                    ["Link"] = Link,
                },
                ["ESP"] = new JObject
                {
                    ["ESP Enabled"] = BoxESP.enableESP,
                    ["ESP Current Shape"] = BoxESP.CurrentShape,
                    ["ESP Rounding"] = BoxESP.Rounding,
                    ["ESP Outline"] = BoxESP.Outline,
                    ["ESP Team Check"] = BoxESP.TeamCheck,
                    ["ESP Draw On Self"] = BoxESP.DrawOnSelf,
                    ["Outline Thickness"] = new JObject
                    {
                        ["X"] = BoxESP.OutlineThickness.X,
                        ["Y"] = BoxESP.OutlineThickness.Y
                    },
                    ["ESP Opacity"] = BoxESP.BoxFillOpacity
                },
                ["Bone ESP"] = new JObject
                {
                    ["Bone ESP Enabled"] = BoneESP.EnableBoneESP,
                    ["Bone ESP Thickness"] = BoneESP.BoneThickness,
                    ["Bone ESP Team Check"] = BoneESP.TeamCheck,
                    ["Bone ESP Draw On Self"] = BoneESP.DrawOnSelf,
                },
                ["HealthBar"] = new JObject
                {
                    ["HealthBar Enabled"] = HealthBar.EnableHealthBar,
                    ["HealthBar Rounding"] = HealthBar.Rounding,
                    ["HealthBar Width"] = HealthBar.HealthBarWidth,
                },
                ["Aimbot"] = new JObject
                {
                    ["Aimbot Enabled"] = Aimbot.AimbotEnable,
                    ["Aimbot Aim On Team"] = Aimbot.Team,
                    ["Aimbot Selected Bone"] = Aimbot.CurrentBoneIndex,
                    ["Aimbot Aim Method"] = Aimbot.CurrentAimMethod,
                },
                ["RCS"] = new JObject
                {
                    ["RCS Enabled"] = RCS.Enabled,
                    ["RCS Strength"] = RCS.Strength
                },
                ["Trigger Bot"] = new JObject
                {
                    ["Trigger Bot Enabled"] = TriggerBot.Enabled,
                    ["Trigger Bot Delay"] = TriggerBot.Delay,
                    ["Trigger Bot Shoot At Team"] = TriggerBot.ShootAtTeam,
                    ["Trigger Bot Require Keybind"] = TriggerBot.RequireKeybind
                },
                ["Bhop"] = new JObject
                {
                    ["Bhop Enabled"] = Bhop.BhopEnable,
                    ["Bhop Keybind"] = Bhop.HopKey,
                },
                ["Eye Ray"] = new JObject
                {
                    ["Eye Ray Enabled"] = EyeRay.Enabled,
                    ["Eye Ray Length"] = EyeRay.Length,
                },
                ["Chams"] = new JObject
                {
                    ["Chams Enabled"] = Chams.EnableChams,

                }
            };

            Directory.CreateDirectory(ConfigDirPath);
            string fullPath = Path.Combine(ConfigDirPath, fileName);
            File.WriteAllText(fullPath, configData.ToString());

            Console.WriteLine($"Wrote {fileName} at {fullPath}");

            SavedConfigs.TryAdd(fileName, true);
        }

        public static void LoadConfig(string fileName)
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            string fullPath = Path.Combine(ConfigDirPath, fileName);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Config Not Found at {fullPath}");
                return;
            }

            JsonString = File.ReadAllText(fullPath);
            if (string.IsNullOrWhiteSpace(JsonString))
            {
                Console.WriteLine("Config is empty.");
                return;
            }

            JObject configData;
            try
            {
                configData = JObject.Parse(JsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                return;
            }

            #region MISC
            Name = configData["0"]?["Name"]?.ToString() ?? Name;
            Version = configData["0"]?["Version"]?.ToString() ?? Version;
            Author = configData["0"]?["Author"]?.ToString() ?? Author;
            Link = configData["0"]?["Link"]?.ToString() ?? Link;
            #endregion

            #region Shape ESP
            BoxESP.enableESP = configData["ESP"]?["ESP Enabled"]?.ToObject<bool>() ?? BoxESP.enableESP;
            BoxESP.Rounding = configData["ESP"]?["ESP Rounding"]?.ToObject<float>() ?? BoxESP.Rounding;
            BoxESP.Outline = configData["ESP"]?["ESP Outline"]?.ToObject<bool>() ?? BoxESP.Outline;
            BoxESP.TeamCheck = configData["ESP"]?["ESP Team Check"]?.ToObject<bool>() ?? BoxESP.TeamCheck;
            BoxESP.DrawOnSelf = configData["ESP"]?["ESP Draw On Self"]?.ToObject<bool>() ?? BoxESP.DrawOnSelf;
            BoxESP.BoxFillOpacity = configData["ESP"]?["ESP Opacity"]?.ToObject<float>() ?? BoxESP.BoxFillOpacity;
            BoxESP.CurrentShape = configData["ESP"]?["ESP Current Shape"]?.ToObject<int>() ?? BoxESP.CurrentShape;
            float outlineX = configData["ESP"]?["Outline Thickness"]?["X"]?.ToObject<float>() ?? BoxESP.OutlineThickness.X;
            float outlineY = configData["ESP"]?["Outline Thickness"]?["Y"]?.ToObject<float>() ?? BoxESP.OutlineThickness.Y;
            BoxESP.OutlineThickness = new Vector2(outlineX, outlineY);
            #endregion

            #region Bone ESP
            BoneESP.EnableBoneESP = configData["Bone ESP"]?["Bone ESP Enabled"]?.ToObject<bool>() ?? BoneESP.EnableBoneESP;
            BoneESP.BoneThickness = configData["Bone ESP"]?["Bone ESP Thickness"]?.ToObject<float>() ?? BoneESP.BoneThickness;
            BoneESP.TeamCheck = configData["Bone ESP"]?["Bone ESP Team Check"]?.ToObject<bool>() ?? BoneESP.TeamCheck;
            BoneESP.DrawOnSelf = configData["Bone ESP"]?["Bone ESP Draw On Self"]?.ToObject<bool>() ?? BoneESP.DrawOnSelf;
            #endregion

            #region HealthBar
            HealthBar.EnableHealthBar = configData["HealthBar"]?["HealthBar Enabled"]?.ToObject<bool>() ?? HealthBar.EnableHealthBar;
            HealthBar.Rounding = configData["HealthBar"]?["HealthBar Rounding"]?.ToObject<float>() ?? HealthBar.Rounding;
            HealthBar.HealthBarWidth = configData["HealthBar"]?["HealthBar Width"]?.ToObject<float>() ?? HealthBar.HealthBarWidth;
            #endregion

            #region Aimbot
            Aimbot.AimbotEnable = configData["Aimbot"]?["Aimbot Enabled"]?.ToObject<bool>() ?? Aimbot.AimbotEnable;
            Aimbot.Team = configData["Aimbot"]?["Aimbot Aim On Team"]?.ToObject<bool>() ?? Aimbot.Team;
            Aimbot.CurrentBoneIndex = configData["Aimbot"]?["Aimbot Selected Bone"]?.ToObject<int>() ?? Aimbot.CurrentBoneIndex;
            Aimbot.CurrentAimMethod = configData["Aimbot"]?["Aimbot Aim Method"]?.ToObject<int>() ?? Aimbot.CurrentAimMethod;
            #endregion

            #region RCS
            RCS.Enabled = configData["RCS"]?["RCS Enabled"]?.ToObject<bool>() ?? RCS.Enabled;
            RCS.Strength = configData["RCS"]?["RCS Strength"]?.ToObject<float>() ?? RCS.Strength;
            #endregion

            #region Trigger Bot
            TriggerBot.Enabled = configData["Trigger Bot"]?["Trigger Bot Enabled"]?.ToObject<bool>() ?? TriggerBot.Enabled;
            TriggerBot.Delay = configData["Trigger Bot"]?["Trigger Bot Delay"]?.ToObject<int>() ?? TriggerBot.Delay;
            TriggerBot.ShootAtTeam = configData["Trigger Bot"]?["Trigger Bot Shoot At Team"]?.ToObject<bool>() ?? TriggerBot.ShootAtTeam;
            TriggerBot.RequireKeybind = configData["Trigger Bot"]?["Trigger Bot Require Keybind"]?.ToObject<bool>() ?? TriggerBot.RequireKeybind;
            #endregion

            #region Bhop
            Bhop.BhopEnable = configData["Bhop"]?["Bhop Enabled"]?.ToObject<bool>() ?? Bhop.BhopEnable;
            Bhop.HopKey = configData["Bhop"]?["Bhop Keybind"]?.ToObject<int>() ?? Bhop.HopKey;
            #endregion
        }
    }
}
