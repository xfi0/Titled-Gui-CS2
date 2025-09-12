using Newtonsoft.Json.Linq;
using System.Numerics;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Classes
{
    internal class Configs
    {
        public static string name = "Titled";
        public static string version = "1.0.2";
        public static string author = "github.com/xfi0";
        public static string link = "https://github.com/xfi0/Titled-Gui-CS2";
        public static string ConfigName = "";
        public static List<string> SavedConfigs = new List<string>();
        public static string SelectedConfig = "";
        public static string ConfigDirPath = "..\\..\\..\\..\\Configs";
        public static void SaveConfig(string FileName)
        {
            JObject configData = new()
            {
                ["0"] = new JObject
                {
                    ["Name"] = name,
                    ["Version"] = version,
                    ["Author"] = author,
                    ["Link"] = link,
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
                }
                ["Trigger Bot"] = new JObject
                {
                    ["Trigger Bot Enabled"] = TriggerBot.Enabled,
                    ["Trigger Bot Delay"] = TriggerBot.Delay,
                    ["Trigger Bot Shoot At Team"] = TriggerBot.ShootAtTeam,
                    ["Trigger Bot Require Keybind"] = TriggerBot.RequireKeybind
                }
                ["Bhop"] = new JObject
                {
                    ["Bhop Enabled"] = Bhop.BhopEnable,
                    ["Bhop Keybind"] = Bhop.HopKey,
                }
            };
            if (!Directory.Exists(ConfigDirPath))
            {
                Directory.CreateDirectory(ConfigDirPath);
            }
            else
            { 
                File.WriteAllText(Path.Combine(ConfigDirPath, FileName + ".json"), configData.ToString());
                Console.WriteLine($"Wrote {FileName} at {Path.Combine(ConfigDirPath, FileName)}.json");
            }
        }
        public static void LoadConfig(string FileName)
        {
            if (!File.Exists(Path.Combine(ConfigDirPath, FileName + ".json")))
            {
                Console.WriteLine($"Config Not Found. At {Path.Combine(ConfigDirPath, FileName)}.json");
                return;
            }

            string jsonString = File.ReadAllText(Path.Combine(ConfigDirPath, FileName + ".json"));

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                Console.WriteLine("Config Is Empty.");
                return;
            }

            JObject configData;
            try
            {
                configData = JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Loading Config: {ex.Message}");
                return;
            }
            #region MISC
            name = configData["0"]?["Name"]?.ToString() ?? name; 
            version = configData["0"]?["Version"]?.ToString() ?? version;
            author = configData["0"]?["Author"]?.ToString() ?? author;
            link = configData["0"]?["Link"]?.ToString() ?? link;
            #endregion MISC

            #region Shape ESP
            BoxESP.enableESP = configData["ESP"]?["ESP Enabled"]?.ToObject<bool>() ?? BoxESP.enableESP;
            BoxESP.Rounding = configData["ESP"]?["ESP Rounding"]?.ToObject<float>() ?? BoxESP.Rounding;
            BoxESP.Outline = configData["ESP"]?["ESP Outline"]?.ToObject<bool>() ?? BoxESP.Outline;
            BoxESP.TeamCheck = configData["ESP"]?["ESP Team Check"]?.ToObject<bool>() ?? BoxESP.TeamCheck;
            BoxESP.DrawOnSelf = configData["ESP"]?["ESP Draw On Self"]?.ToObject<bool>() ?? BoxESP.DrawOnSelf;
            BoxESP.BoxFillOpacity = configData["ESP"]?["ESP Opacity"]?.ToObject<float>() ?? BoxESP.BoxFillOpacity;
            BoxESP.CurrentShape = configData["ESP"]?["ESP Current Shape"]?.ToObject<int>() ?? BoxESP.CurrentShape;
            float OutlineThicknessX = configData["ESP"]?["Outline Thickness"]?["X"]?.ToObject<float>() ?? BoxESP.OutlineThickness.X;
            float OutlineThicknessY = configData["ESP"]?["Outline Thickness"]?["Y"]?.ToObject<float>() ?? BoxESP.OutlineThickness.Y;
            BoxESP.OutlineThickness = new Vector2(OutlineThicknessX, OutlineThicknessY);
            #endregion Shape ESP

            #region Bone ESP
            BoneESP.EnableBoneESP = configData["Bone ESP"]?["Bone ESP Enabled"]?.ToObject<bool>() ?? BoneESP.EnableBoneESP;
            BoneESP.BoneThickness = configData["Bone ESP"]?["Bone ESP Thickness"]?.ToObject<float>() ?? BoneESP.BoneThickness;
            BoneESP.TeamCheck = configData["Bone ESP"]?["Bone ESP Team Check"]?.ToObject<bool>() ?? BoneESP.TeamCheck;
            BoneESP.DrawOnSelf = configData["Bone ESP"]?["Bone ESP Draw On Self"]?.ToObject<bool>() ?? BoneESP.DrawOnSelf;
            #endregion Bone ESP
            #region HealthBar
            HealthBar.EnableHealthBar = configData["HealthBar"]?["HealthBar Enabled"]?.ToObject<bool>() ?? HealthBar.EnableHealthBar;
            HealthBar.Rounding = configData["HealthBar"]?["HealthBar Rounding"]?.ToObject<float>() ?? HealthBar.Rounding;
            HealthBar.HealthBarWidth = configData["HealthBar"]?["HealthBar Width"]?.ToObject<float>() ?? HealthBar.HealthBarWidth;
            #endregion HealthBar
            #region Aimbot
            #endregion Aimbot

        }
    }
}
