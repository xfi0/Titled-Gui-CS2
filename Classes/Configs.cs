using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
                    ["ESP Outline"] = BoxESP.InnerOutline,
                    ["ESP Team Check"] = BoxESP.TeamCheck,
                    ["Outline Thickness"] = new JObject
                    {
                        ["X"] = BoxESP.InnerOutlineThickness.X,
                        ["Y"] = BoxESP.InnerOutlineThickness.Y
                    },
                    ["ESP Opacity"] = BoxESP.BoxFillOpacity,
                    ["ESP Glow Amount"] = BoxESP.GlowAmount,
                    ["ESP Gradient"] = BoxESP.BoxFillGradient,
                    ["ESP Gradient Top"] = new JObject
                    {
                        ["X"] = BoxESP.BoxFillGradientColorTop.X,
                        ["Y"] = BoxESP.BoxFillGradientColorTop.Y,
                        ["Z"] = BoxESP.BoxFillGradientColorTop.Z,
                        ["W"] = BoxESP.BoxFillGradientColorTop.W,
                    },
                    ["ESP Gradient Bottom"] = new JObject
                    {
                        ["X"] = BoxESP.BoxFillGradientBottom.X,
                        ["Y"] = BoxESP.BoxFillGradientBottom.Y,
                        ["Z"] = BoxESP.BoxFillGradientBottom.Z,
                        ["W"] = BoxESP.BoxFillGradientBottom.W,
                    },
                    ["ESP Flash Check"] = BoxESP.FlashCheck,
                    ["Enable Distance Tracker"] = BoxESP.EnableDistanceTracker,
                    ["Outer Outline"] = BoxESP.OuterOutline,
                    ["Inner Outline Color"] = new JObject
                    {
                        ["X"] = BoxESP.InnerOutlineColor.X,
                        ["Y"] = BoxESP.InnerOutlineColor.Y,
                        ["Z"] = BoxESP.InnerOutlineColor.Z,
                        ["W"] = BoxESP.InnerOutlineColor.W,
                    },
                    ["Outline Enemy Color"] = new JObject
                    {
                        ["X"] = BoxESP.OutlineEnemyColor.X,
                        ["Y"] = BoxESP.OutlineEnemyColor.Y,
                        ["Z"] = BoxESP.OutlineEnemyColor.Z,
                        ["W"] = BoxESP.OutlineEnemyColor.W,
                    },
                    ["Outline Team Color"] = new JObject
                    {
                        ["X"] = BoxESP.OutlineTeamColor.X,
                        ["Y"] = BoxESP.OutlineTeamColor.Y,
                        ["Z"] = BoxESP.OutlineTeamColor.Z,
                        ["W"] = BoxESP.OutlineTeamColor.W,
                    }
                },
                ["Colors"] = new JObject
                {
                    ["RGB Enabled"] = Classes.Colors.RGB,
                    ["Team Color"] = new JObject
                    {
                        ["X"] = Classes.Colors.TeamColor.X,
                        ["Y"] = Classes.Colors.TeamColor.Y,
                        ["Z"] = Classes.Colors.TeamColor.Z,
                        ["W"] = Classes.Colors.TeamColor.W,
                    },
                    ["Enemy Color"] = new JObject
                    {
                        ["X"] = Classes.Colors.EnemyColor.X,
                        ["Y"] = Classes.Colors.EnemyColor.Y,
                        ["Z"] = Classes.Colors.EnemyColor.Z,
                        ["W"] = Classes.Colors.EnemyColor.W,
                    }
                },
                ["Tracers"] = new JObject
                {
                    ["Tracers Enabled"] = Tracers.enableTracers,
                    ["Tracers Thickness"] = Tracers.LineThickness,
                    ["Tracers Team Check"] = Tracers.TeamCheck,
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
                    ["Aimbot Draw FOV"] = Aimbot.DrawFov,
                    ["Aimbot Scoped Only"] = Aimbot.ScopedOnly,
                    ["Aimbot Smoothing X"] = Aimbot.SmoothingX,
                    ["Aimbot Smoothing Y"] = Aimbot.SmoothingY,
                    ["Aimbot Key"] = Aimbot.AimbotKey,
                    ["Aimbot FOV Color"] = new JObject
                    {
                        ["Aimbot FOV Color X"] = Aimbot.FovColor.X,
                        ["Aimbot FOV Color Y"] = Aimbot.FovColor.Y,
                        ["Aimbot FOV Color Z"] = Aimbot.FovColor.Z,
                        ["Aimbot FOV Color W"] = Aimbot.FovColor.W,
                    }
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
                    ["Trigger Bot Shoot At Team"] = TriggerBot.TeamCheck,
                    ["Trigger Bot Require Keybind"] = TriggerBot.RequireKeybind,
                    ["Trigger Bot Key"] = TriggerBot.TriggerKey
                },
                ["Bhop"] = new JObject
                {
                    ["Bhop Enabled"] = Bhop.BhopEnable,
                    ["Bhop Keybind"] = Bhop.HopKey,
                },
                ["Jump Hack"] = new JObject
                {
                    ["Jump Hack Enabled"] = JumpHack.JumpHackEnabled,
                    ["Jump Hack Keybind"] = JumpHack.JumpHotkey
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
                },
                ["Name Display"] = new JObject
                {
                    ["Name Display Enabled"] = NameDisplay.Enabled,
                },
                ["Armor Bar"] = new JObject
                {
                    ["Armor Bar Enabled"] = ArmorBar.EnableArmorhBar,
                    ["Armor Bar Rounding"] = ArmorBar.Rounding,
                    ["Armor Bar Team Check"] = ArmorBar.TeamCheck,
                    ["Armor Bar Draw On Self"] = ArmorBar.DrawOnSelf,
                    ["Armor Bar Width"] = ArmorBar.ArmorBarWidth,
                },
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
                },
                ["Bomb Timer Overlay"] = new JObject
                {
                    ["Bomb Timer Enabled"] = BombTimerOverlay.EnableTimeOverlay
                },
                ["Radar"] = new JObject
                {
                    ["Radar Enabled"] = Radar.IsEnabled,
                    ["Radar Draw On Team"] = Radar.DrawOnTeam,
                    ["Radar Draw Cross"] = Radar.DrawCrossb,
                    ["Radar Enemy Point Color"] = new JObject
                    {
                        ["X"] = Radar.EnemyPointColor.X,
                        ["Y"] = Radar.EnemyPointColor.Y,
                        ["Z"] = Radar.EnemyPointColor.Z,
                        ["W"] = Radar.EnemyPointColor.W
                    },
                    ["Radar Team Point Color"] = new JObject
                    {
                        ["X"] = Radar.TeamPointColor.X,
                        ["Y"] = Radar.TeamPointColor.Y,
                        ["Z"] = Radar.TeamPointColor.Z,
                        ["W"] = Radar.TeamPointColor.W
                    }
                },
                ["Spectator Count Overlay"] = new JObject
                {
                    //["Spectator Count Overlay Enabled"] = SpectatorCountOverlay.en
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
                BoxESP.InnerOutline = configData["ESP"]?["ESP Outline"]?.ToObject<bool>() ?? BoxESP.InnerOutline;
                BoxESP.TeamCheck = configData["ESP"]?["ESP Team Check"]?.ToObject<bool>() ?? BoxESP.TeamCheck;
                BoxESP.BoxFillOpacity = configData["ESP"]?["ESP Opacity"]?.ToObject<float>() ?? BoxESP.BoxFillOpacity;
                BoxESP.CurrentShape = configData["ESP"]?["ESP Current Shape"]?.ToObject<int>() ?? BoxESP.CurrentShape;
                float outlineX = configData["ESP"]?["Outline Thickness"]?["X"]?.ToObject<float>() ?? BoxESP.InnerOutlineThickness.X;
                float outlineY = configData["ESP"]?["Outline Thickness"]?["Y"]?.ToObject<float>() ?? BoxESP.InnerOutlineThickness.Y;
                BoxESP.InnerOutlineThickness = new Vector2(outlineX, outlineY);
                BoxESP.GlowAmount = configData["ESP"]?["ESP Glow Amount"]?.ToObject<float>() ?? BoxESP.GlowAmount;
                BoxESP.BoxFillGradient = configData["ESP"]?["ESP Gradient"]?.ToObject<bool>() ?? BoxESP.BoxFillGradient;
                BoxESP.BoxFillGradientColorTop = new Vector4(
                    configData["ESP"]?["ESP Gradient Top"]?["X"]?.ToObject<float>() ?? BoxESP.BoxFillGradientColorTop.X,
                    configData["ESP"]?["ESP Gradient Top"]?["Y"]?.ToObject<float>() ?? BoxESP.BoxFillGradientColorTop.Y,
                    configData["ESP"]?["ESP Gradient Top"]?["Z"]?.ToObject<float>() ?? BoxESP.BoxFillGradientColorTop.Z,
                    configData["ESP"]?["ESP Gradient Top"]?["W"]?.ToObject<float>() ?? BoxESP.BoxFillGradientColorTop.W
                );
                BoxESP.BoxFillGradientBottom = new Vector4(
                    configData["ESP"]?["ESP Gradient Bottom"]?["X"]?.ToObject<float>() ?? BoxESP.BoxFillGradientBottom.X,
                    configData["ESP"]?["ESP Gradient Bottom"]?["Y"]?.ToObject<float>() ?? BoxESP.BoxFillGradientBottom.Y,
                    configData["ESP"]?["ESP Gradient Bottom"]?["Z"]?.ToObject<float>() ?? BoxESP.BoxFillGradientBottom.Z,
                    configData["ESP"]?["ESP Gradient Bottom"]?["W"]?.ToObject<float>() ?? BoxESP.BoxFillGradientBottom.W
                );
                BoxESP.FlashCheck = configData["ESP"]?["ESP Flash Check"]?.ToObject<bool>() ?? BoxESP.FlashCheck;
                BoxESP.EnableDistanceTracker = configData["ESP"]?["Enable Distance Tracker"]?.ToObject<bool>() ?? BoxESP.EnableDistanceTracker;
                BoxESP.OuterOutline = configData["ESP"]?["Outer Outline"]?.ToObject<bool>() ?? BoxESP.OuterOutline;
                BoxESP.InnerOutlineColor = new Vector4(
                    configData["ESP"]?["Inner Outline Color"]?["X"]?.ToObject<float>() ?? BoxESP.InnerOutlineColor.X,
                    configData["ESP"]?["Inner Outline Color"]?["Y"]?.ToObject<float>() ?? BoxESP.InnerOutlineColor.Y,
                    configData["ESP"]?["Inner Outline Color"]?["Z"]?.ToObject<float>() ?? BoxESP.InnerOutlineColor.Z,
                    configData["ESP"]?["Inner Outline Color"]?["W"]?.ToObject<float>() ?? BoxESP.InnerOutlineColor.W
                );
                BoxESP.OutlineEnemyColor = new Vector4(
                    configData["ESP"]?["Outline Enemy Color"]?["X"]?.ToObject<float>() ?? BoxESP.OutlineEnemyColor.X,
                    configData["ESP"]?["Outline Enemy Color"]?["Y"]?.ToObject<float>() ?? BoxESP.OutlineEnemyColor.Y,
                    configData["ESP"]?["Outline Enemy Color"]?["Z"]?.ToObject<float>() ?? BoxESP.OutlineEnemyColor.Z,
                    configData["ESP"]?["Outline Enemy Color"]?["W"]?.ToObject<float>() ?? BoxESP.OutlineEnemyColor.W
                );
                BoxESP.OutlineTeamColor = new Vector4(
                    configData["ESP"]?["Outline Team Color"]?["X"]?.ToObject<float>() ?? BoxESP.OutlineTeamColor.X,
                    configData["ESP"]?["Outline Team Color"]?["Y"]?.ToObject<float>() ?? BoxESP.OutlineTeamColor.Y,
                    configData["ESP"]?["Outline Team Color"]?["Z"]?.ToObject<float>() ?? BoxESP.OutlineTeamColor.Z,
                    configData["ESP"]?["Outline Team Color"]?["W"]?.ToObject<float>() ?? BoxESP.OutlineTeamColor.W
                );
                #endregion

                #region Colors
                Classes.Colors.RGB = configData["Colors"]?["RGB Enabled"]?.ToObject<bool>() ?? Classes.Colors.RGB;
                Classes.Colors.TeamColor = new Vector4(
                    configData["Colors"]?["Team Color"]?["X"]?.ToObject<float>() ?? Classes.Colors.TeamColor.X,
                    configData["Colors"]?["Team Color"]?["Y"]?.ToObject<float>() ?? Classes.Colors.TeamColor.Y,
                    configData["Colors"]?["Team Color"]?["Z"]?.ToObject<float>() ?? Classes.Colors.TeamColor.Z,
                    configData["Colors"]?["Team Color"]?["W"]?.ToObject<float>() ?? Classes.Colors.TeamColor.W
                );
                Classes.Colors.EnemyColor = new Vector4(
                    configData["Colors"]?["Enemy Color"]?["X"]?.ToObject<float>() ?? Classes.Colors.EnemyColor.X,
                    configData["Colors"]?["Enemy Color"]?["Y"]?.ToObject<float>() ?? Classes.Colors.EnemyColor.Y,
                    configData["Colors"]?["Enemy Color"]?["Z"]?.ToObject<float>() ?? Classes.Colors.EnemyColor.Z,
                    configData["Colors"]?["Enemy Color"]?["W"]?.ToObject<float>() ?? Classes.Colors.EnemyColor.W
                );
                #endregion

                #region Tracers
                Tracers.enableTracers = configData["Tracers"]?["Tracers Enabled"]?.ToObject<bool>() ?? Tracers.enableTracers;
                Tracers.LineThickness = configData["Tracers"]?["Tracers Thickness"]?.ToObject<float>() ?? Tracers.LineThickness;
                Tracers.TeamCheck = configData["Tracers"]?["Tracers Team Check"]?.ToObject<bool>() ?? Tracers.TeamCheck;
                Tracers.CurrentStartPos = configData["Tracers"]?["Tracers Current Start"]?.ToObject<int>() ?? Tracers.CurrentStartPos;
                Tracers.CurrentEndPos = configData["Tracers"]?["Tracers Current End"]?.ToObject<int>() ?? Tracers.CurrentEndPos;
                #endregion

                #region Bone ESP
                BoneESP.EnableBoneESP = configData["Bone ESP"]?["Bone ESP Enabled"]?.ToObject<bool>() ?? BoneESP.EnableBoneESP;
                BoneESP.BoneThickness = configData["Bone ESP"]?["Bone ESP Thickness"]?.ToObject<float>() ?? BoneESP.BoneThickness;
                BoneESP.TeamCheck = configData["Bone ESP"]?["Bone ESP Team Check"]?.ToObject<bool>() ?? BoneESP.TeamCheck;
                BoneESP.GlowAmount = configData["Bone ESP"]?["Bone ESP Glow Amount"]?.ToObject<float>() ?? BoneESP.GlowAmount;
                BoneESP.BoneColor = new Vector4(
                    configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color X"]?.ToObject<float>() ?? BoneESP.BoneColor.X,
                    configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color Y"]?.ToObject<float>() ?? BoneESP.BoneColor.Y,
                    configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color Z"]?.ToObject<float>() ?? BoneESP.BoneColor.Z,
                    configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color W"]?.ToObject<float>() ?? BoneESP.BoneColor.W
                );
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
                Aimbot.UseFOV = configData["Aimbot"]?["Aimbot Use FOV"]?.ToObject<bool>() ?? Aimbot.UseFOV;
                Aimbot.DrawFov = configData["Aimbot"]?["Aimbot Draw FOV"]?.ToObject<bool>() ?? Aimbot.DrawFov;
                Aimbot.ScopedOnly = configData["Aimbot"]?["Aimbot Scoped Only"]?.ToObject<bool>() ?? Aimbot.ScopedOnly;
                Aimbot.SmoothingX = configData["Aimbot"]?["Aimbot Smoothing X"]?.ToObject<float>() ?? Aimbot.SmoothingX;
                Aimbot.SmoothingY = configData["Aimbot"]?["Aimbot Smoothing Y"]?.ToObject<float>() ?? Aimbot.SmoothingY;
                Aimbot.AimbotKey = configData["Aimbot"]?["Aimbot Key"]?.ToObject<int>() ?? Aimbot.AimbotKey;
                Aimbot.FovColor = new Vector4(
                    configData["Aimbot"]?["Aimbot FOV Color"]?["Aimbot FOV Color X"]?.ToObject<float>() ?? Aimbot.FovColor.X,
                    configData["Aimbot"]?["Aimbot FOV Color"]?["Aimbot FOV Color Y"]?.ToObject<float>() ?? Aimbot.FovColor.Y,
                    configData["Aimbot"]?["Aimbot FOV Color"]?["Aimbot FOV Color Z"]?.ToObject<float>() ?? Aimbot.FovColor.Z,
                    configData["Aimbot"]?["Aimbot FOV Color"]?["Aimbot FOV Color W"]?.ToObject<float>() ?? Aimbot.FovColor.W
                );
                #endregion

                #region RCS
                RCS.Enabled = configData["RCS"]?["RCS Enabled"]?.ToObject<bool>() ?? RCS.Enabled;
                RCS.Strength = configData["RCS"]?["RCS Strength"]?.ToObject<float>() ?? RCS.Strength;
                #endregion

                #region Trigger Bot
                TriggerBot.Enabled = configData["Trigger Bot"]?["Trigger Bot Enabled"]?.ToObject<bool>() ?? TriggerBot.Enabled;
                TriggerBot.MaxDelay = configData["Trigger Bot"]?["Trigger Bot Max Delay"]?.ToObject<int>() ?? TriggerBot.MaxDelay;
                TriggerBot.MinDelay = configData["Trigger Bot"]?["Trigger Bot Min Delay"]?.ToObject<int>() ?? TriggerBot.MinDelay;
                TriggerBot.TeamCheck = configData["Trigger Bot"]?["Trigger Bot Shoot At Team"]?.ToObject<bool>() ?? TriggerBot.TeamCheck;
                TriggerBot.RequireKeybind = configData["Trigger Bot"]?["Trigger Bot Require Keybind"]?.ToObject<bool>() ?? TriggerBot.RequireKeybind;
                TriggerBot.TriggerKey = configData["Trigger Bot"]?["Trigger Bot Key"]?.ToObject<int>() ?? TriggerBot.TriggerKey;
                #endregion

                #region Bhop
                Bhop.BhopEnable = configData["Bhop"]?["Bhop Enabled"]?.ToObject<bool>() ?? Bhop.BhopEnable;
                Bhop.HopKey = configData["Bhop"]?["Bhop Keybind"]?.ToObject<int>() ?? Bhop.HopKey;
                #endregion

                #region Jump Hack
                JumpHack.JumpHackEnabled = configData["Jump Hack"]?["Jump Hack Enabled"]?.ToObject<bool>() ?? JumpHack.JumpHackEnabled;
                JumpHack.JumpHotkey = configData["Jump Hack"]?["Jump Hack Keybind"]?.ToObject<int>() ?? JumpHack.JumpHotkey;
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

                #region Bomb Timer Overlay
                BombTimerOverlay.EnableTimeOverlay = configData["Bomb Timer Overlay"]?["Bomb Timer Enabled"]?.ToObject<bool>() ?? BombTimerOverlay.EnableTimeOverlay;
                #endregion

                #region Radar
                Radar.IsEnabled = configData["Radar"]?["Radar Enabled"]?.ToObject<bool>() ?? Radar.IsEnabled;
                Radar.DrawOnTeam = configData["Radar"]?["Radar Draw On Team"]?.ToObject<bool>() ?? Radar.DrawOnTeam;
                Radar.DrawCrossb = configData["Radar"]?["Radar Draw Cross"]?.ToObject<bool>() ?? Radar.DrawCrossb;
                Radar.EnemyPointColor = new Vector4(
                    configData["Radar"]?["Radar Enemy Point Color"]?["X"]?.ToObject<float>() ?? Radar.EnemyPointColor.X,
                    configData["Radar"]?["Radar Enemy Point Color"]?["Y"]?.ToObject<float>() ?? Radar.EnemyPointColor.Y,
                    configData["Radar"]?["Radar Enemy Point Color"]?["Z"]?.ToObject<float>() ?? Radar.EnemyPointColor.Z,
                    configData["Radar"]?["Radar Enemy Point Color"]?["W"]?.ToObject<float>() ?? Radar.EnemyPointColor.W
                );
                Radar.TeamPointColor = new Vector4(
                    configData["Radar"]?["Radar Team Point Color"]?["X"]?.ToObject<float>() ?? Radar.TeamPointColor.X,
                    configData["Radar"]?["Radar Team Point Color"]?["Y"]?.ToObject<float>() ?? Radar.TeamPointColor.Y,
                    configData["Radar"]?["Radar Team Point Color"]?["Z"]?.ToObject<float>() ?? Radar.TeamPointColor.Z,
                    configData["Radar"]?["Radar Team Point Color"]?["W"]?.ToObject<float>() ?? Radar.TeamPointColor.W
                );
                #endregion

                #region Spectator Count Overlay
                //SpectatorCountOverlay.Enabled = configData["Spectator Count Overlay"]?["Spectator Count Overlay Enabled"]?.ToObject<bool>() ?? SpectatorCountOverlay.Enabled;
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