using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Titled_Gui.Classes
{
    internal class OffsetGetter
    {
        private static readonly Dictionary<string, int> offsets = new Dictionary<string, int>();
        private static readonly HttpClient httpClient = new();

        private const string OffsetsUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.cs";
        private const string ClientDllUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.cs";
        private const string ButtonsUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/buttons.cs";
        private const string Engine2Url = "https://github.com/a2x/cs2-dumper/blob/b7f46588ef258949eb38014268cec1698bfe2071/output/engine2_dll.cs#L39";

        private static readonly Dictionary<string, List<string>> FieldNameMappings = new Dictionary<string, List<string>>
        {
            {"dwViewMatrix", new List<string> {"dwViewMatrix"}},
            {"dwEntityList", new List<string> {"dwEntityList"}},
            {"dwLocalPlayerPawn", new List<string> {"dwLocalPlayerPawn"}},
            {"dwViewAngles", new List<string> {"dwViewAngles"}},
            {"dwGlobalVars", new List<string> {"dwGlobalVars"}},
            {"dwPlantedC4", new List<string> {"dwPlantedC4"}},
            {"dwGameRules", new List<string> {"dwGameRules"}},
            {"dwSensitivity", new List<string> {"dwSensitivity"}},
            {"dwSensitivity_sensitivity", new List<string> {"dwSensitivity_sensitivity"}},
            {"dwCSGOInput", new List<string> {"dwCSGOInput"}},
            {"attack", new List<string> {"attack", "+attack"}},
            {"jump", new List<string> {"jump", "+jump"}},

            {"m_pCameraServices", new List<string> {"m_pCameraServices", "m_CameraServices"}},
            {"m_iFOV", new List<string> {"m_iFOV", "m_iDesiredFOV"}},
            {"m_bIsScoped", new List<string> {"m_bIsScoped", "m_bIsScopedIn"}},
            {"m_iHealth", new List<string> {"m_iHealth"}},
            {"m_entitySpottedState", new List<string> {"m_entitySpottedState"}},
            {"m_bSpotted", new List<string> {"m_bSpotted", "m_bSpottedBy"}},
            {"m_iIDEntIndex", new List<string> {"m_iIDEntIndex"}},
            {"m_pSceneNode", new List<string> {"m_pSceneNode", "m_pRenderingNode"}},
            {"m_vecViewOffset", new List<string> {"m_vecViewOffset", "m_vViewOffset"}},
            {"m_lifeState", new List<string> {"m_lifeState"}},
            {"m_vOldOrigin", new List<string> {"m_vOldOrigin"}},
            {"m_iTeamNum", new List<string> {"m_iTeamNum"}},
            {"m_hPlayerPawn", new List<string> {"m_hPlayerPawn"}},
            {"m_flFlashBangTime", new List<string> {"m_flFlashBangTime", "m_flFlashDuration"}},
            {"m_modelState", new List<string> {"m_modelState"}},
            {"m_pGameSceneNode", new List<string> {"m_pGameSceneNode"}},
            {"m_flC4Blow", new List<string> {"m_flC4Blow", "m_flDetonateTime"}},
            {"current_time", new List<string> {"current_time", "m_flCurrentTime"}},
            {"m_bBombPlanted", new List<string> {"m_bBombPlanted", "m_bBombTicking"}},
            {"m_iszPlayerName", new List<string> {"m_iszPlayerName"}},
            {"m_pClippingWeapon", new List<string> {"m_pClippingWeapon"}},
            {"m_Item", new List<string> {"m_Item", "m_hActiveWeapon"}},
            {"m_iItemDefinitionIndex", new List<string> {"m_iItemDefinitionIndex"}},
            {"m_AttributeManager", new List<string> {"m_AttributeManager"}},
            {"m_bSpottedByMask", new List<string> { "m_bSpottedByMask" }},
            {"m_pWeaponServices", new List<string> { "m_pWeaponServices" }},
            {"m_hActiveWeapon", new List<string> { "m_hActiveWeapon" }},
            {"m_vecAbsVelocity", new List<string> { "m_vecAbsVelocity" }},
            {"m_fFlags", new List<string> { "m_fFlags" }},
            {"m_hMyWeapons", new List<string> { "m_hMyWeapons" }},
            {"m_AimPunchAngle", new List<string> { "m_AimPunchAngle" }},
            {"m_nCurrentTickThisFrame", new List<string> { "m_nCurrentTickThisFrame" }},
            {"m_ArmorValue", new List<string> { "m_ArmorValue" }},
            {"m_pInGameMoneyServices", new List<string> { "m_pInGameMoneyServices" }},
            {"m_iAccount", new List<string> { "m_iAccount" }},
            {"m_iTotalCashSpent", new List<string> { "m_iTotalCashSpent" }},
            {"m_iCashSpentThisRound", new List<string> { "m_iCashSpentThisRound" }},
            {"m_aimPunchCache", new List<string> { "m_aimPunchCache" }},
        };

        public static async Task UpdateOffsetsAsync()
        {
            try
            {
                Console.WriteLine("[OFFSET FINDER] Starting The Offset Finding Process Please Wait...");
                offsets.Clear();

                await DownloadAndParseFile(OffsetsUrl, ParseOffsetsFile);
                await DownloadAndParseFile(ClientDllUrl, ParseClientDllFile);
                await DownloadAndParseFile(ButtonsUrl, ParseButtonsFile);
                await DownloadAndParseFile(Engine2Url, ParseEngine2File);

                Console.WriteLine($"[OFFSET FINDER] Found: {offsets.Count}");
                UpdateOffsetsClass();
                Console.WriteLine("[OFFSET FINDER] Offsets Updated Succesfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OFFSETS FINDER] ERROR: {ex.Message}");
            }
        }

        private static async Task DownloadAndParseFile(string url, Action<string> parseMethod)
        {
            try
            {
                Console.WriteLine($"[OFFSET FINDER] Downloading Offsets From {url}");
                var response = await httpClient.GetStringAsync(url);
                parseMethod(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OFFSET FINDER] Error {url}: {ex.Message}");
            }
        }

        private static void ParseOffsetsFile(string content)
        {
            // regex to match the offsets
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

            // regex fallback
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
        private static void ParseClassOffsets(string content, string className) // if it nees a explicit class
        {
            string classPattern = $@"public static class {className} \\{{([\\s\\S]*?)\\}}";
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
            Console.WriteLine($"[OFFSET FINDER] Found {matches.Count}  Offsets In File buttons.cs");

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
            Console.WriteLine($"[OFFSET FINDER] Found {matches.Count}  Offsets In File engine2.cs");

            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }
        private static void UpdateOffsetsClass()
        {
            Type offsetsType = typeof(Titled_Gui.Data.Offsets);
            FieldInfo[] fields = offsetsType.GetFields(BindingFlags.Public | BindingFlags.Static);

            int updatedCount = 0;
            foreach (FieldInfo field in fields)
            {
                string FieldName = field.Name;

                if (FieldNameMappings.TryGetValue(FieldName, out List<string>? possibleNames))
                {
                    bool found = false;
                    foreach (string dumperFieldName in possibleNames)
                    {
                        if (offsets.TryGetValue(dumperFieldName, out int value))
                        {
                            if (field != null)
                            {
                                int? currentValue = (int?)field?.GetValue(null);

                                field?.SetValue(null, value);
                                Console.WriteLine($"[OFFSET FINDER] Updated {FieldName} from 0x{currentValue:X} to 0x{value:X} (source: {dumperFieldName})");
                                updatedCount++;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine($"[OFFSET FINDER] ERROR: No Offset Found {FieldName} (tried: {string.Join(", ", possibleNames)})");
                    }
                }
                else
                {
                    Console.WriteLine($"[OFFSET FINDER] ERROR: {FieldName} Is Not Mapped");
                }
            }

            Console.WriteLine($"[OFFSET FINDER] Updated: {updatedCount}/{fields.Length} Offsets");
        }
    }
}