using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Modules.Visual
{
    public struct PlayerModel
    {
        public string ModelName;
        public string ModelPath;
        public string LocalFilePath;
        public IntPtr TexturePtr;

        public PlayerModel(string name, string path, string localPath)
        {
            ModelName = name;
            ModelPath = path;
            LocalFilePath = localPath;
            TexturePtr = IntPtr.Zero;
        }
    }

    public static class ModelChanger
    {
        #region Fields

        public static bool Enabled = false;
        public static int SelectedModelIndex = -1;
        public static List<PlayerModel> AvailableModels = new();

        private static uint lastModelHash = 0;
        private static DateTime lastScanTime = DateTime.MinValue;
        private static DateTime lastApplyTime = DateTime.MinValue;
        private static readonly TimeSpan scanInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan applyInterval = TimeSpan.FromMilliseconds(500);

        // Local directory next to exe
        private static string LocalModelsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "characters");

        public static bool ShowPreview = true;
        private static Vector2 previewSize = new(250, 350);

        // Status
        private static string statusMessage = "Ready";
        private static Vector4 statusColor = new(1, 1, 1, 1);

        #endregion

        #region Model Discovery

        public static void ScanForModels()
        {
            try
            {
                AvailableModels.Clear();
                SetStatus("Scanning for models...", new Vector4(1, 1, 0, 1));

                // Create directory if it doesn't exist
                if (!Directory.Exists(LocalModelsDirectory))
                {
                    Directory.CreateDirectory(LocalModelsDirectory);
                    SetStatus("Created /characters/ folder. Add .vmdl files here!", new Vector4(1, 1, 0, 1));
                    CreateReadmeFile();
                    return;
                }

                // Scan local directory
                ScanLocalDirectory(LocalModelsDirectory);

                if (AvailableModels.Count > 0)
                {
                    SetStatus($"Found {AvailableModels.Count} player models", new Vector4(0, 1, 0, 1));
                }
                else
                {
                    SetStatus("No models found. Add .vmdl files to /characters/", new Vector4(1, 1, 0, 1));
                }

                lastScanTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                SetStatus($"Scan error: {ex.Message}", new Vector4(1, 0, 0, 1));
                Console.WriteLine($"ModelChanger scan error: {ex.Message}");
            }
        }

        private static void ScanLocalDirectory(string directory)
        {
            try
            {
                // Scan for .vmdl files (without _c)
                var vmdlFiles = Directory.GetFiles(directory, "*.vmdl", SearchOption.AllDirectories);

                foreach (var file in vmdlFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Skip arm models
                    if (fileName.Contains("arm", StringComparison.OrdinalIgnoreCase) ||
                        fileName.Contains("sleeve", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Create relative path from characters folder
                    string relativePath = file.Replace(LocalModelsDirectory, "")
                        .Replace('\\', '/')
                        .TrimStart('/');

                    // Build game path (characters/models/...)
                    string gamePath = "characters/models/" + relativePath;

                    AvailableModels.Add(new PlayerModel(fileName, gamePath, file));
                }

                // Also scan for .vmdl_c files
                var vmdlcFiles = Directory.GetFiles(directory, "*.vmdl_c", SearchOption.AllDirectories);

                foreach (var file in vmdlcFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Skip if already added as .vmdl
                    if (AvailableModels.Any(m => m.LocalFilePath.Replace(".vmdl_c", ".vmdl") == file.Replace(".vmdl_c", ".vmdl")))
                        continue;

                    if (fileName.Contains("arm", StringComparison.OrdinalIgnoreCase) ||
                        fileName.Contains("sleeve", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Remove _c suffix from filename
                    if (fileName.EndsWith("_c"))
                        fileName = fileName.Substring(0, fileName.Length - 2);

                    string relativePath = file.Replace(LocalModelsDirectory, "")
                        .Replace('\\', '/')
                        .TrimStart('/');

                    // Remove _c from path
                    if (relativePath.EndsWith(".vmdl_c"))
                        relativePath = relativePath.Substring(0, relativePath.Length - 2);

                    string gamePath = "characters/models/" + relativePath;

                    AvailableModels.Add(new PlayerModel(fileName, gamePath, file));
                }

                // Sort by name
                AvailableModels = AvailableModels.OrderBy(m => m.ModelName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning directory {directory}: {ex.Message}");
            }
        }

        private static void CreateReadmeFile()
        {
            try
            {
                string readmePath = Path.Combine(LocalModelsDirectory, "README.txt");
                string readmeContent = @"=== CS2 Player Model Changer ===

HOW TO USE:
1. Place CS2 player model files (.vmdl or .vmdl_c) in this folder
2. You can organize them in subfolders
3. Click 'Scan Models' button in the menu
4. Select a model and enable the changer

WHERE TO GET MODELS:
- Extract from CS2 game files (game/csgo/characters/models/)
- Download from community sites
- Create custom models

SUPPORTED FORMATS:
- .vmdl (preferred)
- .vmdl_c (compiled models)

FOLDER STRUCTURE EXAMPLE:
/characters/
  /ct/
    - ctm_fbi.vmdl
    - ctm_swat.vmdl
  /t/
    - tm_phoenix.vmdl
    - tm_leet.vmdl
  /custom/
    - my_custom_model.vmdl

NOTES:
- Models must be compatible with CS2
- Arm/sleeve models are automatically skipped
- Use at your own risk in online games
";
                File.WriteAllText(readmePath, readmeContent);
            }
            catch { }
        }

        public static void OpenModelsFolder()
        {
            try
            {
                if (!Directory.Exists(LocalModelsDirectory))
                    Directory.CreateDirectory(LocalModelsDirectory);

                System.Diagnostics.Process.Start("explorer.exe", LocalModelsDirectory);
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to open folder: {ex.Message}", new Vector4(1, 0, 0, 1));
            }
        }

        #endregion

        #region Model Application

        public static void ApplyModel()
        {
            try
            {
                // Throttle apply attempts
                if ((DateTime.Now - lastApplyTime).TotalMilliseconds < applyInterval.TotalMilliseconds)
                    return;

                lastApplyTime = DateTime.Now;

                if (!Enabled || SelectedModelIndex < 0 || SelectedModelIndex >= AvailableModels.Count)
                    return;

                // Check if process is valid
                if (GameState.swed == null || GameState.LocalPlayer == null)
                    return;

                Entity localPlayer = GameState.LocalPlayer;

                // Check if player is alive
                if (localPlayer.Health <= 0 || localPlayer.PawnAddress == IntPtr.Zero)
                    return;

                var selectedModel = AvailableModels[SelectedModelIndex];

                // Calculate model hash
                uint modelHash = CalculateModelHash(selectedModel.ModelPath);

                // Check if model is already applied
                if (lastModelHash == modelHash)
                    return;

                // Apply model
                if (SetPlayerModel(localPlayer.PawnAddress, selectedModel.ModelPath))
                {
                    lastModelHash = modelHash;
                    SetStatus($"Applied: {selectedModel.ModelName}", new Vector4(0, 1, 0, 1));
                }
                else
                {
                    SetStatus("Failed to apply model", new Vector4(1, 0, 0, 1));
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Apply error: {ex.Message}", new Vector4(1, 0, 0, 1));
                Console.WriteLine($"ModelChanger apply error: {ex.Message}");
            }
        }

        private static bool SetPlayerModel(IntPtr pawnAddress, string modelPath)
        {
            try
            {
                // TODO: Implement actual model change
                // This requires:
                // 1. Finding SetModel function address
                // 2. Calling it with modelPath parameter
                // 3. Or writing model path to entity structure

                Console.WriteLine($"Attempting to set model: {modelPath}");
                return true; // Placeholder
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetPlayerModel error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private static uint CalculateModelHash(string modelPath)
        {
            // FNV-1a hash
            uint hash = 2166136261;
            foreach (char c in modelPath)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return hash;
        }

        private static void SetStatus(string message, Vector4 color)
        {
            statusMessage = message;
            statusColor = color;
            Console.WriteLine($"[ModelChanger] {message}");
        }

        public static void Reset()
        {
            lastModelHash = 0;
            Enabled = false;
            SetStatus("Model changer disabled", new Vector4(1, 1, 1, 1));
        }

        #endregion

        #region Menu Rendering

        public static void RenderMenu()
        {
            ImGui.PushID("ModelChanger##Menu");

            // Status bar
            var statusPos = ImGui.GetCursorScreenPos();
            var drawList = ImGui.GetWindowDrawList();
            var statusSize = new Vector2(ImGui.GetContentRegionAvail().X, 30);

            // Status background
            drawList.AddRectFilled(
                statusPos,
                new Vector2(statusPos.X + statusSize.X, statusPos.Y + statusSize.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.12f, 1f)),
                5f
            );

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);
            ImGui.Indent(10);
            ImGui.PushStyleColor(ImGuiCol.Text, statusColor);
            ImGui.TextWrapped(statusMessage);
            ImGui.PopStyleColor();
            ImGui.Unindent(10);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Enable toggle
            bool enabled = Enabled;
            if (ImGui.Checkbox("Enable Player Model Changer", ref enabled))
            {
                Enabled = enabled;
                if (!enabled)
                    Reset();
                else
                    SetStatus("Model changer enabled", new Vector4(0, 1, 0, 1));
            }

            ImGui.Spacing();

            // Control buttons row 1
            if (ImGui.Button("📁 Open Folder", new Vector2((ImGui.GetContentRegionAvail().X - 10) / 2, 30)))
            {
                OpenModelsFolder();
            }

            ImGui.SameLine();

            if (ImGui.Button("🔄 Scan Models", new Vector2((ImGui.GetContentRegionAvail().X), 30)))
            {
                ScanForModels();
            }

            ImGui.Spacing();

            // Control buttons row 2
            ImGui.BeginDisabled(SelectedModelIndex < 0);
            if (ImGui.Button("✓ Apply Now", new Vector2((ImGui.GetContentRegionAvail().X - 10) / 2, 30)))
            {
                lastModelHash = 0; // Force reapply
                ApplyModel();
            }
            ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button("↺ Reset", new Vector2((ImGui.GetContentRegionAvail().X), 30)))
            {
                Reset();
            }

            ImGui.Spacing();

            // Info box
            var infoPos = ImGui.GetCursorScreenPos();
            var infoSize = new Vector2(ImGui.GetContentRegionAvail().X, 50);

            drawList.AddRectFilled(
                infoPos,
                new Vector2(infoPos.X + infoSize.X, infoPos.Y + infoSize.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.12f, 0.16f, 1f)),
                5f
            );

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
            ImGui.Indent(10);
            ImGui.Text($"📦 Available models: {AvailableModels.Count}");
            if (SelectedModelIndex >= 0 && SelectedModelIndex < AvailableModels.Count)
            {
                ImGui.TextColored(new Vector4(0.5f, 0.8f, 1f, 1f), $"✓ Selected: {AvailableModels[SelectedModelIndex].ModelName}");
            }
            else
            {
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), "No model selected");
            }
            ImGui.Unindent(10);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Model list with search
            RenderModelList();

            ImGui.Spacing();

            // Preview toggle
            ImGui.Checkbox("Show Preview Window", ref ShowPreview);

            // Help text
            if (AvailableModels.Count == 0)
            {
                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.8f, 0.3f, 1f));
                ImGui.TextWrapped("💡 Tip: Click 'Open Folder' to add your model files (.vmdl)");
                ImGui.PopStyleColor();
            }

            ImGui.PopID();
        }

        private static string searchFilter = "";

        private static void RenderModelList()
        {
            ImGui.Text("Model List:");

            // Search bar
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputTextWithHint("##search", "🔍 Search models...", ref searchFilter, 256))
            {
                // Search updated
            }

            ImGui.Spacing();

            if (AvailableModels.Count > 0)
            {
                // Filter models based on search
                var filteredModels = string.IsNullOrEmpty(searchFilter)
                    ? AvailableModels
                    : AvailableModels.Where(m => m.ModelName.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)).ToList();

                // Model list
                ImGui.BeginChild("ModelListChild", new Vector2(0, 200), ImGuiChildFlags.Border);
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 4));

                    for (int i = 0; i < filteredModels.Count; i++)
                    {
                        var model = filteredModels[i];
                        int originalIndex = AvailableModels.IndexOf(model);
                        bool selected = SelectedModelIndex == originalIndex;

                        // Colored background for selected
                        if (selected)
                        {
                            var itemPos = ImGui.GetCursorScreenPos();
                            var itemSize = new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeightWithSpacing());
                            ImGui.GetWindowDrawList().AddRectFilled(
                                itemPos,
                                new Vector2(itemPos.X + itemSize.X, itemPos.Y + itemSize.Y),
                                ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.5f, 0.8f, 0.3f)),
                                3f
                            );
                        }

                        // Selectable with icon
                        string icon = selected ? "✓" : "👤";
                        if (ImGui.Selectable($"{icon} {model.ModelName}##model_{originalIndex}", selected))
                        {
                            SelectedModelIndex = originalIndex;
                            lastModelHash = 0; // Force reapply
                            SetStatus($"Selected: {model.ModelName}", new Vector4(1, 1, 0, 1));
                        }

                        // Context menu
                        if (ImGui.BeginPopupContextItem($"ctx_{originalIndex}"))
                        {
                            if (ImGui.MenuItem("Apply this model"))
                            {
                                SelectedModelIndex = originalIndex;
                                lastModelHash = 0;
                                ApplyModel();
                            }

                            ImGui.Separator();

                            if (ImGui.MenuItem("Show in folder"))
                            {
                                try
                                {
                                    string args = $"/select,\"{model.LocalFilePath}\"";
                                    System.Diagnostics.Process.Start("explorer.exe", args);
                                }
                                catch { }
                            }

                            ImGui.EndPopup();
                        }

                        // Tooltip with details
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text($"📝 Name: {model.ModelName}");
                            ImGui.Separator();
                            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), "Game path:");
                            ImGui.TextWrapped(model.ModelPath);
                            ImGui.Separator();
                            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), "Local file:");
                            ImGui.TextWrapped(model.LocalFilePath);
                            ImGui.Separator();
                            ImGui.TextColored(new Vector4(0.5f, 1f, 0.5f, 1f), "💡 Double-click to apply");
                            ImGui.TextColored(new Vector4(0.5f, 1f, 0.5f, 1f), "Right-click for options");
                            ImGui.EndTooltip();
                        }

                        // Double click to apply
                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            SelectedModelIndex = originalIndex;
                            lastModelHash = 0;
                            ApplyModel();
                        }
                    }

                    ImGui.PopStyleVar();

                    if (filteredModels.Count == 0)
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 80);
                        var text = "No models match search";
                        var textSize = ImGui.CalcTextSize(text);
                        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - textSize.X) / 2);
                        ImGui.TextColored(new Vector4(1, 1, 0, 1), text);
                    }
                }
                ImGui.EndChild();

                ImGui.Text($"📊 Showing {filteredModels.Count} / {AvailableModels.Count} models");
            }
            else
            {
                ImGui.BeginChild("EmptyListChild", new Vector2(0, 200), ImGuiChildFlags.Border);
                {
                    var size = ImGui.GetContentRegionAvail();
                    ImGui.SetCursorPosY(size.Y / 2 - 40);

                    var text = "📁 No models found";
                    var textSize = ImGui.CalcTextSize(text);
                    ImGui.SetCursorPosX((size.X - textSize.X) / 2);
                    ImGui.TextColored(new Vector4(1, 1, 0, 1), text);

                    ImGui.Spacing();

                    text = "Click 'Open Folder' and add .vmdl files";
                    textSize = ImGui.CalcTextSize(text);
                    ImGui.SetCursorPosX((size.X - textSize.X) / 2);
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), text);

                    ImGui.Spacing();

                    text = "Then click 'Scan Models'";
                    textSize = ImGui.CalcTextSize(text);
                    ImGui.SetCursorPosX((size.X - textSize.X) / 2);
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), text);
                }
                ImGui.EndChild();
            }
        }

        public static void RenderPreview()
        {
            if (!ShowPreview || SelectedModelIndex < 0 || SelectedModelIndex >= AvailableModels.Count)
                return;

            var selected = AvailableModels[SelectedModelIndex];

            ImGui.SetNextWindowSize(previewSize, ImGuiCond.FirstUseEver);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));

            if (ImGui.Begin("🎭 Model Preview", ref ShowPreview, ImGuiWindowFlags.NoCollapse))
            {
                var windowSize = ImGui.GetContentRegionAvail();
                var drawList = ImGui.GetWindowDrawList();
                var pos = ImGui.GetCursorScreenPos();

                // Background gradient
                var topColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.15f, 0.15f, 0.20f, 1f));
                var bottomColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.12f, 1f));

                drawList.AddRectFilledMultiColor(
                    pos,
                    new Vector2(pos.X + windowSize.X, pos.Y + windowSize.Y),
                    topColor, topColor, bottomColor, bottomColor
                );

                // Animated border
                float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;
                float pulseAlpha = 0.5f + 0.3f * MathF.Sin(time * 2f);
                drawList.AddRect(
                    pos,
                    new Vector2(pos.X + windowSize.X, pos.Y + windowSize.Y),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.5f, 0.8f, pulseAlpha)),
                    5f, ImDrawFlags.None, 2f
                );

                // Model icon/placeholder with pulse effect
                var iconSize = 100f;
                var iconPos = new Vector2(
                    pos.X + (windowSize.X - iconSize) / 2,
                    pos.Y + 30
                );

                float pulseScale = 1f + 0.05f * MathF.Sin(time * 3f);
                float currentIconSize = iconSize * pulseScale;

                drawList.AddCircleFilled(
                    new Vector2(iconPos.X + iconSize / 2, iconPos.Y + iconSize / 2),
                    currentIconSize / 2,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.5f, 0.8f, 0.3f)),
                    32
                );

                // Icon text
                var iconText = "👤";
                var iconTextSize = ImGui.CalcTextSize(iconText);
                drawList.AddText(
                    ImGui.GetFont(),
                    50f,
                    new Vector2(
                        iconPos.X + (iconSize - iconTextSize.X / 2) / 2,
                        iconPos.Y + (iconSize - iconTextSize.Y / 2) / 2
                    ),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.9f)),
                    iconText
                );

                // Model name with shadow
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 150);
                var nameSize = ImGui.CalcTextSize(selected.ModelName);
                var namePos = new Vector2((windowSize.X - nameSize.X) / 2, ImGui.GetCursorPosY());

                // Shadow
                drawList.AddText(
                    new Vector2(pos.X + namePos.X + 2, pos.Y + namePos.Y + 2),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.5f)),
                    selected.ModelName
                );
                // Main text
                ImGui.SetCursorPosX(namePos.X);
                ImGui.TextColored(new Vector4(1, 1, 1, 1), selected.ModelName);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                // Info section
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "Game Path:");
                ImGui.TextWrapped(selected.ModelPath);

                ImGui.Spacing();

                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "Local File:");
                ImGui.TextWrapped(Path.GetFileName(selected.LocalFilePath));

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                // Buttons
                if (ImGui.Button("✓ Apply This Model", new Vector2(-1, 35)))
                {
                    lastModelHash = 0;
                    ApplyModel();
                }

                ImGui.Spacing();

                if (ImGui.Button("📁 Show in Explorer", new Vector2(-1, 30)))
                {
                    try
                    {
                        string args = $"/select,\"{selected.LocalFilePath}\"";
                        System.Diagnostics.Process.Start("explorer.exe", args);
                    }
                    catch { }
                }

                ImGui.End();
            }

            ImGui.PopStyleVar();
        }

        #endregion

        #region Update Loop

        public static void Update()
        {
            try
            {
                // Auto-scan on first update
                if (AvailableModels.Count == 0 && (DateTime.Now - lastScanTime) > TimeSpan.FromSeconds(2))
                {
                    ScanForModels();
                }

                // Apply model if enabled
                if (Enabled && SelectedModelIndex >= 0)
                {
                    ApplyModel();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ModelChanger Update error: {ex.Message}");
            }
        }

        #endregion
    }
}