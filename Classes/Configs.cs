using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Numerics;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Classes
{
    internal class Configs : Classes.ThreadService
    {
        public static string MenuName = "Titled";
        public static string Version = "2.3";
        public static string Author = "https://github.com/xfi0";
        public static string Link = "https://github.com/xfi0/Titled-Gui-CS2";
        public static string titledDocumentsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Titled", "CS2", "External");

        public static string ConfigName = "";
        public static ConcurrentDictionary<string, bool> SavedConfigs = new();
        public static string SelectedConfig = "";

        public static readonly string ConfigDirPath = Path.Combine(titledDocumentsFolder, "Configs");
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
                    ["Outline Team RGB"] = BoxESP.OutlineColors.TeamRGB,
                    ["Outline Enemy RGB"] = BoxESP.OutlineColors.EnemyRGB,
                    ["Fill Team RGB"] = BoxESP.FillColors.TeamRGB,
                    ["Fill Enemy RGB"] = BoxESP.FillColors.EnemyRGB,
                    ["Occluded Team RGB"] = BoxESP.OccludedColors.TeamRGB,
                    ["Occluded Enemy RGB"] = BoxESP.OccludedColors.EnemyRGB,
                    ["Gradient Team RGB"] = BoxESP.GradientColors.TeamRGB,
                    ["Gradient Enemy RGB"] = BoxESP.GradientColors.EnemyRGB,
                    ["Outline Thickness"] = new JObject
                    {
                        ["X"] = BoxESP.InnerOutlineThickness.X,
                        ["Y"] = BoxESP.InnerOutlineThickness.Y
                    },
                    ["ESP Glow Amount"] = BoxESP.GlowAmount,
                    ["ESP Gradient"] = BoxESP.BoxFillGradient,
                    ["ESP Gradient Top"] = new JObject
                    {
                        ["X"] = BoxESP.GradientColors.TeamColor.X,
                        ["Y"] = BoxESP.GradientColors.TeamColor.Y,
                        ["Z"] = BoxESP.GradientColors.TeamColor.Z,
                        ["W"] = BoxESP.GradientColors.TeamColor.W,
                    },
                    ["ESP Gradient Bottom"] = new JObject
                    {
                        ["X"] = BoxESP.GradientColors.EnemyColor.X,
                        ["Y"] = BoxESP.GradientColors.EnemyColor.Y,
                        ["Z"] = BoxESP.GradientColors.EnemyColor.Z,
                        ["W"] = BoxESP.GradientColors.EnemyColor.W,
                    },
                    ["ESP Flash Check"] = BoxESP.FlashCheck,
                    ["Outer Outline"] = BoxESP.OuterOutline,
                    ["Inner Outline Color"] = new JObject
                    {
                        ["X"] = BoxESP.InnerOutlineColors.TeamColor.X,
                        ["Y"] = BoxESP.InnerOutlineColors.TeamColor.Y,
                        ["Z"] = BoxESP.InnerOutlineColors.TeamColor.Z,
                        ["W"] = BoxESP.InnerOutlineColors.TeamColor.W,
                    },
                    ["Outline Enemy Color"] = new JObject
                    {
                        ["X"] = BoxESP.OutlineColors.EnemyColor.X,
                        ["Y"] = BoxESP.OutlineColors.EnemyColor.Y,
                        ["Z"] = BoxESP.OutlineColors.EnemyColor.Z,
                        ["W"] = BoxESP.OutlineColors.EnemyColor.W,
                    },
                    ["Outline Team Color"] = new JObject
                    {
                        ["X"] = BoxESP.OutlineColors.TeamColor.X,
                        ["Y"] = BoxESP.OutlineColors.TeamColor.Y,
                        ["Z"] = BoxESP.OutlineColors.TeamColor.Z,
                        ["W"] = BoxESP.OutlineColors.TeamColor.W,
                    },
                    ["Fill Team Color"] = new JObject
                    {
                        ["X"] = BoxESP.FillColors.TeamColor.X,
                        ["Y"] = BoxESP.FillColors.TeamColor.Y,
                        ["Z"] = BoxESP.FillColors.TeamColor.Z,
                        ["W"] = BoxESP.FillColors.TeamColor.W
                    },
                    ["Fill Enemy Color"] = new JObject
                    {
                        ["X"] = BoxESP.FillColors.EnemyColor.X,
                        ["Y"] = BoxESP.FillColors.EnemyColor.Y,
                        ["Z"] = BoxESP.FillColors.EnemyColor.Z,
                        ["W"] = BoxESP.FillColors.EnemyColor.W
                    },
                    ["Occluded Team Color"] = new JObject
                    {
                        ["X"] = BoxESP.OccludedColors.TeamColor.X,
                        ["Y"] = BoxESP.OccludedColors.TeamColor.Y,
                        ["Z"] = BoxESP.OccludedColors.TeamColor.Z,
                        ["W"] = BoxESP.OccludedColors.TeamColor.W
                    },
                    ["Occluded Enemy Color"] = new JObject
                    {
                        ["X"] = BoxESP.OccludedColors.EnemyColor.X,
                        ["Y"] = BoxESP.OccludedColors.EnemyColor.Y,
                        ["Z"] = BoxESP.OccludedColors.EnemyColor.Z,
                        ["W"] = BoxESP.OccludedColors.EnemyColor.W
                    },
                },
                ["Flags"] = new JObject
                {
                    ["Name Display Enabled"] = NameDisplay.Enabled,
                    ["Enable Distance Tracker"] = DistanceText.Enabled,
                    ["Gun Display Enabled"] = Flags.GunEnabled,
                    ["Scoped Enabled"] = Flags.ScopedEnabled,
                    ["Flash Enabled"] = Flags.FlashEnabled,
                    ["Flags Text Color Team"] = new JObject
                    {
                        ["X"] = Flags.TextColors.TeamColor.X,
                        ["Y"] = Flags.TextColors.TeamColor.Y,
                        ["Z"] = Flags.TextColors.TeamColor.Z,
                        ["W"] = Flags.TextColors.TeamColor.W,
                    },

                    ["Flags Text Color Enemy"] = new JObject
                    {
                        ["X"] = Flags.TextColors.EnemyColor.X,
                        ["Y"] = Flags.TextColors.EnemyColor.Y,
                        ["Z"] = Flags.TextColors.EnemyColor.Z,
                        ["W"] = Flags.TextColors.EnemyColor.W,
                    },
                    ["Ping Display Enabled"] = PingDisplay.Enabled,
                    ["Ping Display Color"] = new JObject
                    {
                        ["X"] = PingDisplay.PingTextColor.X,
                        ["Y"] = PingDisplay.PingTextColor.Y,
                        ["Z"] = PingDisplay.PingTextColor.Z,
                        ["W"] = PingDisplay.PingTextColor.W,
                    },
                },
                ["Tracers"] = new JObject
                {
                    ["Tracers Enabled"] = Tracers.EnableTracers,
                    ["Tracers Thickness"] = Tracers.LineThickness,
                    ["Tracers Team Check"] = Tracers.TeamCheck,
                    ["Tracers Current Start"] = Tracers.CurrentStartPos,
                    ["Tracers Current End"] = Tracers.CurrentEndPos,
                    ["Tracers Team Color"] = new JObject
                    {
                        ["X"] = Tracers.TeamColor.X,
                        ["Y"] = Tracers.TeamColor.Y,
                        ["Z"] = Tracers.TeamColor.Z,
                        ["W"] = Tracers.TeamColor.W,
                    },
                    ["Tracers Enemy Color"] = new JObject
                    {
                        ["X"] = Tracers.EnemyColor.X,
                        ["Y"] = Tracers.EnemyColor.Y,
                        ["Z"] = Tracers.EnemyColor.Z,
                        ["W"] = Tracers.EnemyColor.W,
                    }
                },
                ["Bone ESP"] = new JObject
                {
                    ["Bone ESP Enabled"] = BoneESP.EnableBoneESP,
                    ["Bone ESP Thickness"] = BoneESP.BoneThickness,
                    ["Bone ESP Team Check"] = BoneESP.TeamCheck,
                    ["Bone ESP Glow Amount"] = BoneESP.GlowAmount,
                    ["Bone ESP Team Colors Visible"] = new JObject
                    {
                        ["X"] = BoneESP.VisibleColors.TeamColor.X,
                        ["Y"] = BoneESP.VisibleColors.TeamColor.Y,
                        ["Z"] = BoneESP.VisibleColors.TeamColor.Z,
                        ["W"] = BoneESP.VisibleColors.TeamColor.W,
                    },
                    ["Bone ESP Enemy Colors Visible"] = new JObject
                    {
                        ["X"] = BoneESP.VisibleColors.EnemyColor.X,
                        ["Y"] = BoneESP.VisibleColors.EnemyColor.Y,
                        ["Z"] = BoneESP.VisibleColors.EnemyColor.Z,
                        ["W"] = BoneESP.VisibleColors.EnemyColor.W,
                    },
                    ["Bone ESP Team Colors Occluded"] = new JObject
                    {
                        ["X"] = BoneESP.OccludedColors.TeamColor.X,
                        ["Y"] = BoneESP.OccludedColors.TeamColor.Y,
                        ["Z"] = BoneESP.OccludedColors.TeamColor.Z,
                        ["W"] = BoneESP.OccludedColors.TeamColor.W,
                    },
                    ["Bone ESP Enemy Colors Occluded"] = new JObject
                    {
                        ["X"] = BoneESP.OccludedColors.EnemyColor.X,
                        ["Y"] = BoneESP.OccludedColors.EnemyColor.Y,
                        ["Z"] = BoneESP.OccludedColors.EnemyColor.Z,
                        ["W"] = BoneESP.OccludedColors.EnemyColor.W,
                    },
                    ["Bone ESP Visible Team RGB"] = BoneESP.VisibleColors.TeamRGB,
                    ["Bone ESP Visible Enemy RGB"] = BoneESP.VisibleColors.EnemyRGB,
                    ["Bone ESP Occluded Team RGB"] = BoneESP.OccludedColors.TeamRGB,
                    ["Bone ESP Occluded Enemy RGB"] = BoneESP.OccludedColors.EnemyRGB,
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
                    ["Chams Enabled"] = Chams.Enabled,
                    ["Chams Style Index"] = Chams.StyleIndex,
                    ["Chams Team Check"] = Chams.TeamCheck
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
                ["Armor Bar"] = new JObject
                {
                    ["Armor Bar Enabled"] = ArmorBar.EnableArmorBar,
                    ["Armor Bar Draw On Self"] = ArmorBar.DrawOnSelf,
                    ["Armor Bar Width"] = ArmorBar.ArmorBarWidth,
                },
                ["Hit Actions"] = new JObject
                {
                    ["Hit Sound Enabled"] = HitStuff.EnableHitSounds,
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
                ["Spectator List"] = new JObject
                {
                    ["Spectator List Enabled"] = SpectatorList.Enabled
                },
                ["World ESP"] = new JObject()
                {
                    ["Draw Boxes"] = WorldESP.DrawBoxes,
                    ["Draw Text"] = WorldESP.DrawText,
                    ["Chicken ESP Enabled"] = WorldESP.ChickenESP,
                    ["Hostage ESP Enabled"] = WorldESP.HostageESP,
                    ["Dropped Weapon ESP Enabled"] = WorldESP.DroppedWeaponESP,
                    ["Thrown Projectile ESP Enabled"] = WorldESP.ProjectileESP,
                    ["Molotov ESP Enabled"] = WorldESP.MolotovBoundsESP,
                    ["Chicken Text Color"] = new JObject
                    {
                        ["X"] = WorldESP.ChickenTextColor.X,
                        ["Y"] = WorldESP.ChickenTextColor.Y,
                        ["Z"] = WorldESP.ChickenTextColor.Z,
                        ["W"] = WorldESP.ChickenTextColor.W
                    },
                    ["Dropped Weapon Text Color"] = new JObject
                    {
                        ["X"] = WorldESP.WeaponTextColor.X,
                        ["Y"] = WorldESP.WeaponTextColor.Y,
                        ["Z"] = WorldESP.WeaponTextColor.Z,
                        ["W"] = WorldESP.WeaponTextColor.W
                    },
                    ["Thrown Projectile Text Color"] = new JObject
                    {
                        ["X"] = WorldESP.ProjectileTextColor.X,
                        ["Y"] = WorldESP.ProjectileTextColor.Y,
                        ["Z"] = WorldESP.ProjectileTextColor.Z,
                        ["W"] = WorldESP.ProjectileTextColor.W
                    },
                    ["Box Color"] = new JObject
                    {
                        ["X"] = WorldESP.BoxColor.X,
                        ["Y"] = WorldESP.BoxColor.Y,
                        ["Z"] = WorldESP.BoxColor.Z,
                        ["W"] = WorldESP.BoxColor.W
                    },
                    ["Molotov Bounds Filled Color"] = new JObject
                    {
                        ["X"] = WorldESP.MolotovColors.PrimaryColor.X,
                        ["Y"] = WorldESP.MolotovColors.PrimaryColor.Y,
                        ["Z"] = WorldESP.MolotovColors.PrimaryColor.Z,
                        ["W"] = WorldESP.MolotovColors.PrimaryColor.W
                    },
                    ["Molotov Bounds Outline Color"] = new JObject
                    {
                        ["X"] = WorldESP.MolotovColors.SecondaryColor.X,
                        ["Y"] = WorldESP.MolotovColors.SecondaryColor.Y,
                        ["Z"] = WorldESP.MolotovColors.SecondaryColor.Z,
                        ["W"] = WorldESP.MolotovColors.SecondaryColor.W
                    },
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

                #region Shape ESP
                BoxESP.EnableESP = configData["ESP"]?["ESP Enabled"]?.ToObject<bool>() ?? BoxESP.EnableESP;
                BoxESP.Rounding = configData["ESP"]?["ESP Rounding"]?.ToObject<float>() ?? BoxESP.Rounding;
                BoxESP.InnerOutline = configData["ESP"]?["ESP Outline"]?.ToObject<bool>() ?? BoxESP.InnerOutline;
                BoxESP.TeamCheck = configData["ESP"]?["ESP Team Check"]?.ToObject<bool>() ?? BoxESP.TeamCheck;
                BoxESP.CurrentShape = configData["ESP"]?["ESP Current Shape"]?.ToObject<int>() ?? BoxESP.CurrentShape;
                float outlineX = configData["ESP"]?["Outline Thickness"]?["X"]?.ToObject<float>() ?? BoxESP.InnerOutlineThickness.X;
                float outlineY = configData["ESP"]?["Outline Thickness"]?["Y"]?.ToObject<float>() ?? BoxESP.InnerOutlineThickness.Y;
                BoxESP.InnerOutlineThickness = new Vector2(outlineX, outlineY);
                BoxESP.GlowAmount = configData["ESP"]?["ESP Glow Amount"]?.ToObject<float>() ?? BoxESP.GlowAmount;
                BoxESP.BoxFillGradient = configData["ESP"]?["ESP Gradient"]?.ToObject<bool>() ?? BoxESP.BoxFillGradient;
                BoxESP.GradientColors.TeamColor = new Vector4(
                    configData["ESP"]?["ESP Gradient Top"]?["X"]?.ToObject<float>() ?? BoxESP.GradientColors.TeamColor.X,
                    configData["ESP"]?["ESP Gradient Top"]?["Y"]?.ToObject<float>() ?? BoxESP.GradientColors.TeamColor.Y,
                    configData["ESP"]?["ESP Gradient Top"]?["Z"]?.ToObject<float>() ?? BoxESP.GradientColors.TeamColor.Z,
                    configData["ESP"]?["ESP Gradient Top"]?["W"]?.ToObject<float>() ?? BoxESP.GradientColors.TeamColor.W
                );
                BoxESP.GradientColors.EnemyColor = new Vector4(
                    configData["ESP"]?["ESP Gradient Bottom"]?["X"]?.ToObject<float>() ?? BoxESP.GradientColors.EnemyColor.X,
                    configData["ESP"]?["ESP Gradient Bottom"]?["Y"]?.ToObject<float>() ?? BoxESP.GradientColors.EnemyColor.Y,
                    configData["ESP"]?["ESP Gradient Bottom"]?["Z"]?.ToObject<float>() ?? BoxESP.GradientColors.EnemyColor.Z,
                    configData["ESP"]?["ESP Gradient Bottom"]?["W"]?.ToObject<float>() ?? BoxESP.GradientColors.EnemyColor.W
                );
                BoxESP.FlashCheck = configData["ESP"]?["ESP Flash Check"]?.ToObject<bool>() ?? BoxESP.FlashCheck;
                BoxESP.OuterOutline = configData["ESP"]?["Outer Outline"]?.ToObject<bool>() ?? BoxESP.OuterOutline;
                BoxESP.OccludedColors.EnemyColor = new Vector4(
                configData["ESP"]?["Occluded Enemy Color"]?["X"]?.ToObject<float>() ?? BoxESP.OccludedColors.EnemyColor.X,
                configData["ESP"]?["Occluded Enemy Color"]?["Y"]?.ToObject<float>() ?? BoxESP.OccludedColors.EnemyColor.Y,
                configData["ESP"]?["Occluded Enemy Color"]?["Z"]?.ToObject<float>() ?? BoxESP.OccludedColors.EnemyColor.Z,
                configData["ESP"]?["Occluded Enemy Color"]?["W"]?.ToObject<float>() ?? BoxESP.OccludedColors.EnemyColor.W
                );
                BoxESP.OccludedColors.TeamColor = new Vector4(
                    configData["ESP"]?["Occluded Team Color"]?["X"]?.ToObject<float>() ?? BoxESP.OccludedColors.TeamColor.X,
                    configData["ESP"]?["Occluded Team Color"]?["Y"]?.ToObject<float>() ?? BoxESP.OccludedColors.TeamColor.Y,
                    configData["ESP"]?["Occluded Team Color"]?["Z"]?.ToObject<float>() ?? BoxESP.OccludedColors.TeamColor.Z,
                    configData["ESP"]?["Occluded Team Color"]?["W"]?.ToObject<float>() ?? BoxESP.OccludedColors.TeamColor.W
                );
                BoxESP.OutlineColors.TeamColor = new Vector4(
                    configData["ESP"]?["Outline Team Color"]?["X"]?.ToObject<float>() ?? BoxESP.OutlineColors.TeamColor.X,
                    configData["ESP"]?["Outline Team Color"]?["Y"]?.ToObject<float>() ?? BoxESP.OutlineColors.TeamColor.Y,
                    configData["ESP"]?["Outline Team Color"]?["Z"]?.ToObject<float>() ?? BoxESP.OutlineColors.TeamColor.Z,
                    configData["ESP"]?["Outline Team Color"]?["W"]?.ToObject<float>() ?? BoxESP.OutlineColors.TeamColor.W
                );
                BoxESP.OutlineColors.EnemyColor = new Vector4(
                    configData["ESP"]?["Outline Enemy Color"]?["X"]?.ToObject<float>() ?? BoxESP.OutlineColors.EnemyColor.X,
                    configData["ESP"]?["Outline Enemy Color"]?["Y"]?.ToObject<float>() ?? BoxESP.OutlineColors.EnemyColor.Y,
                    configData["ESP"]?["Outline Enemy Color"]?["Z"]?.ToObject<float>() ?? BoxESP.OutlineColors.EnemyColor.Z,
                    configData["ESP"]?["Outline Enemy Color"]?["W"]?.ToObject<float>() ?? BoxESP.OutlineColors.EnemyColor.W
                );
                BoxESP.FillColors.TeamColor = new Vector4(
                    configData["ESP"]?["Fill Team Color"]?["X"]?.ToObject<float>() ?? BoxESP.FillColors.TeamColor.X,
                    configData["ESP"]?["Fill Team Color"]?["Y"]?.ToObject<float>() ?? BoxESP.FillColors.TeamColor.Y,
                    configData["ESP"]?["Fill Team Color"]?["Z"]?.ToObject<float>() ?? BoxESP.FillColors.TeamColor.Z,
                    configData["ESP"]?["Fill Team Color"]?["W"]?.ToObject<float>() ?? BoxESP.FillColors.TeamColor.W
                );
                BoxESP.FillColors.EnemyColor = new Vector4(
                    configData["ESP"]?["Fill Enemy Color"]?["X"]?.ToObject<float>() ?? BoxESP.FillColors.EnemyColor.X,
                    configData["ESP"]?["Fill Enemy Color"]?["Y"]?.ToObject<float>() ?? BoxESP.FillColors.EnemyColor.Y,
                    configData["ESP"]?["Fill Enemy Color"]?["Z"]?.ToObject<float>() ?? BoxESP.FillColors.EnemyColor.Z,
                    configData["ESP"]?["Fill Enemy Color"]?["W"]?.ToObject<float>() ?? BoxESP.FillColors.EnemyColor.W
                );
                BoxESP.OutlineColors.TeamRGB = configData["ESP"]?["Outline Team RGB"]?.ToObject<bool>() ?? BoxESP.OutlineColors.TeamRGB;
                BoxESP.OutlineColors.EnemyRGB = configData["ESP"]?["Outline Enemy RGB"]?.ToObject<bool>() ?? BoxESP.OutlineColors.EnemyRGB;
                BoxESP.FillColors.TeamRGB = configData["ESP"]?["Fill Team RGB"]?.ToObject<bool>() ?? BoxESP.FillColors.TeamRGB;
                BoxESP.FillColors.EnemyRGB = configData["ESP"]?["Fill Enemy RGB"]?.ToObject<bool>() ?? BoxESP.FillColors.EnemyRGB;
                BoxESP.OccludedColors.TeamRGB = configData["ESP"]?["Occluded Team RGB"]?.ToObject<bool>() ?? BoxESP.OccludedColors.TeamRGB;
                BoxESP.OccludedColors.EnemyRGB = configData["ESP"]?["Occluded Enemy RGB"]?.ToObject<bool>() ?? BoxESP.OccludedColors.EnemyRGB;
                #endregion

                #region Tracers
                Tracers.EnableTracers = configData["Tracers"]?["Tracers Enabled"]?.ToObject<bool>() ?? Tracers.EnableTracers;
                Tracers.LineThickness = configData["Tracers"]?["Tracers Thickness"]?.ToObject<float>() ?? Tracers.LineThickness;
                Tracers.TeamCheck = configData["Tracers"]?["Tracers Team Check"]?.ToObject<bool>() ?? Tracers.TeamCheck;
                Tracers.CurrentStartPos = configData["Tracers"]?["Tracers Current Start"]?.ToObject<int>() ?? Tracers.CurrentStartPos;
                Tracers.CurrentEndPos = configData["Tracers"]?["Tracers Current End"]?.ToObject<int>() ?? Tracers.CurrentEndPos;
                Tracers.TeamColor.X = configData["Tracers"]?["Tracers Team Color"]?["X"]?.ToObject<float>() ?? Tracers.TeamColor.X;
                Tracers.TeamColor.Y = configData["Tracers"]?["Tracers Team Color"]?["Y"]?.ToObject<float>() ?? Tracers.TeamColor.Y;
                Tracers.TeamColor.Z = configData["Tracers"]?["Tracers Team Color"]?["Z"]?.ToObject<float>() ?? Tracers.TeamColor.Z;
                Tracers.TeamColor.W = configData["Tracers"]?["Tracers Team Color"]?["W"]?.ToObject<float>() ?? Tracers.TeamColor.W;
                Tracers.EnemyColor.X = configData["Tracers"]?["Tracers Enemy Color"]?["X"]?.ToObject<float>() ?? Tracers.EnemyColor.X;
                Tracers.EnemyColor.Y = configData["Tracers"]?["Tracers Enemy Color"]?["Y"]?.ToObject<float>() ?? Tracers.EnemyColor.Y;
                Tracers.EnemyColor.Z = configData["Tracers"]?["Tracers Enemy Color"]?["Z"]?.ToObject<float>() ?? Tracers.EnemyColor.Z;
                Tracers.EnemyColor.W = configData["Tracers"]?["Tracers Enemy Color"]?["W"]?.ToObject<float>() ?? Tracers.EnemyColor.W;
                #endregion

                #region Bone ESP
                BoneESP.EnableBoneESP = configData["Bone ESP"]?["Bone ESP Enabled"]?.ToObject<bool>() ?? BoneESP.EnableBoneESP;
                BoneESP.BoneThickness = configData["Bone ESP"]?["Bone ESP Thickness"]?.ToObject<float>() ?? BoneESP.BoneThickness;
                BoneESP.TeamCheck = configData["Bone ESP"]?["Bone ESP Team Check"]?.ToObject<bool>() ?? BoneESP.TeamCheck;
                BoneESP.GlowAmount = configData["Bone ESP"]?["Bone ESP Glow Amount"]?.ToObject<float>() ?? BoneESP.GlowAmount;
                BoneESP.VisibleColors.TeamColor = new Vector4(
                    configData["Bone ESP"]?["Bone ESP Team Colors Visible"]?["X"]?.ToObject<float>() ?? BoneESP.VisibleColors.TeamColor.X,
                    configData["Bone ESP"]?["Bone ESP Team Colors Visible"]?["Y"]?.ToObject<float>() ?? BoneESP.VisibleColors.TeamColor.Y,
                    configData["Bone ESP"]?["Bone ESP Team Colors Visible"]?["Z"]?.ToObject<float>() ?? BoneESP.VisibleColors.TeamColor.Z,
                    configData["Bone ESP"]?["Bone ESP Team Colors Visible"]?["W"]?.ToObject<float>() ?? BoneESP.VisibleColors.TeamColor.W
                );
                BoneESP.VisibleColors.EnemyColor = new Vector4(
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Visible"]?["X"]?.ToObject<float>() ?? BoneESP.VisibleColors.EnemyColor.X,
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Visible"]?["Y"]?.ToObject<float>() ?? BoneESP.VisibleColors.EnemyColor.Y,
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Visible"]?["Z"]?.ToObject<float>() ?? BoneESP.VisibleColors.EnemyColor.Z,
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Visible"]?["W"]?.ToObject<float>() ?? BoneESP.VisibleColors.EnemyColor.W
                );
                BoneESP.OccludedColors.TeamColor = new Vector4(
                    configData["Bone ESP"]?["Bone ESP Team Colors Occluded"]?["X"]?.ToObject<float>() ?? BoneESP.OccludedColors.TeamColor.X,
                    configData["Bone ESP"]?["Bone ESP Team Colors Occluded"]?["Y"]?.ToObject<float>() ?? BoneESP.OccludedColors.TeamColor.Y,
                    configData["Bone ESP"]?["Bone ESP Team Colors Occluded"]?["Z"]?.ToObject<float>() ?? BoneESP.OccludedColors.TeamColor.Z,
                    configData["Bone ESP"]?["Bone ESP Team Colors Occluded"]?["W"]?.ToObject<float>() ?? BoneESP.OccludedColors.TeamColor.W
                );
                BoneESP.OccludedColors.EnemyColor = new Vector4(
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Occluded"]?["X"]?.ToObject<float>() ?? BoneESP.OccludedColors.EnemyColor.X,
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Occluded"]?["Y"]?.ToObject<float>() ?? BoneESP.OccludedColors.EnemyColor.Y,
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Occluded"]?["Z"]?.ToObject<float>() ?? BoneESP.OccludedColors.EnemyColor.Z,
                    configData["Bone ESP"]?["Bone ESP Enemy Colors Occluded"]?["W"]?.ToObject<float>() ?? BoneESP.OccludedColors.EnemyColor.W
                );
                BoxESP.OutlineColors.TeamRGB = configData["ESP"]?["Outline Team RGB"]?.ToObject<bool>() ?? BoxESP.OutlineColors.TeamRGB;
                BoxESP.OutlineColors.EnemyRGB = configData["ESP"]?["Outline Enemy RGB"]?.ToObject<bool>() ?? BoxESP.OutlineColors.EnemyRGB;
                BoxESP.FillColors.TeamRGB = configData["ESP"]?["Fill Team RGB"]?.ToObject<bool>() ?? BoxESP.FillColors.TeamRGB;
                BoxESP.FillColors.EnemyRGB = configData["ESP"]?["Fill Enemy RGB"]?.ToObject<bool>() ?? BoxESP.FillColors.EnemyRGB;
                BoxESP.OccludedColors.TeamRGB = configData["ESP"]?["Occluded Team RGB"]?.ToObject<bool>() ?? BoxESP.OccludedColors.TeamRGB;
                BoxESP.OccludedColors.EnemyRGB = configData["ESP"]?["Occluded Enemy RGB"]?.ToObject<bool>() ?? BoxESP.OccludedColors.EnemyRGB;
                if (configData["Bone ESP"]?["Bone ESP Color"] != null)
                {
                    var legacyColor = new Vector4(
                        configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color X"]?.ToObject<float>() ?? 1f,
                        configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color Y"]?.ToObject<float>() ?? 1f,
                        configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color Z"]?.ToObject<float>() ?? 1f,
                        configData["Bone ESP"]?["Bone ESP Color"]?["Bone ESP Color W"]?.ToObject<float>() ?? 1f
                    );
                    BoneESP.VisibleColors.TeamColor = legacyColor;
                    BoneESP.VisibleColors.EnemyColor = legacyColor;
                }
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
                Chams.Enabled = configData["Chams"]?["Chams Enabled"]?.ToObject<bool>() ?? Chams.Enabled;
                Chams.StyleIndex = configData["Chams"]?["Chams Style Index"]?.ToObject<int>() ?? Chams.StyleIndex;
                Chams.TeamCheck = configData["Chams"]?["Chams Team Check"]?.ToObject<bool>() ?? Chams.TeamCheck;
                #endregion

                #region No Flash
                NoFlash.NoFlashEnable = configData["No Flash"]?["No Flash Enabled"]?.ToObject<bool>() ?? NoFlash.NoFlashEnable;
                #endregion

                #region FOV Changer
                FovChanger.Enabled = configData["FOV Changer"]?["FOV Changer Enabled"]?.ToObject<bool>() ?? FovChanger.Enabled;
                FovChanger.FOV = configData["FOV Changer"]?["FOV Changer FOV"]?.ToObject<int>() ?? FovChanger.FOV;
                #endregion

                #region Armor Bar
                ArmorBar.EnableArmorBar = configData["Armor Bar"]?["Armor Bar Enabled"]?.ToObject<bool>() ?? ArmorBar.EnableArmorBar;
                ArmorBar.DrawOnSelf = configData["Armor Bar"]?["Armor Bar Draw On Self"]?.ToObject<bool>() ?? ArmorBar.DrawOnSelf;
                ArmorBar.ArmorBarWidth = configData["Armor Bar"]?["Armor Bar Width"]?.ToObject<float>() ?? ArmorBar.ArmorBarWidth;
                #endregion

                #region Hit Actions
                HitStuff.EnableHitSounds = configData["Hit Actions"]?["Hit Sound Enabled"]?.ToObject<bool>() ?? HitStuff.EnableHitSounds;
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

                #region Spectator List
                SpectatorList.Enabled = configData["Spectator List"]?["Spectator List Enabled"]?.ToObject<bool>() ?? SpectatorList.Enabled;
                #endregion

                #region WorldESP
                WorldESP.DrawText = configData["World ESP"]?["Draw Text"]?.ToObject<bool>() ?? WorldESP.DrawText;
                WorldESP.DrawBoxes = configData["World ESP"]?["Draw Boxes"]?.ToObject<bool>() ?? WorldESP.DrawBoxes;
                WorldESP.ChickenESP = configData["World ESP"]?["Chicken ESP Enabled"]?.ToObject<bool>() ?? WorldESP.ChickenESP;
                WorldESP.MolotovBoundsESP = configData["World ESP"]?["Molotov ESP Enabled"]?.ToObject<bool>() ?? WorldESP.MolotovBoundsESP;
                WorldESP.ChickenESP = configData["World ESP"]?["Chicken ESP Enabled"]?.ToObject<bool>() ?? WorldESP.DrawText;
                WorldESP.HostageESP = configData["World ESP"]?["Hostage ESP Enabled"]?.ToObject<bool>() ?? WorldESP.HostageESP;
                WorldESP.DroppedWeaponESP = configData["World ESP"]?["Dropped Weapon ESP Enabled"]?.ToObject<bool>() ?? WorldESP.DroppedWeaponESP;
                WorldESP.ProjectileESP = configData["World ESP"]?["Thrown Projectile ESP Enabled"]?.ToObject<bool>() ?? WorldESP.ProjectileESP;
                WorldESP.ProjectileESP = configData["World ESP"]?["Molotov ESP Enabled"]?.ToObject<bool>() ?? WorldESP.MolotovBoundsESP;
                WorldESP.ChickenTextColor = new Vector4(
                    configData["World ESP"]?["Chicken Text Color"]?["X"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.X,
                    configData["World ESP"]?["Chicken Text Color"]?["Y"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.Y,
                    configData["World ESP"]?["Chicken Text Color"]?["Z"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.Z,
                    configData["World ESP"]?["Chicken Text Color"]?["W"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.W
                );
                WorldESP.ProjectileTextColor = new Vector4(
                    configData["World ESP"]?["Thrown Projectile Text Color"]?["X"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.X,
                    configData["World ESP"]?["Thrown Projectile Text Color"]?["Y"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.Y,
                    configData["World ESP"]?["Thrown Projectile Text Color"]?["Z"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.Z,
                    configData["World ESP"]?["Thrown Projectile Text Color"]?["W"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.W
                );
                WorldESP.WeaponTextColor = new Vector4(
                    configData["World ESP"]?["Dropped Weapon Text Color"]?["X"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.X,
                    configData["World ESP"]?["Dropped Weapon Text Color"]?["Y"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.Y,
                    configData["World ESP"]?["Dropped Weapon Text Color"]?["Z"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.Z,
                    configData["World ESP"]?["Dropped Weapon Text Color"]?["W"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.W
                );
                WorldESP.BoxColor = new Vector4(
                    configData["World ESP"]?["Box Color"]?["X"]?.ToObject<float>() ?? WorldESP.BoxColor.X,
                    configData["World ESP"]?["Box Color"]?["Y"]?.ToObject<float>() ?? WorldESP.BoxColor.Y,
                    configData["World ESP"]?["Box Color"]?["Z"]?.ToObject<float>() ?? WorldESP.BoxColor.Z,
                    configData["World ESP"]?["Box Color"]?["W"]?.ToObject<float>() ?? WorldESP.BoxColor.W
                );
                WorldESP.MolotovColors = new Colors(
                    primaryColor: new Vector4(
                        configData["World ESP"]?["Molotov Bounds Filled Color"]?["X"]?.ToObject<float>() ?? WorldESP.MolotovColors.PrimaryColor.X,
                        configData["World ESP"]?["Molotov Bounds Filled Color"]?["Y"]?.ToObject<float>() ?? WorldESP.MolotovColors.PrimaryColor.Y,
                        configData["World ESP"]?["Molotov Bounds Filled Color"]?["Z"]?.ToObject<float>() ?? WorldESP.MolotovColors.PrimaryColor.Z,
                        configData["World ESP"]?["Molotov Bounds Filled Color"]?["W"]?.ToObject<float>() ?? WorldESP.MolotovColors.PrimaryColor.W
                    ),
                    secondaryColor: new Vector4(
                        configData["World ESP"]?["Molotov Bounds Outline Color"]?["X"]?.ToObject<float>() ?? WorldESP.MolotovColors.SecondaryColor.X,
                        configData["World ESP"]?["Molotov Bounds Outline Color"]?["Y"]?.ToObject<float>() ?? WorldESP.MolotovColors.SecondaryColor.Y,
                        configData["World ESP"]?["Molotov Bounds Outline Color"]?["Z"]?.ToObject<float>() ?? WorldESP.MolotovColors.SecondaryColor.Z,
                        configData["World ESP"]?["Molotov Bounds Outline Color"]?["W"]?.ToObject<float>() ?? WorldESP.MolotovColors.SecondaryColor.W
                    )
                );
                #endregion
                #region Flags
                DistanceText.Enabled = (configData["Flags"] != null
                    ? configData["Flags"]?["Enable Distance Tracker"]?.ToObject<bool>()
                    : configData["ESP"]?["Enable Distance Tracker"]?.ToObject<bool>())
                    ?? DistanceText.Enabled;

                NameDisplay.Enabled = (configData["Flags"] != null
                    ? configData["Flags"]?["Name Display Enabled"]?.ToObject<bool>()
                    : configData["Name Display"]?["Name Display Enabled"]?.ToObject<bool>())
                    ?? NameDisplay.Enabled;

                Flags.GunEnabled = (configData["Flags"]?["Gun Display Enabled"] ?? configData["Flags"]?["Gun Enabled"])?.ToObject<bool>() ?? Flags.GunEnabled;
                Flags.TextColors.TeamColor = new Vector4(
                    configData["Flags"]?["Text Team Color"]?["X"]?.ToObject<float>() ?? Flags.TextColors.TeamColor.X,
                    configData["Flags"]?["Text Team Color"]?["Y"]?.ToObject<float>() ?? Flags.TextColors.TeamColor.Y,
                    configData["Flags"]?["Text Team Color"]?["Z"]?.ToObject<float>() ?? Flags.TextColors.TeamColor.Z,
                    configData["Flags"]?["Text Team Color"]?["W"]?.ToObject<float>() ?? Flags.TextColors.TeamColor.W
                );
                Flags.TextColors.EnemyColor = new Vector4(
                    configData["Flags"]?["Text Enemy Color"]?["X"]?.ToObject<float>() ?? Flags.TextColors.EnemyColor.X,
                    configData["Flags"]?["Text Enemy Color"]?["Y"]?.ToObject<float>() ?? Flags.TextColors.EnemyColor.Y,
                    configData["Flags"]?["Text Enemy Color"]?["Z"]?.ToObject<float>() ?? Flags.TextColors.EnemyColor.Z,
                    configData["Flags"]?["Text Enemy Color"]?["W"]?.ToObject<float>() ?? Flags.TextColors.EnemyColor.W
                );
                PingDisplay.Enabled = configData["Flags"]?["Ping Display Enabled"]?.ToObject<bool>() ?? PingDisplay.Enabled;
                PingDisplay.PingTextColor = new Vector4(
                    configData["Flags"]?["Ping Display Color"]?["X"]?.ToObject<float>() ?? PingDisplay.PingTextColor.X,
                    configData["Flags"]?["Ping Display Color"]?["Y"]?.ToObject<float>() ?? PingDisplay.PingTextColor.Y,
                    configData["Flags"]?["Ping Display Color"]?["Z"]?.ToObject<float>() ?? PingDisplay.PingTextColor.Z,
                    configData["Flags"]?["Ping Display Color"]?["W"]?.ToObject<float>() ?? PingDisplay.PingTextColor.W
                );
                Flags.FlashEnabled = configData["Flags"]?["Flash Enabled"]?.ToObject<bool>() ?? Flags.FlashEnabled;
                Flags.ScopedEnabled = configData["Flags"]?["Scoped Enabled"]?.ToObject<bool>() ?? Flags.ScopedEnabled;

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
            var files = Directory.EnumerateFiles(Configs.ConfigDirPath).Select(Path.GetFileName).Where(f => f != null).ToHashSet(); // refresh so if any thing changes the disc updates
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

            Thread.Sleep(500);
        }
    }
}