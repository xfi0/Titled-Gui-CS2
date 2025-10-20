using System.Reflection;
using System.Text.RegularExpressions;

namespace Titled_Gui.Classes
{
    internal class OffsetGetter
    {
        private static readonly Dictionary<string, int> offsets = []; // holds the resolved offsets
        private static HttpClient httpClient = new();
        static OffsetGetter() 
        {
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }
        // urls to pull the dumper outputs from
        private const string OffsetsUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.cs";
        private const string ClientDllUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.cs";
        private const string ButtonsUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/buttons.cs";
        private const string Engine2Url = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/engine2_dll.cs";

        private static string OffsetsContent = string.Empty;
        private static string ClientDllContent = string.Empty;
        private static string ButtonsContent = string.Empty;
        private static string Engine2Content = string.Empty;

        private class Offset(string name, string? className = null)
        {
            public string Name { get; } = name;
            public string? ClassName { get; } = className;
        }

        private static readonly Dictionary<string, List<Offset>> FieldNameMappings = new()
        {
            { "dwViewMatrix", new() { new Offset("dwViewMatrix") } },
            { "dwEntityList", new() { new Offset("dwEntityList") } },
            { "dwLocalPlayerPawn", new() { new Offset("dwLocalPlayerPawn") } },
            { "dwLocalPlayerController", new() { new Offset("dwLocalPlayerController") } },
            { "dwViewAngles", new() { new Offset("dwViewAngles") } },
            { "dwGlobalVars", new() { new Offset("dwGlobalVars") } },
            { "dwPlantedC4", new() { new Offset("dwPlantedC4") } },
            { "dwGameRules", new() { new Offset("dwGameRules") } },
            { "dwSensitivity", new() { new Offset("dwSensitivity") } },
            { "dwSensitivity_sensitivity", new() { new Offset("dwSensitivity_sensitivity") } },
            { "dwCSGOInput", new() { new Offset("dwCSGOInput") } },
            { "attack", new() { new Offset("attack"), new Offset("+attack") } },
            { "jump", new() { new Offset("jump"), new Offset("+jump") } },
            { "m_pCameraServices", new() { new Offset("m_pCameraServices"), new Offset("m_CameraServices") } },
            { "m_iFOV", new() { new Offset("m_iFOV"), new Offset("m_iDesiredFOV") } },
            { "m_bIsScoped", new() { new Offset("m_bIsScoped"), new Offset("m_bIsScopedIn") } },
            { "m_iHealth", new() { new Offset("m_iHealth") } },
            { "m_bSpotted", new() { new Offset("m_bSpotted"), new Offset("m_bSpottedBy") } },
            { "m_iIDEntIndex", new() { new Offset("m_iIDEntIndex") } },
            { "m_pSceneNode", new() { new Offset("m_pSceneNode"), new Offset("m_pRenderingNode") } },
            { "m_vecViewOffset", new() { new Offset("m_vecViewOffset"), new Offset("m_vViewOffset") } },
            { "m_lifeState", new() { new Offset("m_lifeState") } },
            { "m_vOldOrigin", new() { new Offset("m_vOldOrigin") } },
            { "m_iTeamNum", new() { new Offset("m_iTeamNum") } },
            { "m_hPlayerPawn", new() { new Offset("m_hPlayerPawn") } },
            { "m_flFlashBangTime", new() { new Offset("m_flFlashBangTime"), new Offset("m_flFlashDuration") } },
            { "m_modelState", new() { new Offset("m_modelState") } },
            { "m_pGameSceneNode", new() { new Offset("m_pGameSceneNode") } },
            { "m_flC4Blow", new() { new Offset("m_flC4Blow"), new Offset("m_flDetonateTime") } },
            { "current_time", new() { new Offset("current_time"), new Offset("m_flCurrentTime") } },
            { "m_bBombPlanted", new() { new Offset("m_bBombPlanted"), new Offset("m_bBombTicking") } },
            { "m_iszPlayerName", new() { new Offset("m_iszPlayerName") } },
            { "m_pClippingWeapon", new() { new Offset("m_pClippingWeapon") } },
            { "m_Item", new() { new Offset("m_Item"), new Offset("m_hActiveWeapon") } },
            { "m_iItemDefinitionIndex", new() { new Offset("m_iItemDefinitionIndex") } },
            { "m_AttributeManager", new() { new Offset("m_AttributeManager") } },
            { "m_bSpottedByMask", new() { new Offset("m_bSpottedByMask") } },
            { "m_pWeaponServices", new() { new Offset("m_pWeaponServices") } },
            { "m_hActiveWeapon", new() { new Offset("m_hActiveWeapon") } },
            { "m_vecAbsVelocity", new() { new Offset("m_vecAbsVelocity") } },
            { "m_fFlags", new() { new Offset("m_fFlags") } },
            { "m_hMyWeapons", new() { new Offset("m_hMyWeapons") } },
            { "m_aimPunchAngle", new() { new Offset("m_aimPunchAngle") } },
            { "m_nCurrentTickThisFrame", new() { new Offset("m_nCurrentTickThisFrame") } },
            { "m_ArmorValue", new() { new Offset("m_ArmorValue") } },
            { "m_pInGameMoneyServices", new() { new Offset("m_pInGameMoneyServices") } },
            { "m_iAccount", new() { new Offset("m_iAccount") } },
            { "m_iTotalCashSpent", new() { new Offset("m_iTotalCashSpent") } },
            { "m_iCashSpentThisRound", new() { new Offset("m_iCashSpentThisRound") } },
            { "m_bIsDefusing", new() { new Offset("m_bIsDefusing") } },
            { "m_bInBombZone", new() { new Offset("m_bInBombZone") } },
            { "m_bIsBuyMenuOpen", new() { new Offset("m_bIsBuyMenuOpen") } },
            { "m_aimPunchCache", new() { new Offset("m_aimPunchCache") } },
            { "m_iAmmo", new() { new Offset("m_iAmmo") } },
            { "m_iPing", new() { new Offset("m_iPing") } },
            { "m_bIsWalking", new() { new Offset("m_bIsWalking") } },
            { "m_totalHitsOnServer", new() { new Offset("m_totalHitsOnServer") } },
            { "m_angEyeAngles", new() { new Offset("m_angEyeAngles") } },
            { "m_iSpectatorSlotCount", new() { new Offset("m_iSpectatorSlotCount") } },
            { "m_iShotsFired", new() { new Offset("m_iShotsFired") } },
            { "m_vecAbsOrigin", new() { new Offset("m_vecAbsOrigin") } },
            { "m_GunGameImmunityColor", new() { new Offset("m_GunGameImmunityColor") } },

            // EXPLICIT CLASS THING
            { "m_pActionTrackingServices", new() { new Offset("m_pActionTrackingServices", "CCSPlayerController") } },
            { "m_pBulletServices", new() { new Offset("m_pBulletServices", "CCSPlayerController") } },
            { "m_flTotalRoundDamageDealt", new() { new Offset("m_flTotalRoundDamageDealt", "CCSPlayerController") } },
            { "m_iNumRoundKills", new() { new Offset("m_iNumRoundKills", "CCSPlayerController_ActionTrackingServices") } },
            { "m_iNumRoundKillsHeadshots", new() { new Offset("m_iNumRoundKillsHeadshots", "CCSPlayerController_ActionTrackingServices") } },
            { "m_entitySpottedState", new() { new Offset("m_entitySpottedState", "C_CSPlayerPawn") } },
            //{ "m_bBombPlanted", new() { new Offset("m_bBombPlanted", "C_CSGameRules") } },

        };

        public static async Task UpdateOffsetsAsync()
        {
            try
            {
                Console.WriteLine("[OFFSET FINDER] Starting The Offset Finding Process Please Wait...");
                offsets.Clear();

                // download and cache
                OffsetsContent = await DownloadFile(OffsetsUrl);
                ClientDllContent = await DownloadFile(ClientDllUrl);
                ButtonsContent = await DownloadFile(ButtonsUrl);
                Engine2Content = await DownloadFile(Engine2Url);

                ParseOffsetsFile(OffsetsContent);
                ParseClientDllFile(ClientDllContent);
                ParseButtonsFile(ButtonsContent);
                ParseEngine2File(Engine2Content);

                Console.WriteLine($"[OFFSET FINDER] Found: {offsets.Count}");
                UpdateOffsetsClass();
                Console.WriteLine("[OFFSET FINDER] Offsets Updated Successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OFFSETS FINDER] ERROR: {ex.Message}");
            }
        }

        private static async Task<string> DownloadFile(string url)
        {
            try
            {
                Console.WriteLine($"[OFFSET FINDER] Downloading {url}");
                string content = await httpClient.GetStringAsync(url);

                Console.WriteLine($"[OFFSET FINDER] Downloaded {url}, length={content.Length}");
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OFFSET FINDER] Error downloading {url}: {ex.Message}");
                return string.Empty;
            }
        }


        private static void ParseOffsetsFile(string content)
        {
            // regex to match the offsets (global constants)
            var matches = Regex.Matches(content, @"public (?:const|static) nint (\w+) = (0x[0-9A-Fa-f]+);");
            Console.WriteLine($"[OFFSET FINDER] Found {matches.Count} Offsets In File offsets.cs");

            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }

        private static void ParseClientDllFile(string content)
        {
            ParseClassOffsets(content, "C_BaseEntity");

            // regex fallback for any extra global consts
            var matches = Regex.Matches(content, @"public (?:const|static) nint (\w+) = (0x[0-9A-Fa-f]+);");
            Console.WriteLine($"[OFFSET FINDER] Found {matches.Count} additional offsets in client_dll.cs");

            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                if (!offsets.ContainsKey(name))
                {
                    offsets[name] = value;
                }
            }
        }

        private static void ParseClassOffsets(string content, string className) // explicitly parse a given class
        {
            string classPattern = $@"public\s+static\s+class\s+{className}\s*\{{([\s\S]*?)\}}";
            var classMatch = Regex.Match(content, classPattern, RegexOptions.Multiline);

            if (!classMatch.Success)
            {
                Console.WriteLine($"[OFFSET FINDER] Could Not Find Class {className}");
                return;
            }

            var matches = Regex.Matches(classMatch.Groups[1].Value, @"public const nint (\w+) = (0x[0-9A-Fa-f]+);");

            Console.WriteLine($"[OFFSET FINDER] Found {matches.Count} offsets in class {className}");

            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }

        private static void ParseButtonsFile(string content)
        {
            // regex to match the offsets
            var matches = Regex.Matches(content, @"public const nint ([\w\+]+) = (0x[0-9A-Fa-f]+);");
            Console.WriteLine($"[OFFSET FINDER] Found {matches.Count} Offsets In File buttons.cs");

            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }

        private static void ParseEngine2File(string content)
        {
            // regex to match the offsets
            var matches = Regex.Matches(content, @"public const nint ([\w\+]+) = (0x[0-9A-Fa-f]+);");
            Console.WriteLine($"[OFFSET FINDER] Found {matches.Count} Offsets In File engine2.cs");

            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }

        private static void UpdateOffsetsClass()
        {
            // grab your static Offsets class
            Type offsetsType = typeof(Titled_Gui.Data.Game.Offsets);
            FieldInfo[] fields = offsetsType.GetFields(BindingFlags.Public | BindingFlags.Static);

            int updatedCount = 0;
            foreach (FieldInfo field in fields)
            {
                string fieldName = field.Name;

                if (FieldNameMappings.TryGetValue(fieldName, out List<Offset>? sources))
                {
                    bool found = false;
                    foreach (var source in sources)
                    {
                        if (source.ClassName != null)
                        {
                            ParseClassOffsets(ClientDllContent, source.ClassName);
                        }

                        if (offsets.TryGetValue(source.Name, out int value))
                        {
                            int? currentValue = (int?)field?.GetValue(null);
                            field?.SetValue(null, value);

                            Console.WriteLine($"[OFFSET FINDER] Updated {fieldName} From 0x{currentValue:X} To 0x{value:X} (Source: {source.Name}, Class: {source.ClassName ?? "global"})");
                            updatedCount++;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine($"[OFFSET FINDER] ERROR: No Offset Found {fieldName} (Tried: {string.Join(", ", sources.Select(s => s.Name))})");
                    }
                }
                else
                {
                    Console.WriteLine($"[OFFSET FINDER] ERROR: {fieldName} Is Not Mapped");
                }
            }

            Console.WriteLine($"[OFFSET FINDER] Updated: {updatedCount}/{fields.Length} Offsets");
        }
    }
}
