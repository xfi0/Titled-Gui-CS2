using ImGuiNET;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.HvH;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Classes
{
    internal class Configs : Classes.ThreadService
    {
        public static string MenuName = "Tutamaka";
        public static string Version = "1.4.0";
        public static string Author = "github.com/xfi0";
        public static string Link = "https://github.com/xfi0/Titled-Gui-CS2";

        public static string ConfigName = "";
        public static ConcurrentDictionary<string, bool> SavedConfigs = new();
        public static string SelectedConfig = "";
        public static string ShareCode = "";
        public static string EditConfigName = "";
        public static string EditingConfig = null;
        public static string ShareCodeInput = "";
        public static string ShareMessage = "";
        public static string ShareMessageType = "";
        public static bool ShowLoadSharedWindow = false;
        public static bool ShowCreatePopup = false;
        public static bool ShowLoadSharedPopup = false;
        public static bool ShowSharePopup = false;
        public static bool ShowDeletePopup = false;

        public static string NewConfigName = "";
        public static string ShareConfigName = "";
        public static string GeneratedShareCode = "";
        public static string ConfigToDelete = "";

        public static bool ShowEditPopup = false;

        public static string LastMessage = "";
        public static string LastMessageType = ""; // "success" or "error"

        public static readonly string ConfigDirPath = Path.Combine(AppContext.BaseDirectory, "Configs");
        public static string JsonString = "";

        // Baza danych MySQL
        private static readonly string dbHost = "<db_host>";
        private static readonly string dbUser = "<db_user>";
        private static readonly string dbPass = "<sb_pass>";
        private static readonly string dbName = "<db_name>";
        private static string ConnectionString =>
            $"Server={dbHost};Port=3306;Database={dbName};Uid={dbUser};Pwd={dbPass};" +
            $"CharSet=utf8;ConnectionTimeout=30;SslMode=Preferred;AllowPublicKeyRetrieval=true;" +
            $"Pooling=true;MinPoolSize=0;MaxPoolSize=100;";

        // Tabele
        private const string ConfigsTable = "tutamaka_configs";
        private const string SharedConfigsTable = "tutamaka_shared_configs";

        static Configs()
        {
            InitializeDatabase();
        }

        public static void ClearMessage()
        {
            LastMessage = "";
            LastMessageType = "";
        }

        private static void InitializeDatabase()
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                // Tabela lokalnych konfigów
                string createConfigsTable = $@"
                CREATE TABLE IF NOT EXISTS {ConfigsTable} (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    config_data LONGTEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE KEY unique_name (name)
                )";

                // Tabela udostępnionych konfigów
                string createSharedTable = $@"
                CREATE TABLE IF NOT EXISTS {SharedConfigsTable} (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    share_code VARCHAR(9) NOT NULL,
                    config_name VARCHAR(255) NOT NULL,
                    config_data LONGTEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    expires_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP + INTERVAL 30 DAY),
                    UNIQUE KEY unique_share_code (share_code),
                    INDEX idx_expires (expires_at)
                )";

                using var cmd1 = new MySqlCommand(createConfigsTable, connection);
                cmd1.ExecuteNonQuery();

                using var cmd2 = new MySqlCommand(createSharedTable, connection);
                cmd2.ExecuteNonQuery();

                Console.WriteLine("Database tables initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
            }
        }

        public static void SaveConfig(string fileName)
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            JObject configData = CreateConfigJson();

            Directory.CreateDirectory(ConfigDirPath);
            string fullPath = Path.Combine(ConfigDirPath, fileName);
            File.WriteAllText(fullPath, configData.ToString());

            Console.WriteLine($"Wrote {fileName} at {fullPath}");

            // Zapisz także do bazy danych
            SaveConfigToDatabase(fileName, configData.ToString());

            SavedConfigs.TryAdd(fileName, true);
        }

        private static void SaveConfigToDatabase(string name, string configData)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = $@"
                INSERT INTO {ConfigsTable} (name, config_data) 
                VALUES (@name, @data)
                ON DUPLICATE KEY UPDATE config_data = @data";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@data", configData);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config to database: {ex.Message}");
            }
        }

        public static void RenameConfig(string oldName, string newName)
        {
            try
            {
                if (!oldName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    oldName += ".json";

                if (!newName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    newName += ".json";

                string oldPath = Path.Combine(ConfigDirPath, oldName);
                string newPath = Path.Combine(ConfigDirPath, newName);

                if (File.Exists(oldPath))
                {
                    File.Move(oldPath, newPath);

                    // Zaktualizuj w bazie danych
                    using var connection = new MySqlConnection(ConnectionString);
                    connection.Open();

                    string query = $@"
                    UPDATE {ConfigsTable} 
                    SET name = @newName 
                    WHERE name = @oldName";

                    using var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@newName", newName);
                    cmd.Parameters.AddWithValue("@oldName", oldName);
                    cmd.ExecuteNonQuery();

                    // Zaktualizuj słownik
                    SavedConfigs.TryRemove(oldName, out _);
                    SavedConfigs.TryAdd(newName, true);

                    if (SelectedConfig == oldName)
                    {
                        SelectedConfig = newName;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming config: {ex.Message}");
            }
        }

        public static void CreateNewFromCurrent()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ConfigName))
                {
                    ConfigName = $"config_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                }

                SaveConfig(ConfigName);

                // Zaktualizuj listę
                if (!SavedConfigs.ContainsKey(ConfigName))
                {
                    SavedConfigs.TryAdd(ConfigName, true);
                }

                SelectedConfig = ConfigName;

                Console.WriteLine($"Created new config from current settings: {ConfigName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating new config: {ex.Message}");
            }
        }

        private static void RenderConfigItem(string configName)
        {
            bool isSelected = Configs.SelectedConfig == configName;

            ImGui.PushID(configName);

            // Wiersz z przyciskami i nazwą
            ImGui.Columns(3, $"config_row_{configName}", false);

            // Przycisk Load
            if (ImGui.Button("Load", new Vector2(60, 30)))
            {
                Configs.LoadConfig(configName);
                Configs.SelectedConfig = configName;
            }

            ImGui.NextColumn();

            // Nazwa konfigu
            if (isSelected)
            {
                Vector2 cursorPos = ImGui.GetCursorScreenPos();
                Vector2 size = new Vector2(ImGui.GetColumnWidth(), 30);

                ImGui.GetWindowDrawList().AddRectFilled(
                    cursorPos,
                    cursorPos + size,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(
                        Renderer.CurrentAccentColor.X * 0.3f,
                        Renderer.CurrentAccentColor.Y * 0.3f,
                        Renderer.CurrentAccentColor.Z * 0.3f,
                        0.5f
                    )),
                    5.0f
                );
            }

            if (ImGui.Selectable(configName, isSelected, ImGuiSelectableFlags.SpanAllColumns,
                new Vector2(0, 30)))
            {
                Configs.SelectedConfig = configName;
            }

            ImGui.NextColumn();

            // Przycisk Delete
            if (ImGui.Button("Delete", new Vector2(60, 30)))
            {
                Configs.DeleteConfig(configName);
                if (Configs.SelectedConfig == configName)
                {
                    Configs.SelectedConfig = "";
                }
            }

            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void DeleteConfig(string configName)
        {
            if (!configName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                configName += ".json";

            try
            {
                // Usuń z pliku
                string fullPath = Path.Combine(ConfigDirPath, configName);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Console.WriteLine($"Deleted config file: {configName}");
                }

                // Usuń z bazy danych
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = $"DELETE FROM {ConfigsTable} WHERE name = @name";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", configName);
                cmd.ExecuteNonQuery();

                // Usuń ze słownika
                SavedConfigs.TryRemove(configName, out _);

                if (SelectedConfig == configName)
                {
                    SelectedConfig = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting config: {ex.Message}");
            }
        }

        public static string ShareConfig(string configName)
        {
            try
            {
                if (!configName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    configName += ".json";

                // Pobierz konfig z bazy danych
                string configData = "";
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT config_data FROM {ConfigsTable} WHERE name = @name";
                    using var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", configName);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        configData = result.ToString();
                }

                if (string.IsNullOrEmpty(configData))
                {
                    // Spróbuj z pliku
                    string fullPath = Path.Combine(ConfigDirPath, configName);
                    if (File.Exists(fullPath))
                        configData = File.ReadAllText(fullPath);
                }

                if (string.IsNullOrEmpty(configData))
                    return "Error: Config not found";

                // Generuj 9-cyfrowy kod
                string shareCode = GenerateShareCode();

                // Zapisz do tabeli udostępnionych
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = $@"
                    INSERT INTO {SharedConfigsTable} (share_code, config_name, config_data) 
                    VALUES (@code, @name, @data)";

                    using var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@code", shareCode);
                    cmd.Parameters.AddWithValue("@name", configName);
                    cmd.Parameters.AddWithValue("@data", configData);
                    cmd.ExecuteNonQuery();
                }

                // Wyczyść stare rekordy (starsze niż 30 dni)
                CleanupOldSharedConfigs();

                return shareCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sharing config: {ex.Message}");
                return "Error: " + ex.Message;
            }
        }

        public static bool LoadSharedConfig(string shareCode)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = $@"
                SELECT config_name, config_data 
                FROM {SharedConfigsTable} 
                WHERE share_code = @code 
                AND (expires_at IS NULL OR expires_at > NOW())";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@code", shareCode);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string configName = reader.GetString("config_name");
                    string configData = reader.GetString("config_data");

                    // Załaduj konfig od razu
                    LoadConfigFromJson(configData);

                    // Zapisz jako lokalny config
                    string fileName = $"shared_{shareCode}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    string fullPath = Path.Combine(ConfigDirPath, fileName);
                    File.WriteAllText(fullPath, configData);

                    // Dodaj do listy
                    SavedConfigs.TryAdd(fileName, true);
                    SelectedConfig = fileName;
                    ConfigName = Path.GetFileNameWithoutExtension(fileName);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shared config: {ex.Message}");
                return false;
            }
        }

        private static void CleanupOldSharedConfigs()
        {
            try
            {
                Task.Run(() =>
                {
                    using var connection = new MySqlConnection(ConnectionString);
                    connection.Open();

                    string query = $"DELETE FROM {SharedConfigsTable} WHERE expires_at < NOW()";
                    using var cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                });
            }
            catch { /* Ignore cleanup errors */ }
        }

        private static string GenerateShareCode()
        {
            var random = new Random();
            return random.Next(100000000, 999999999).ToString();
        }

        private static JObject CreateConfigJson()
        {
            return new JObject()
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
                        ["W"] = BoneESP.BoneColor.W,
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
                    },
                    ["Aimbot Visibility Check"] = Aimbot.VisibilityCheck,
                    ["Aimbot Target Line"] = Aimbot.targetLine
                },
                ["RCS"] = new JObject
                {
                    ["RCS Enabled"] = RCS.enabled,
                    ["RCS Strength"] = RCS.strength
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
                ["BulletTrail"] = new JObject
                {
                    ["EnableBulletTrails"] = BulletTrailManager.EnableBulletTrails,
                    ["UseRealBulletImpact"] = BulletTrailManager.UseRealBulletImpact,
                    ["StartColor"] = new JObject
                    {
                        ["X"] = BulletTrailManager.StartColor.X,
                        ["Y"] = BulletTrailManager.StartColor.Y,
                        ["Z"] = BulletTrailManager.StartColor.Z,
                        ["W"] = BulletTrailManager.StartColor.W
                    },
                    ["EndColor"] = new JObject
                    {
                        ["X"] = BulletTrailManager.EndColor.X,
                        ["Y"] = BulletTrailManager.EndColor.Y,
                        ["Z"] = BulletTrailManager.EndColor.Z,
                        ["W"] = BulletTrailManager.EndColor.W
                    },
                    ["TrailLifetime"] = BulletTrailManager.TrailLifetime,
                    ["TrailThickness"] = BulletTrailManager.TrailThickness,
                    ["TrailOpacity"] = BulletTrailManager.TrailOpacity,
                    ["MaxTrails"] = BulletTrailManager.MaxTrails,
                    ["ShowGlowEffect"] = BulletTrailManager.ShowGlowEffect,
                    ["TrailLength"] = BulletTrailManager.TrailLength,
                    ["SparkSize"] = BulletTrailManager.SparkSize,
                    ["GlowIntensity"] = BulletTrailManager.GlowIntensity,
                    ["TrailSegments"] = BulletTrailManager.TrailSegments,
                    ["AlwaysShowTrail"] = BulletTrailManager.AlwaysShowTrail,
                    ["SparkStartColor"] = new JObject
                    {
                        ["X"] = BulletTrailManager.SparkStartColor.X,
                        ["Y"] = BulletTrailManager.SparkStartColor.Y,
                        ["Z"] = BulletTrailManager.SparkStartColor.Z,
                        ["W"] = BulletTrailManager.SparkStartColor.W
                    },
                    ["SparkEndColor"] = new JObject
                    {
                        ["X"] = BulletTrailManager.SparkEndColor.X,
                        ["Y"] = BulletTrailManager.SparkEndColor.Y,
                        ["Z"] = BulletTrailManager.SparkEndColor.Z,
                        ["W"] = BulletTrailManager.SparkEndColor.W
                    },
                    ["RealTrailSparkStartColor"] = new JObject
                    {
                        ["X"] = BulletTrailManager.RealTrailSparkStartColor.X,
                        ["Y"] = BulletTrailManager.RealTrailSparkStartColor.Y,
                        ["Z"] = BulletTrailManager.RealTrailSparkStartColor.Z,
                        ["W"] = BulletTrailManager.RealTrailSparkStartColor.W
                    },
                    ["RealTrailSparkEndColor"] = new JObject
                    {
                        ["X"] = BulletTrailManager.RealTrailSparkEndColor.X,
                        ["Y"] = BulletTrailManager.RealTrailSparkEndColor.Y,
                        ["Z"] = BulletTrailManager.RealTrailSparkEndColor.Z,
                        ["W"] = BulletTrailManager.RealTrailSparkEndColor.W
                    }
                },
                ["C4ESP"] = new JObject
                {
                    ["BoxEnabled"] = C4ESP.BoxEnabled,
                    ["TextEnabled"] = C4ESP.TextEnabled,
                    ["BoxColor"] = new JObject
                    {
                        ["X"] = C4ESP.BoxColor.X,
                        ["Y"] = C4ESP.BoxColor.Y,
                        ["Z"] = C4ESP.BoxColor.Z,
                        ["W"] = C4ESP.BoxColor.W
                    },
                    ["TextColor"] = new JObject
                    {
                        ["X"] = C4ESP.TextColor.X,
                        ["Y"] = C4ESP.TextColor.Y,
                        ["Z"] = C4ESP.TextColor.Z,
                        ["W"] = C4ESP.TextColor.W
                    }
                },
                ["WorldESP"] = new JObject
                {
                    ["DroppedWeaponESP"] = WorldESP.DroppedWeaponESP,
                    ["WeaponTextColor"] = new JObject
                    {
                        ["X"] = WorldESP.WeaponTextColor.X,
                        ["Y"] = WorldESP.WeaponTextColor.Y,
                        ["Z"] = WorldESP.WeaponTextColor.Z,
                        ["W"] = WorldESP.WeaponTextColor.W
                    },
                    ["HostageESP"] = WorldESP.HostageESP,
                    ["HostageTextColor"] = new JObject
                    {
                        ["X"] = WorldESP.HostageTextColor.X,
                        ["Y"] = WorldESP.HostageTextColor.Y,
                        ["Z"] = WorldESP.HostageTextColor.Z,
                        ["W"] = WorldESP.HostageTextColor.W
                    },
                    ["HostageBoxColor"] = new JObject
                    {
                        ["X"] = WorldESP.HostageBoxColor.X,
                        ["Y"] = WorldESP.HostageBoxColor.Y,
                        ["Z"] = WorldESP.HostageBoxColor.Z,
                        ["W"] = WorldESP.HostageBoxColor.W
                    },
                    ["ChickenESP"] = WorldESP.ChickenESP,
                    ["ChickenTextColor"] = new JObject
                    {
                        ["X"] = WorldESP.ChickenTextColor.X,
                        ["Y"] = WorldESP.ChickenTextColor.Y,
                        ["Z"] = WorldESP.ChickenTextColor.Z,
                        ["W"] = WorldESP.ChickenTextColor.W
                    },
                    ["ChickenBoxColor"] = new JObject
                    {
                        ["X"] = WorldESP.ChickenBoxColor.X,
                        ["Y"] = WorldESP.ChickenBoxColor.Y,
                        ["Z"] = WorldESP.ChickenBoxColor.Z,
                        ["W"] = WorldESP.ChickenBoxColor.W
                    },
                    ["ProjectileESP"] = WorldESP.ProjectileESP,
                    ["ProjectileTextColor"] = new JObject
                    {
                        ["X"] = WorldESP.ProjectileTextColor.X,
                        ["Y"] = WorldESP.ProjectileTextColor.Y,
                        ["Z"] = WorldESP.ProjectileTextColor.Z,
                        ["W"] = WorldESP.ProjectileTextColor.W
                    }
                },
                ["SoundESP"] = new JObject
                {
                    ["enabled"] = SoundESP.enabled,
                    ["color"] = new JObject
                    {
                        ["X"] = SoundESP.color.X,
                        ["Y"] = SoundESP.color.Y,
                        ["Z"] = SoundESP.color.Z,
                        ["W"] = SoundESP.color.W
                    }
                },
                ["GunDisplay"] = new JObject
                {
                    ["Enabled"] = GunDisplay.Enabled,
                    ["TextColor"] = new JObject
                    {
                        ["X"] = GunDisplay.TextColor.X,
                        ["Y"] = GunDisplay.TextColor.Y,
                        ["Z"] = GunDisplay.TextColor.Z,
                        ["W"] = GunDisplay.TextColor.W
                    }
                },
                ["SilentAim"] = new JObject
                {
                    ["Enabled"] = SilentAimManager.Enabled,
                    ["TeamCheck"] = SilentAimManager.TeamCheck,
                    ["VisibilityCheck"] = SilentAimManager.VisibilityCheck,

                },
                ["SpinBot"] = new JObject
                {
                    ["SpinbotEnabled"] = SpinBot.SpinbotEnabled,
                    ["currentSpinbotMode"] = SpinBot.currentSpinbotMode,
                    ["SpinbotSpeed"] = SpinBot.SpinbotSpeed,
                    ["EnableX"] = SpinBot.EnableX,
                    ["EnableY"] = SpinBot.EnableY,
                    ["EnableZ"] = SpinBot.EnableZ,
                    ["MovementFix"] = SpinBot.MovementFix,
                    ["JiggleIntensity"] = SpinBot.JiggleIntensity,
                    ["JiggleFrequency"] = SpinBot.JiggleFrequency,
                    ["RandomMin"] = SpinBot.RandomMin,
                    ["RandomMax"] = SpinBot.RandomMax,
                    ["AntiAimEnabled"] = SpinBot.AntiAimEnabled,
                    ["CustomAngleX"] = SpinBot.CustomAngleX,
                    ["CustomAngleY"] = SpinBot.CustomAngleY,
                    ["CustomAngleZ"] = SpinBot.CustomAngleZ,
                    ["PitchAngle"] = SpinBot.PitchAngle,
                    ["YawAngle"] = SpinBot.YawAngle
                },
                ["SpectatorList"] = new JObject
                {
                    ["Enabled"] = SpectatorList.Enabled,
                    ["Position"] = new JObject
                    {
                        ["X"] = SpectatorList.Position.X,
                        ["Y"] = SpectatorList.Position.Y
                    },
                    ["WindowBgColor"] = new JObject
                    {
                        ["X"] = SpectatorList.WindowBgColor.X,
                        ["Y"] = SpectatorList.WindowBgColor.Y,
                        ["Z"] = SpectatorList.WindowBgColor.Z,
                        ["W"] = SpectatorList.WindowBgColor.W
                    },
                    ["BorderColor"] = new JObject
                    {
                        ["X"] = SpectatorList.BorderColor.X,
                        ["Y"] = SpectatorList.BorderColor.Y,
                        ["Z"] = SpectatorList.BorderColor.Z,
                        ["W"] = SpectatorList.BorderColor.W
                    },
                    ["TextColor"] = new JObject
                    {
                        ["X"] = SpectatorList.TextColor.X,
                        ["Y"] = SpectatorList.TextColor.Y,
                        ["Z"] = SpectatorList.TextColor.Z,
                        ["W"] = SpectatorList.TextColor.W
                    },
                    ["AccentColor"] = new JObject
                    {
                        ["X"] = SpectatorList.AccentColor.X,
                        ["Y"] = SpectatorList.AccentColor.Y,
                        ["Z"] = SpectatorList.AccentColor.Z,
                        ["W"] = SpectatorList.AccentColor.W
                    },
                    ["Width"] = SpectatorList.Width,
                    ["ItemHeight"] = SpectatorList.ItemHeight,
                    ["HeaderHeight"] = SpectatorList.HeaderHeight,
                    ["Padding"] = SpectatorList.Padding,
                    ["Rounding"] = SpectatorList.Rounding
                },
                ["ArrowESP"] = new JObject
                {
                    ["Enabled"] = ArrowESP.Enabled,
                    ["ShowTeam"] = ArrowESP.ShowTeam,
                    ["ShowDistanceText"] = ArrowESP.ShowDistanceText,
                    ["AlwaysShowDistance"] = ArrowESP.AlwaysShowDistance,
                    ["EnableGlow"] = ArrowESP.EnableGlow,
                    ["GlowAmount"] = ArrowESP.GlowAmount,
                    ["GlowColor"] = new JObject
                    {
                        ["X"] = ArrowESP.GlowColor.X,
                        ["Y"] = ArrowESP.GlowColor.Y,
                        ["Z"] = ArrowESP.GlowColor.Z,
                        ["W"] = ArrowESP.GlowColor.W
                    },
                    ["ArrowSize"] = ArrowESP.ArrowSize,
                    ["UseDynamicSize"] = ArrowESP.UseDynamicSize,
                    ["MinArrowSize"] = ArrowESP.MinArrowSize,
                    ["MaxArrowSize"] = ArrowESP.MaxArrowSize,
                    ["ArrowDistance"] = ArrowESP.ArrowDistance,
                    ["MinDistanceToShow"] = ArrowESP.MinDistanceToShow,
                    ["MaxArrowDistance"] = ArrowESP.MaxArrowDistance,
                    ["ArrowThickness"] = ArrowESP.ArrowThickness,
                    ["ShowHealthBar"] = ArrowESP.ShowHealthBar,
                    ["HealthBarHeight"] = ArrowESP.HealthBarHeight,
                    ["ShowName"] = ArrowESP.ShowName,
                    ["FadeByDistance"] = ArrowESP.FadeByDistance,
                    ["ShowOutOfView"] = ArrowESP.ShowOutOfView,
                    ["TeamArrowColor"] = new JObject
                    {
                        ["X"] = ArrowESP.TeamArrowColor.X,
                        ["Y"] = ArrowESP.TeamArrowColor.Y,
                        ["Z"] = ArrowESP.TeamArrowColor.Z,
                        ["W"] = ArrowESP.TeamArrowColor.W
                    },
                    ["EnemyArrowColor"] = new JObject
                    {
                        ["X"] = ArrowESP.EnemyArrowColor.X,
                        ["Y"] = ArrowESP.EnemyArrowColor.Y,
                        ["Z"] = ArrowESP.EnemyArrowColor.Z,
                        ["W"] = ArrowESP.EnemyArrowColor.W
                    }
                },
                ["SkinChanger"] = new JObject
                {
                    ["Enabled"] = SkinChanger.Enabled
                },
                ["GUI Settings"] = new JObject
                {
                    ["EnableWaterMark"] = Renderer.EnableWaterMark,
                    ["EnableFPSLimit"] = Renderer.EnableFPSLimit,
                    ["TargetFPS"] = Renderer.TargetFPS,
                    ["EnableBackgroundMusic"] = Renderer.EnableBackgroundMusic,
                    ["MusicOnlyInMenu"] = Renderer.MusicOnlyInMenu,
                    ["OpenKey"] = (int)Renderer.OpenKey,
                    ["WindowAlpha"] = Renderer.windowAlpha,
                    ["AccentColor"] = new JObject
                    {
                        ["X"] = Renderer.accentColor.X,
                        ["Y"] = Renderer.accentColor.Y,
                        ["Z"] = Renderer.accentColor.Z,
                        ["W"] = Renderer.accentColor.W
                    },
                    ["SidebarColor"] = new JObject
                    {
                        ["X"] = Renderer.SidebarColor.X,
                        ["Y"] = Renderer.SidebarColor.Y,
                        ["Z"] = Renderer.SidebarColor.Z,
                        ["W"] = Renderer.SidebarColor.W
                    },
                    ["MainContentCol"] = new JObject
                    {
                        ["X"] = Renderer.MainContentCol.X,
                        ["Y"] = Renderer.MainContentCol.Y,
                        ["Z"] = Renderer.MainContentCol.Z,
                        ["W"] = Renderer.MainContentCol.W
                    },
                    ["TextCol"] = new JObject
                    {
                        ["X"] = Renderer.TextCol.X,
                        ["Y"] = Renderer.TextCol.Y,
                        ["Z"] = Renderer.TextCol.Z,
                        ["W"] = Renderer.TextCol.W
                    },
                  
                    ["NumberOfParticles"] = Renderer.NumberOfParticles,
                    ["ParticleSpeed"] = 0.53f, // Default value
                    ["ParticleColor"] = new JObject
                    {
                        ["X"] = Renderer.ParticleColor.X,
                        ["Y"] = Renderer.ParticleColor.Y,
                        ["Z"] = Renderer.ParticleColor.Z,
                        ["W"] = Renderer.ParticleColor.W
                    },
                    ["LineColor"] = new JObject
                    {
                        ["X"] = Renderer.LineColor.X,
                        ["Y"] = Renderer.LineColor.Y,
                        ["Z"] = Renderer.LineColor.Z,
                        ["W"] = Renderer.LineColor.W
                    },
                    ["ParticleRadius"] = Renderer.ParticleRadius,
                    ["MaxLineDistance"] = Renderer.MaxLineDistance
                }
            };
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

                LoadConfigFromJson(JsonString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void LoadConfigFromJson(string jsonData)
        {
            try
            {
                JObject configData = JObject.Parse(jsonData);

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
                Aimbot.VisibilityCheck = configData["Aimbot"]?["Aimbot Visibility Check"]?.ToObject<bool>() ?? Aimbot.VisibilityCheck;
                Aimbot.targetLine = configData["Aimbot"]?["Aimbot Target Line"]?.ToObject<bool>() ?? Aimbot.targetLine;
                #endregion

                #region RCS
                RCS.enabled = configData["RCS"]?["RCS Enabled"]?.ToObject<bool>() ?? RCS.enabled;
                RCS.strength = configData["RCS"]?["RCS Strength"]?.ToObject<float>() ?? RCS.strength;
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

                #region Bullet Trail
                BulletTrailManager.EnableBulletTrails = configData["BulletTrail"]?["EnableBulletTrails"]?.ToObject<bool>() ?? BulletTrailManager.EnableBulletTrails;
                BulletTrailManager.UseRealBulletImpact = configData["BulletTrail"]?["UseRealBulletImpact"]?.ToObject<bool>() ?? BulletTrailManager.UseRealBulletImpact;

                BulletTrailManager.StartColor = new Vector4(
                    configData["BulletTrail"]?["StartColor"]?["X"]?.ToObject<float>() ?? BulletTrailManager.StartColor.X,
                    configData["BulletTrail"]?["StartColor"]?["Y"]?.ToObject<float>() ?? BulletTrailManager.StartColor.Y,
                    configData["BulletTrail"]?["StartColor"]?["Z"]?.ToObject<float>() ?? BulletTrailManager.StartColor.Z,
                    configData["BulletTrail"]?["StartColor"]?["W"]?.ToObject<float>() ?? BulletTrailManager.StartColor.W
                );

                BulletTrailManager.EndColor = new Vector4(
                    configData["BulletTrail"]?["EndColor"]?["X"]?.ToObject<float>() ?? BulletTrailManager.EndColor.X,
                    configData["BulletTrail"]?["EndColor"]?["Y"]?.ToObject<float>() ?? BulletTrailManager.EndColor.Y,
                    configData["BulletTrail"]?["EndColor"]?["Z"]?.ToObject<float>() ?? BulletTrailManager.EndColor.Z,
                    configData["BulletTrail"]?["EndColor"]?["W"]?.ToObject<float>() ?? BulletTrailManager.EndColor.W
                );

                BulletTrailManager.TrailLifetime = configData["BulletTrail"]?["TrailLifetime"]?.ToObject<float>() ?? BulletTrailManager.TrailLifetime;
                BulletTrailManager.TrailThickness = configData["BulletTrail"]?["TrailThickness"]?.ToObject<float>() ?? BulletTrailManager.TrailThickness;
                BulletTrailManager.TrailOpacity = configData["BulletTrail"]?["TrailOpacity"]?.ToObject<float>() ?? BulletTrailManager.TrailOpacity;
                BulletTrailManager.MaxTrails = configData["BulletTrail"]?["MaxTrails"]?.ToObject<int>() ?? BulletTrailManager.MaxTrails;
                BulletTrailManager.ShowGlowEffect = configData["BulletTrail"]?["ShowGlowEffect"]?.ToObject<bool>() ?? BulletTrailManager.ShowGlowEffect;
                BulletTrailManager.TrailLength = configData["BulletTrail"]?["TrailLength"]?.ToObject<float>() ?? BulletTrailManager.TrailLength;
                BulletTrailManager.SparkSize = configData["BulletTrail"]?["SparkSize"]?.ToObject<float>() ?? BulletTrailManager.SparkSize;
                BulletTrailManager.GlowIntensity = configData["BulletTrail"]?["GlowIntensity"]?.ToObject<float>() ?? BulletTrailManager.GlowIntensity;
                BulletTrailManager.TrailSegments = configData["BulletTrail"]?["TrailSegments"]?.ToObject<int>() ?? BulletTrailManager.TrailSegments;
                BulletTrailManager.AlwaysShowTrail = configData["BulletTrail"]?["AlwaysShowTrail"]?.ToObject<bool>() ?? BulletTrailManager.AlwaysShowTrail;

                BulletTrailManager.SparkStartColor = new Vector4(
                    configData["BulletTrail"]?["SparkStartColor"]?["X"]?.ToObject<float>() ?? BulletTrailManager.SparkStartColor.X,
                    configData["BulletTrail"]?["SparkStartColor"]?["Y"]?.ToObject<float>() ?? BulletTrailManager.SparkStartColor.Y,
                    configData["BulletTrail"]?["SparkStartColor"]?["Z"]?.ToObject<float>() ?? BulletTrailManager.SparkStartColor.Z,
                    configData["BulletTrail"]?["SparkStartColor"]?["W"]?.ToObject<float>() ?? BulletTrailManager.SparkStartColor.W
                );

                BulletTrailManager.SparkEndColor = new Vector4(
                    configData["BulletTrail"]?["SparkEndColor"]?["X"]?.ToObject<float>() ?? BulletTrailManager.SparkEndColor.X,
                    configData["BulletTrail"]?["SparkEndColor"]?["Y"]?.ToObject<float>() ?? BulletTrailManager.SparkEndColor.Y,
                    configData["BulletTrail"]?["SparkEndColor"]?["Z"]?.ToObject<float>() ?? BulletTrailManager.SparkEndColor.Z,
                    configData["BulletTrail"]?["SparkEndColor"]?["W"]?.ToObject<float>() ?? BulletTrailManager.SparkEndColor.W
                );

                BulletTrailManager.RealTrailSparkStartColor = new Vector4(
                    configData["BulletTrail"]?["RealTrailSparkStartColor"]?["X"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkStartColor.X,
                    configData["BulletTrail"]?["RealTrailSparkStartColor"]?["Y"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkStartColor.Y,
                    configData["BulletTrail"]?["RealTrailSparkStartColor"]?["Z"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkStartColor.Z,
                    configData["BulletTrail"]?["RealTrailSparkStartColor"]?["W"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkStartColor.W
                );

                BulletTrailManager.RealTrailSparkEndColor = new Vector4(
                    configData["BulletTrail"]?["RealTrailSparkEndColor"]?["X"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkEndColor.X,
                    configData["BulletTrail"]?["RealTrailSparkEndColor"]?["Y"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkEndColor.Y,
                    configData["BulletTrail"]?["RealTrailSparkEndColor"]?["Z"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkEndColor.Z,
                    configData["BulletTrail"]?["RealTrailSparkEndColor"]?["W"]?.ToObject<float>() ?? BulletTrailManager.RealTrailSparkEndColor.W
                );

                BulletTrailManager.UpdateAllTrailColors();
                #endregion

                #region C4 ESP
                C4ESP.BoxEnabled = configData["C4ESP"]?["BoxEnabled"]?.ToObject<bool>() ?? C4ESP.BoxEnabled;
                C4ESP.TextEnabled = configData["C4ESP"]?["TextEnabled"]?.ToObject<bool>() ?? C4ESP.TextEnabled;
                C4ESP.BoxColor = new Vector4(
                    configData["C4ESP"]?["BoxColor"]?["X"]?.ToObject<float>() ?? C4ESP.BoxColor.X,
                    configData["C4ESP"]?["BoxColor"]?["Y"]?.ToObject<float>() ?? C4ESP.BoxColor.Y,
                    configData["C4ESP"]?["BoxColor"]?["Z"]?.ToObject<float>() ?? C4ESP.BoxColor.Z,
                    configData["C4ESP"]?["BoxColor"]?["W"]?.ToObject<float>() ?? C4ESP.BoxColor.W
                );
                C4ESP.TextColor = new Vector4(
                    configData["C4ESP"]?["TextColor"]?["X"]?.ToObject<float>() ?? C4ESP.TextColor.X,
                    configData["C4ESP"]?["TextColor"]?["Y"]?.ToObject<float>() ?? C4ESP.TextColor.Y,
                    configData["C4ESP"]?["TextColor"]?["Z"]?.ToObject<float>() ?? C4ESP.TextColor.Z,
                    configData["C4ESP"]?["TextColor"]?["W"]?.ToObject<float>() ?? C4ESP.TextColor.W
                );
                #endregion

                #region World ESP
                WorldESP.DroppedWeaponESP = configData["WorldESP"]?["DroppedWeaponESP"]?.ToObject<bool>() ?? WorldESP.DroppedWeaponESP;
                WorldESP.WeaponTextColor = new Vector4(
                    configData["WorldESP"]?["WeaponTextColor"]?["X"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.X,
                    configData["WorldESP"]?["WeaponTextColor"]?["Y"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.Y,
                    configData["WorldESP"]?["WeaponTextColor"]?["Z"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.Z,
                    configData["WorldESP"]?["WeaponTextColor"]?["W"]?.ToObject<float>() ?? WorldESP.WeaponTextColor.W
                );
                WorldESP.HostageESP = configData["WorldESP"]?["HostageESP"]?.ToObject<bool>() ?? WorldESP.HostageESP;
                WorldESP.HostageTextColor = new Vector4(
                    configData["WorldESP"]?["HostageTextColor"]?["X"]?.ToObject<float>() ?? WorldESP.HostageTextColor.X,
                    configData["WorldESP"]?["HostageTextColor"]?["Y"]?.ToObject<float>() ?? WorldESP.HostageTextColor.Y,
                    configData["WorldESP"]?["HostageTextColor"]?["Z"]?.ToObject<float>() ?? WorldESP.HostageTextColor.Z,
                    configData["WorldESP"]?["HostageTextColor"]?["W"]?.ToObject<float>() ?? WorldESP.HostageTextColor.W
                );
                WorldESP.HostageBoxColor = new Vector4(
                    configData["WorldESP"]?["HostageBoxColor"]?["X"]?.ToObject<float>() ?? WorldESP.HostageBoxColor.X,
                    configData["WorldESP"]?["HostageBoxColor"]?["Y"]?.ToObject<float>() ?? WorldESP.HostageBoxColor.Y,
                    configData["WorldESP"]?["HostageBoxColor"]?["Z"]?.ToObject<float>() ?? WorldESP.HostageBoxColor.Z,
                    configData["WorldESP"]?["HostageBoxColor"]?["W"]?.ToObject<float>() ?? WorldESP.HostageBoxColor.W
                );
                WorldESP.ChickenESP = configData["WorldESP"]?["ChickenESP"]?.ToObject<bool>() ?? WorldESP.ChickenESP;
                WorldESP.ChickenTextColor = new Vector4(
                    configData["WorldESP"]?["ChickenTextColor"]?["X"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.X,
                    configData["WorldESP"]?["ChickenTextColor"]?["Y"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.Y,
                    configData["WorldESP"]?["ChickenTextColor"]?["Z"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.Z,
                    configData["WorldESP"]?["ChickenTextColor"]?["W"]?.ToObject<float>() ?? WorldESP.ChickenTextColor.W
                );
                WorldESP.ChickenBoxColor = new Vector4(
                    configData["WorldESP"]?["ChickenBoxColor"]?["X"]?.ToObject<float>() ?? WorldESP.ChickenBoxColor.X,
                    configData["WorldESP"]?["ChickenBoxColor"]?["Y"]?.ToObject<float>() ?? WorldESP.ChickenBoxColor.Y,
                    configData["WorldESP"]?["ChickenBoxColor"]?["Z"]?.ToObject<float>() ?? WorldESP.ChickenBoxColor.Z,
                    configData["WorldESP"]?["ChickenBoxColor"]?["W"]?.ToObject<float>() ?? WorldESP.ChickenBoxColor.W
                );
                WorldESP.ProjectileESP = configData["WorldESP"]?["ProjectileESP"]?.ToObject<bool>() ?? WorldESP.ProjectileESP;
                WorldESP.ProjectileTextColor = new Vector4(
                    configData["WorldESP"]?["ProjectileTextColor"]?["X"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.X,
                    configData["WorldESP"]?["ProjectileTextColor"]?["Y"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.Y,
                    configData["WorldESP"]?["ProjectileTextColor"]?["Z"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.Z,
                    configData["WorldESP"]?["ProjectileTextColor"]?["W"]?.ToObject<float>() ?? WorldESP.ProjectileTextColor.W
                );
                #endregion

                #region Sound ESP
                SoundESP.enabled = configData["SoundESP"]?["enabled"]?.ToObject<bool>() ?? SoundESP.enabled;
                SoundESP.color = new Vector4(
                    configData["SoundESP"]?["color"]?["X"]?.ToObject<float>() ?? SoundESP.color.X,
                    configData["SoundESP"]?["color"]?["Y"]?.ToObject<float>() ?? SoundESP.color.Y,
                    configData["SoundESP"]?["color"]?["Z"]?.ToObject<float>() ?? SoundESP.color.Z,
                    configData["SoundESP"]?["color"]?["W"]?.ToObject<float>() ?? SoundESP.color.W
                );
                #endregion

                #region Gun Display
                GunDisplay.Enabled = configData["GunDisplay"]?["Enabled"]?.ToObject<bool>() ?? GunDisplay.Enabled;
                GunDisplay.TextColor = new Vector4(
                    configData["GunDisplay"]?["TextColor"]?["X"]?.ToObject<float>() ?? GunDisplay.TextColor.X,
                    configData["GunDisplay"]?["TextColor"]?["Y"]?.ToObject<float>() ?? GunDisplay.TextColor.Y,
                    configData["GunDisplay"]?["TextColor"]?["Z"]?.ToObject<float>() ?? GunDisplay.TextColor.Z,
                    configData["GunDisplay"]?["TextColor"]?["W"]?.ToObject<float>() ?? GunDisplay.TextColor.W
                );
                #endregion

                #region Silent Aim
                SilentAimManager.Enabled = configData["SilentAim"]?["Enabled"]?.ToObject<bool>() ?? SilentAimManager.Enabled;
                SilentAimManager.TeamCheck = configData["SilentAim"]?["TeamCheck"]?.ToObject<bool>() ?? SilentAimManager.TeamCheck;
                SilentAimManager.VisibilityCheck = configData["SilentAim"]?["VisibilityCheck"]?.ToObject<bool>() ?? SilentAimManager.VisibilityCheck;

                #endregion

                #region SpinBot
                SpinBot.SpinbotEnabled = configData["SpinBot"]?["SpinbotEnabled"]?.ToObject<bool>() ?? SpinBot.SpinbotEnabled;
                SpinBot.currentSpinbotMode = configData["SpinBot"]?["currentSpinbotMode"]?.ToObject<int>() ?? SpinBot.currentSpinbotMode;
                SpinBot.SpinbotSpeed = configData["SpinBot"]?["SpinbotSpeed"]?.ToObject<float>() ?? SpinBot.SpinbotSpeed;
                SpinBot.EnableX = configData["SpinBot"]?["EnableX"]?.ToObject<bool>() ?? SpinBot.EnableX;
                SpinBot.EnableY = configData["SpinBot"]?["EnableY"]?.ToObject<bool>() ?? SpinBot.EnableY;
                SpinBot.EnableZ = configData["SpinBot"]?["EnableZ"]?.ToObject<bool>() ?? SpinBot.EnableZ;
                SpinBot.MovementFix = configData["SpinBot"]?["MovementFix"]?.ToObject<bool>() ?? SpinBot.MovementFix;
                SpinBot.JiggleIntensity = configData["SpinBot"]?["JiggleIntensity"]?.ToObject<float>() ?? SpinBot.JiggleIntensity;
                SpinBot.JiggleFrequency = configData["SpinBot"]?["JiggleFrequency"]?.ToObject<float>() ?? SpinBot.JiggleFrequency;
                SpinBot.RandomMin = configData["SpinBot"]?["RandomMin"]?.ToObject<float>() ?? SpinBot.RandomMin;
                SpinBot.RandomMax = configData["SpinBot"]?["RandomMax"]?.ToObject<float>() ?? SpinBot.RandomMax;
                SpinBot.AntiAimEnabled = configData["SpinBot"]?["AntiAimEnabled"]?.ToObject<bool>() ?? SpinBot.AntiAimEnabled;
                SpinBot.CustomAngleX = configData["SpinBot"]?["CustomAngleX"]?.ToObject<float>() ?? SpinBot.CustomAngleX;
                SpinBot.CustomAngleY = configData["SpinBot"]?["CustomAngleY"]?.ToObject<float>() ?? SpinBot.CustomAngleY;
                SpinBot.CustomAngleZ = configData["SpinBot"]?["CustomAngleZ"]?.ToObject<float>() ?? SpinBot.CustomAngleZ;
                SpinBot.PitchAngle = configData["SpinBot"]?["PitchAngle"]?.ToObject<float>() ?? SpinBot.PitchAngle;
                SpinBot.YawAngle = configData["SpinBot"]?["YawAngle"]?.ToObject<float>() ?? SpinBot.YawAngle;
                #endregion

                #region SpectatorList
                SpectatorList.Enabled = configData["SpectatorList"]?["Enabled"]?.ToObject<bool>() ?? SpectatorList.Enabled;
                SpectatorList.Position = new Vector2(
                    configData["SpectatorList"]?["Position"]?["X"]?.ToObject<float>() ?? SpectatorList.Position.X,
                    configData["SpectatorList"]?["Position"]?["Y"]?.ToObject<float>() ?? SpectatorList.Position.Y
                );
                SpectatorList.WindowBgColor = new Vector4(
                    configData["SpectatorList"]?["WindowBgColor"]?["X"]?.ToObject<float>() ?? SpectatorList.WindowBgColor.X,
                    configData["SpectatorList"]?["WindowBgColor"]?["Y"]?.ToObject<float>() ?? SpectatorList.WindowBgColor.Y,
                    configData["SpectatorList"]?["WindowBgColor"]?["Z"]?.ToObject<float>() ?? SpectatorList.WindowBgColor.Z,
                    configData["SpectatorList"]?["WindowBgColor"]?["W"]?.ToObject<float>() ?? SpectatorList.WindowBgColor.W
                );
                SpectatorList.BorderColor = new Vector4(
                    configData["SpectatorList"]?["BorderColor"]?["X"]?.ToObject<float>() ?? SpectatorList.BorderColor.X,
                    configData["SpectatorList"]?["BorderColor"]?["Y"]?.ToObject<float>() ?? SpectatorList.BorderColor.Y,
                    configData["SpectatorList"]?["BorderColor"]?["Z"]?.ToObject<float>() ?? SpectatorList.BorderColor.Z,
                    configData["SpectatorList"]?["BorderColor"]?["W"]?.ToObject<float>() ?? SpectatorList.BorderColor.W
                );
                SpectatorList.TextColor = new Vector4(
                    configData["SpectatorList"]?["TextColor"]?["X"]?.ToObject<float>() ?? SpectatorList.TextColor.X,
                    configData["SpectatorList"]?["TextColor"]?["Y"]?.ToObject<float>() ?? SpectatorList.TextColor.Y,
                    configData["SpectatorList"]?["TextColor"]?["Z"]?.ToObject<float>() ?? SpectatorList.TextColor.Z,
                    configData["SpectatorList"]?["TextColor"]?["W"]?.ToObject<float>() ?? SpectatorList.TextColor.W
                );
                SpectatorList.AccentColor = new Vector4(
                    configData["SpectatorList"]?["AccentColor"]?["X"]?.ToObject<float>() ?? SpectatorList.AccentColor.X,
                    configData["SpectatorList"]?["AccentColor"]?["Y"]?.ToObject<float>() ?? SpectatorList.AccentColor.Y,
                    configData["SpectatorList"]?["AccentColor"]?["Z"]?.ToObject<float>() ?? SpectatorList.AccentColor.Z,
                    configData["SpectatorList"]?["AccentColor"]?["W"]?.ToObject<float>() ?? SpectatorList.AccentColor.W
                );
                SpectatorList.Width = configData["SpectatorList"]?["Width"]?.ToObject<float>() ?? SpectatorList.Width;
                SpectatorList.ItemHeight = configData["SpectatorList"]?["ItemHeight"]?.ToObject<float>() ?? SpectatorList.ItemHeight;
                SpectatorList.HeaderHeight = configData["SpectatorList"]?["HeaderHeight"]?.ToObject<float>() ?? SpectatorList.HeaderHeight;
                SpectatorList.Padding = configData["SpectatorList"]?["Padding"]?.ToObject<float>() ?? SpectatorList.Padding;
                SpectatorList.Rounding = configData["SpectatorList"]?["Rounding"]?.ToObject<float>() ?? SpectatorList.Rounding;
                #endregion

                #region ArrowESP
                ArrowESP.Enabled = configData["ArrowESP"]?["Enabled"]?.ToObject<bool>() ?? ArrowESP.Enabled;
                ArrowESP.ShowTeam = configData["ArrowESP"]?["ShowTeam"]?.ToObject<bool>() ?? ArrowESP.ShowTeam;
                ArrowESP.ShowDistanceText = configData["ArrowESP"]?["ShowDistanceText"]?.ToObject<bool>() ?? ArrowESP.ShowDistanceText;
                ArrowESP.AlwaysShowDistance = configData["ArrowESP"]?["AlwaysShowDistance"]?.ToObject<bool>() ?? ArrowESP.AlwaysShowDistance;
                ArrowESP.EnableGlow = configData["ArrowESP"]?["EnableGlow"]?.ToObject<bool>() ?? ArrowESP.EnableGlow;
                ArrowESP.GlowAmount = configData["ArrowESP"]?["GlowAmount"]?.ToObject<float>() ?? ArrowESP.GlowAmount;
                ArrowESP.GlowColor = new Vector4(
                    configData["ArrowESP"]?["GlowColor"]?["X"]?.ToObject<float>() ?? ArrowESP.GlowColor.X,
                    configData["ArrowESP"]?["GlowColor"]?["Y"]?.ToObject<float>() ?? ArrowESP.GlowColor.Y,
                    configData["ArrowESP"]?["GlowColor"]?["Z"]?.ToObject<float>() ?? ArrowESP.GlowColor.Z,
                    configData["ArrowESP"]?["GlowColor"]?["W"]?.ToObject<float>() ?? ArrowESP.GlowColor.W
                );
                ArrowESP.ArrowSize = configData["ArrowESP"]?["ArrowSize"]?.ToObject<float>() ?? ArrowESP.ArrowSize;
                ArrowESP.UseDynamicSize = configData["ArrowESP"]?["UseDynamicSize"]?.ToObject<bool>() ?? ArrowESP.UseDynamicSize;
                ArrowESP.MinArrowSize = configData["ArrowESP"]?["MinArrowSize"]?.ToObject<float>() ?? ArrowESP.MinArrowSize;
                ArrowESP.MaxArrowSize = configData["ArrowESP"]?["MaxArrowSize"]?.ToObject<float>() ?? ArrowESP.MaxArrowSize;
                ArrowESP.ArrowDistance = configData["ArrowESP"]?["ArrowDistance"]?.ToObject<float>() ?? ArrowESP.ArrowDistance;
                ArrowESP.MinDistanceToShow = configData["ArrowESP"]?["MinDistanceToShow"]?.ToObject<float>() ?? ArrowESP.MinDistanceToShow;
                ArrowESP.MaxArrowDistance = configData["ArrowESP"]?["MaxArrowDistance"]?.ToObject<float>() ?? ArrowESP.MaxArrowDistance;
                ArrowESP.ArrowThickness = configData["ArrowESP"]?["ArrowThickness"]?.ToObject<float>() ?? ArrowESP.ArrowThickness;
                ArrowESP.ShowHealthBar = configData["ArrowESP"]?["ShowHealthBar"]?.ToObject<bool>() ?? ArrowESP.ShowHealthBar;
                ArrowESP.HealthBarHeight = configData["ArrowESP"]?["HealthBarHeight"]?.ToObject<float>() ?? ArrowESP.HealthBarHeight;
                ArrowESP.ShowName = configData["ArrowESP"]?["ShowName"]?.ToObject<bool>() ?? ArrowESP.ShowName;
                ArrowESP.FadeByDistance = configData["ArrowESP"]?["FadeByDistance"]?.ToObject<bool>() ?? ArrowESP.FadeByDistance;
                ArrowESP.ShowOutOfView = configData["ArrowESP"]?["ShowOutOfView"]?.ToObject<bool>() ?? ArrowESP.ShowOutOfView;
                ArrowESP.TeamArrowColor = new Vector4(
                    configData["ArrowESP"]?["TeamArrowColor"]?["X"]?.ToObject<float>() ?? ArrowESP.TeamArrowColor.X,
                    configData["ArrowESP"]?["TeamArrowColor"]?["Y"]?.ToObject<float>() ?? ArrowESP.TeamArrowColor.Y,
                    configData["ArrowESP"]?["TeamArrowColor"]?["Z"]?.ToObject<float>() ?? ArrowESP.TeamArrowColor.Z,
                    configData["ArrowESP"]?["TeamArrowColor"]?["W"]?.ToObject<float>() ?? ArrowESP.TeamArrowColor.W
                );
                ArrowESP.EnemyArrowColor = new Vector4(
                    configData["ArrowESP"]?["EnemyArrowColor"]?["X"]?.ToObject<float>() ?? ArrowESP.EnemyArrowColor.X,
                    configData["ArrowESP"]?["EnemyArrowColor"]?["Y"]?.ToObject<float>() ?? ArrowESP.EnemyArrowColor.Y,
                    configData["ArrowESP"]?["EnemyArrowColor"]?["Z"]?.ToObject<float>() ?? ArrowESP.EnemyArrowColor.Z,
                    configData["ArrowESP"]?["EnemyArrowColor"]?["W"]?.ToObject<float>() ?? ArrowESP.EnemyArrowColor.W
                );
                #endregion

                #region SkinChanger
                SkinChanger.Enabled = configData["SkinChanger"]?["Enabled"]?.ToObject<bool>() ?? SkinChanger.Enabled;
                #endregion

                #region GUI Settings
                Renderer.EnableWaterMark = configData["GUI Settings"]?["EnableWaterMark"]?.ToObject<bool>() ?? Renderer.EnableWaterMark;
                Renderer.EnableFPSLimit = configData["GUI Settings"]?["EnableFPSLimit"]?.ToObject<bool>() ?? Renderer.EnableFPSLimit;
                Renderer.TargetFPS = configData["GUI Settings"]?["TargetFPS"]?.ToObject<int>() ?? Renderer.TargetFPS;
                Renderer.EnableBackgroundMusic = configData["GUI Settings"]?["EnableBackgroundMusic"]?.ToObject<bool>() ?? Renderer.EnableBackgroundMusic;
                Renderer.MusicOnlyInMenu = configData["GUI Settings"]?["MusicOnlyInMenu"]?.ToObject<bool>() ?? Renderer.MusicOnlyInMenu;
                Renderer.OpenKey = (ImGuiKey)(configData["GUI Settings"]?["OpenKey"]?.ToObject<int>() ?? (int)Renderer.OpenKey);
                Renderer.windowAlpha = configData["GUI Settings"]?["WindowAlpha"]?.ToObject<float>() ?? Renderer.windowAlpha;

                Renderer.accentColor = new Vector4(
                    configData["GUI Settings"]?["AccentColor"]?["X"]?.ToObject<float>() ?? Renderer.accentColor.X,
                    configData["GUI Settings"]?["AccentColor"]?["Y"]?.ToObject<float>() ?? Renderer.accentColor.Y,
                    configData["GUI Settings"]?["AccentColor"]?["Z"]?.ToObject<float>() ?? Renderer.accentColor.Z,
                    configData["GUI Settings"]?["AccentColor"]?["W"]?.ToObject<float>() ?? Renderer.accentColor.W
                );

                Renderer.SidebarColor = new Vector4(
                    configData["GUI Settings"]?["SidebarColor"]?["X"]?.ToObject<float>() ?? Renderer.SidebarColor.X,
                    configData["GUI Settings"]?["SidebarColor"]?["Y"]?.ToObject<float>() ?? Renderer.SidebarColor.Y,
                    configData["GUI Settings"]?["SidebarColor"]?["Z"]?.ToObject<float>() ?? Renderer.SidebarColor.Z,
                    configData["GUI Settings"]?["SidebarColor"]?["W"]?.ToObject<float>() ?? Renderer.SidebarColor.W
                );

                Renderer.MainContentCol = new Vector4(
                    configData["GUI Settings"]?["MainContentCol"]?["X"]?.ToObject<float>() ?? Renderer.MainContentCol.X,
                    configData["GUI Settings"]?["MainContentCol"]?["Y"]?.ToObject<float>() ?? Renderer.MainContentCol.Y,
                    configData["GUI Settings"]?["MainContentCol"]?["Z"]?.ToObject<float>() ?? Renderer.MainContentCol.Z,
                    configData["GUI Settings"]?["MainContentCol"]?["W"]?.ToObject<float>() ?? Renderer.MainContentCol.W
                );

                Renderer.TextCol = new Vector4(
                    configData["GUI Settings"]?["TextCol"]?["X"]?.ToObject<float>() ?? Renderer.TextCol.X,
                    configData["GUI Settings"]?["TextCol"]?["Y"]?.ToObject<float>() ?? Renderer.TextCol.Y,
                    configData["GUI Settings"]?["TextCol"]?["Z"]?.ToObject<float>() ?? Renderer.TextCol.Z,
                    configData["GUI Settings"]?["TextCol"]?["W"]?.ToObject<float>() ?? Renderer.TextCol.W
                );

             
                Renderer.NumberOfParticles = configData["GUI Settings"]?["NumberOfParticles"]?.ToObject<int>() ?? Renderer.NumberOfParticles;

                // ParticleSpeed is stored in Renderer instance, not static
                if (GameState.renderer != null)
                {
                    GameState.renderer.ParticleSpeed = configData["GUI Settings"]?["ParticleSpeed"]?.ToObject<float>() ?? 0.53f;
                }

                Renderer.ParticleColor = new Vector4(
                    configData["GUI Settings"]?["ParticleColor"]?["X"]?.ToObject<float>() ?? Renderer.ParticleColor.X,
                    configData["GUI Settings"]?["ParticleColor"]?["Y"]?.ToObject<float>() ?? Renderer.ParticleColor.Y,
                    configData["GUI Settings"]?["ParticleColor"]?["Z"]?.ToObject<float>() ?? Renderer.ParticleColor.Z,
                    configData["GUI Settings"]?["ParticleColor"]?["W"]?.ToObject<float>() ?? Renderer.ParticleColor.W
                );

                Renderer.LineColor = new Vector4(
                    configData["GUI Settings"]?["LineColor"]?["X"]?.ToObject<float>() ?? Renderer.LineColor.X,
                    configData["GUI Settings"]?["LineColor"]?["Y"]?.ToObject<float>() ?? Renderer.LineColor.Y,
                    configData["GUI Settings"]?["LineColor"]?["Z"]?.ToObject<float>() ?? Renderer.LineColor.Z,
                    configData["GUI Settings"]?["LineColor"]?["W"]?.ToObject<float>() ?? Renderer.LineColor.W
                );

                Renderer.ParticleRadius = configData["GUI Settings"]?["ParticleRadius"]?.ToObject<float>() ?? Renderer.ParticleRadius;
                Renderer.MaxLineDistance = configData["GUI Settings"]?["MaxLineDistance"]?.ToObject<float>() ?? Renderer.MaxLineDistance;
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config from JSON: {ex.Message}");
            }
        }

        protected override void FrameAction()
        {
            if (!Directory.Exists(Configs.ConfigDirPath))
            {
                Directory.CreateDirectory(Configs.ConfigDirPath);
            }
            var files = Directory.EnumerateFiles(Configs.ConfigDirPath).Select(Path.GetFileName).Where(f => f != null).ToHashSet();
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

        internal static void SetAutoLoadConfig(string selectedConfig)
        {
            throw new NotImplementedException();
        }

        internal static void DeleteSelectedConfig(string selectedConfig)
        {
            DeleteConfig(selectedConfig);
        }
    }
}