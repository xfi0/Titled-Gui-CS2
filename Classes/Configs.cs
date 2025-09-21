using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Classes
{
    internal class Configs : Classes.ThreadService
    {
        public static string MenuName = "Titled";
        public static string Version = "1.3";
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
                    ["Name"] = MenuName,
                    ["Version"] = Version,
                    ["Author"] = Author,
                    ["Link"] = Link,
                },
                ["ESP"] = new JObject
                {
                    ["ESP Enabled"] = BoxESP.EnableESP,
                    ["ESP Current Shape"] = BoxESP.CurrentShape,
                    ["ESP Rounding"] = BoxESP.Rounding,
                    ["ESP Outline"] = BoxESP.Outline,
                    ["ESP Team Check"] = BoxESP.TeamCheck,
                    ["Outline Thickness"] = new JObject
                    {
                        ["X"] = BoxESP.OutlineThickness.X,
                        ["Y"] = BoxESP.OutlineThickness.Y
                    },
                    ["ESP Opacity"] = BoxESP.BoxFillOpacity,
                    ["ESP Glow Amount"] = BoxESP.GlowAmount,
                    ["ESP Gradient"] = BoxESP.BoxFillGradient,
                    ["ESP Gradient Top"] = new JObject
                    {
                        ["X"]  = BoxESP.BoxFillGradientColorTop.X,
                        ["Y"] = BoxESP.BoxFillGradientColorTop.Y,
                        ["Z"] = BoxESP.BoxFillGradientColorTop.Z,
                        ["W"] = BoxESP.BoxFillGradientColorTop.W,
                    }
                    ["ESP Flash Check"] = BoxESP.FlashCheck,
                },
                ["Tracers"] = new JObject
                {
                    ["Tracers Enabled"] = Tracers.enableTracers,
                    ["Tracers Thickness"] = Tracers.LineThickness,
                    ["Tracers Team Check"] = Tracers.TeamCheck,
                    ["Tracers Draw On Self"] = Tracers.DrawOnSelf,
                    ["Tracers Current Start"] = Tracers.CurrentStartPos,
                    ["Tracers Current End"] = Tracers.CurrentEndPos
                },
                ["Bone ESP"] = new JObject
                {
                    ["Bone ESP Enabled"] = BoneESP.EnableBoneESP,
                    ["Bone ESP Thickness"] = BoneESP.BoneThickness,
                    ["Bone ESP Team Check"] = BoneESP.TeamCheck,
                    ["Bone ESP Glow Amount"] = BoneESP.GlowAmount,
                    ["Bone ESP Color"] = new JObject
                    {
                        ["Bone ESP Color X"] = BoneESP.BoneColor.X,
                        ["Bone ESP Color Y"] = BoneESP.BoneColor.Y,
                        ["Bone ESP Color Z"] = BoneESP.BoneColor.Z,
                        ["Bone ESP Color W"] = BoneESP.BoneColor.W,
                    }
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
                    ["Aimbot FOV Size"] = Aimbot.FovSize,
                    ["Aimbot Use FOV"] = Aimbot.UseFOV,
                    ["Aimbot FOV Color"] = new JObject
                    {
                        ["Aimbot FOV Color X"] = Aimbot.FovColor.X,
                        ["Aimbot FOV Color Y"] = Aimbot.FovColor.Y,
                        ["Aimbot FOV Color Z"] = Aimbot.FovColor.Z,
                        ["Aimbot FOV Color W"] = Aimbot.FovColor.Z,   
                    }

                    ["Aimbox Scope Check"] = Aimbot.ScopedOnly,
                },
                ["RCS"] = new JObject
                {
                    ["RCS Enabled"] = RCS.Enabled,
                    ["RCS Strength"] = RCS.Strength
                },
                ["Trigger Bot"] = new JObject
                {
                    ["Trigger Bot Enabled"] = TriggerBot.Enabled,
                    ["Trigger Bot Max Delay"] = TriggerBot.MaxDelay,
                    ["Trigger Bot Min Delay"] = TriggerBot.MinDelay,
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
                    ["Chams Thickness"] = Chams.BoneThickness,
                    ["Chams Draw On Self"] = Chams.DrawOnSelf
                },
                ["No Flash"] = new JObject
                {
                    ["No Flash Enabled"] = NoFlash.NoFlashEnable,
                },
                ["FOV Changer"] = new JObject
                {
                    ["FOV Changer Enabled"] = FovChanger.Enabled,
                    ["FOV Changer FOV"] = FovChanger.FOV,
                }
                ["Name Display"] = new JObject
                {
                    ["Name Display Enabled"] = NameDisplay.Enabled,
                }
                ["Armor Bar"] = new JObject
                {
                    ["Armor Bar Enabled"] = ArmorBar.EnableArmorhBar,
                    ["Armor Bar Rounding"] = ArmorBar.Rounding,
                    ["Armor Bar Team Check"] = ArmorBar.TeamCheck,
                    ["Armor Bar Draw On Self"] = ArmorBar.DrawOnSelf,
                    ["Armor Bar Width"] = ArmorBar.ArmorBarWidth,
                }
                ["Hit Actions"] = new JObject
                {
                    ["Hit Sound Enabled"] = HitStuff.Enabled,
                    ["Hit Sound Volume"] = HitStuff.Volume,
                    ["Current Hit Sound"] = HitStuff.CurrentHitSound,
                    ["Enable Headshot Text"] = HitStuff.EnableHeadshotText,
                    ["Headshot Text Color"] = new JObject
                    {
                        ["Headshot Text Color X"] = HitStuff.TextColor.X,
                        ["Headshot Text Color Y"] = HitStuff.TextColor.Y, 
                        ["Headshot Text Color Z"] = HitStuff.TextColor.Z,
                        ["Headshot Text Color W"] = HitStuff.TextColor.W,
                    }
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
            try
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
                    Console.WriteLine("Config Is Empty.");
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
                MenuName = configData["0"]?["Name"]?.ToString() ?? MenuName;
                Version = configData["0"]?["Version"]?.ToString() ?? Version;
                Author = configData["0"]?["Author"]?.ToString() ?? Author;
                Link = configData["0"]?["Link"]?.ToString() ?? Link;
                #endregion

                #region Shape ESP
                BoxESP.EnableESP = configData["ESP"]?["ESP Enabled"]?.ToObject<bool>() ?? BoxESP.EnableESP;
                BoxESP.Rounding = configData["ESP"]?["ESP Rounding"]?.ToObject<float>() ?? BoxESP.Rounding;
                BoxESP.Outline = configData["ESP"]?["ESP Outline"]?.ToObject<bool>() ?? BoxESP.Outline;
                BoxESP.TeamCheck = configData["ESP"]?["ESP Team Check"]?.ToObject<bool>() ?? BoxESP.TeamCheck;
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
                Aimbot.FovSize = configData["Aimbot"]?["Aimbot FOV Size"]?.ToObject<int>() ?? Aimbot.FovSize;
                #endregion

                #region RCS
                RCS.Enabled = configData["RCS"]?["RCS Enabled"]?.ToObject<bool>() ?? RCS.Enabled;
                RCS.Strength = configData["RCS"]?["RCS Strength"]?.ToObject<float>() ?? RCS.Strength;
                #endregion

                #region Trigger Bot
                TriggerBot.Enabled = configData["Trigger Bot"]?["Trigger Bot Enabled"]?.ToObject<bool>() ?? TriggerBot.Enabled;
                TriggerBot.MaxDelay = configData["Trigger Bot"]?["Trigger Bot Max Delay"]?.ToObject<int>() ?? TriggerBot.MaxDelay;
                TriggerBot.MinDelay = configData["Trigger Bot"]?["Trigger Bot Min Delay"]?.ToObject<int>() ?? TriggerBot.MinDelay;
                TriggerBot.ShootAtTeam = configData["Trigger Bot"]?["Trigger Bot Shoot At Team"]?.ToObject<bool>() ?? TriggerBot.ShootAtTeam;
                TriggerBot.RequireKeybind = configData["Trigger Bot"]?["Trigger Bot Require Keybind"]?.ToObject<bool>() ?? TriggerBot.RequireKeybind;
                #endregion

                #region Bhop
                Bhop.BhopEnable = configData["Bhop"]?["Bhop Enabled"]?.ToObject<bool>() ?? Bhop.BhopEnable;
                Bhop.HopKey = configData["Bhop"]?["Bhop Keybind"]?.ToObject<int>() ?? Bhop.HopKey;
                #endregion

                #region Eye Ray
                EyeRay.Enabled = configData["Eye Ray"]?["Eye Ray Enabled"]?.ToObject<bool>() ?? EyeRay.Enabled;
                EyeRay.Length = configData["Eye Ray"]?["Eye Ray Length"]?.ToObject<float>() ?? EyeRay.Length;
                #endregion

                #region Chams
                Chams.EnableChams = configData["Chams"]?["Chams Enabled"]?.ToObject<bool>() ?? Chams.EnableChams;
                Chams.BoneThickness = configData["Chams"]?["Chams Thickness"]?.ToObject<float>() ?? Chams.BoneThickness;
                Chams.DrawOnSelf = configData["Chams"]?["Chams Draw On Self"]?.ToObject<bool>() ?? Chams.DrawOnSelf;
                #endregion

                #region No Flash
                NoFlash.NoFlashEnable = configData["No Flash"]?["No Flash Enabled"]?.ToObject<bool>() ?? NoFlash.NoFlashEnable;
                #endregion

                #region FOV Changer
                FovChanger.Enabled = configData["FOV Changer"]?["FOV Changer Enabled"]?.ToObject<bool>() ?? FovChanger.Enabled;
                FovChanger.FOV = configData["FOV Changer"]?["FOV Changer FOV"]?.ToObject<int>() ?? FovChanger.FOV;
                #endregion

                #region Name Display
                NameDisplay.Enabled = configData["Name Display"]?["Name Display Enabled"]?.ToObject<bool>() ?? NameDisplay.Enabled;
                #endregion

                #region Armor Bar
                ArmorBar.EnableArmorhBar = configData["Armor Bar"]?["Armor Bar Enabled"]?.ToObject<bool>() ?? ArmorBar.EnableArmorhBar;
                ArmorBar.Rounding = configData["Armor Bar"]?["Armor Bar Rounding"]?.ToObject<float>() ?? ArmorBar.Rounding;
                ArmorBar.TeamCheck = configData["Armor Bar"]?["Armor Bar Team Check"]?.ToObject<bool>() ?? ArmorBar.TeamCheck;
                ArmorBar.DrawOnSelf = configData["Armor Bar"]?["Armor Bar Draw On Self"]?.ToObject<bool>() ?? ArmorBar.DrawOnSelf;
                ArmorBar.ArmorBarWidth = configData["Armor Bar"]?["Armor Bar Width"]?.ToObject<float>() ?? ArmorBar.ArmorBarWidth;
                #endregion

                #region Hit Actions
                HitStuff.Enabled = configData["Hit Actions"]?["Hit Sound Enabled"]?.ToObject<bool>() ?? HitStuff.Enabled;
                HitStuff.Volume = configData["Hit Actions"]?["Hit Sound Volume"]?.ToObject<float>() ?? HitStuff.Volume;
                HitStuff.CurrentHitSound = configData["Hit Actions"]?["Current Hit Sound"]?.ToObject<int>() ?? HitStuff.CurrentHitSound;
                HitStuff.EnableHeadshotText = configData["Hit Actions"]?["Enable Headshot Text"]?.ToObject<bool>() ?? HitStuff.EnableHeadshotText;
                float headshotR = configData["Hit Actions"]?["Headshot Text Color"]?["Headshot Text Color X"]?.ToObject<float>() ?? HitStuff.TextColor.X;
                float headshotG = configData["Hit Actions"]?["Headshot Text Color"]?["Headshot Text Color Y"]?.ToObject<float>() ?? HitStuff.TextColor.Y;
                float headshotB = configData["Hit Actions"]?["Headshot Text Color"]?["Headshot Text Color Z"]?.ToObject<float>() ?? HitStuff.TextColor.Z;
                float headshotA = configData["Hit Actions"]?["Headshot Text Color"]?["Headshot Text Color W"]?.ToObject<float>() ?? HitStuff.TextColor.W;
                HitStuff.TextColor = new Vector4(headshotR, headshotG, headshotB, headshotA);
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        protected override void FrameAction()
        {
            if (!Directory.Exists(Configs.ConfigDirPath))
            {
                Directory.CreateDirectory(Configs.ConfigDirPath);
            }
            var files = Directory.EnumerateFiles(Configs.ConfigDirPath).Select(Path.GetFileName).Where(f => f != null).ToHashSet(); // refresh so if any thing changes the dic updates
            foreach (var file in files)
            {
                Configs.SavedConfigs.TryAdd(file!, true);
            }
            foreach (var key in Configs.SavedConfigs.Keys)
            {
                if (!files.Contains(key))
                {
                    Configs.SavedConfigs.TryRemove(key, out _);
                }
            }
        }
    }
}
