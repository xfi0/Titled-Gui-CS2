using ImGuiNET;
using System.Numerics;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Menu.Types;
using Titled_Gui.ImGUI.Widgets;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using static Titled_Gui.ImGUI.Widgets.ColorPickers;
using static Titled_Gui.ImGUI.Widgets.Combos;
using static Titled_Gui.ImGUI.Widgets.Sliders;
using static Titled_Gui.ImGUI.Widgets.Toggles;

internal class Sections
{
    public static float ChildRounding = 6f;
    public class SectionT(string label, int tab, Action content)
    {
        public string label = label;
        public Action content = content;
        public int tab = tab;
    }
    public static List<SectionT> sections = InitializeSections();

    public static List<SectionT> InitializeSections()
    {
        List<SectionT> sections = new()
        {
            new("Misc", 0, () =>
            {
                RenderBoolSettingWithWarning("Auto Bunny Hop", () => Bhop.BhopEnable, v => Bhop.BhopEnable = v);
                RenderBoolSetting("Hit Sound", () => HitStuff.EnableHitSounds, v => HitStuff.EnableHitSounds = v);
                RenderFloatSlider("Hit Sound Volume", ref HitStuff.Volume, 0, 1);
                RenderIntCombo("Current Hit Sound", ref HitStuff.CurrentHitSound, HitStuff.HitSoundDisplays, HitStuff.HitSounds.Count, true);
                RenderBoolSettingWith1ColorPicker("Headshot Text", () => HitStuff.EnableHeadshotText, v => HitStuff.EnableHeadshotText = v, ref HitStuff.TextColor);
            }),
            new("Gernade Helper", 0, () =>
            {
                RenderBoolSetting("Enable Gernade Helper", () => GernadeLineup.Enabled, v => GernadeLineup.Enabled = v);
                RenderBoolSetting("Always Show", () => GernadeLineup.AlwaysShow, v => GernadeLineup.AlwaysShow = v);
                //Render2ColorPickers("Position Circle Color", ref GernadeLineup.PositionColorInside, ref GernadeLineup.PositionColorOutside);
                ImGui.InputText("Lineup Name", ref GernadeLineup.LineupName, 128);
                RenderIntCombo("Linup Type", ref GernadeLineup.SelectedType, GernadeLineup.TypesList, GernadeLineup.TypesList.Count, false);
                Titled_Gui.ImGUI.Widgets.Button.RenderButton("Save Lineup", () => {
                    GernadeLineup.SaveLineup(GernadeLineup.LineupName, (GrenadeLaunchType)GernadeLineup.SelectedType);
                });
            }),

            new("Aimbot", 1, () =>
            {
                RenderBoolSetting("Enable Aimbot", () => Aimbot.AimbotEnable, v => Aimbot.AimbotEnable = v);
                RenderIntCombo("Aim Bone", ref Aimbot.CurrentBone, Aimbot.Bones.ToList(), Aimbot.Bones.Length);
                Keybind.RenderKeybindChooser("Aimbot Keybind", ref Aimbot.AimbotKey, () => Aimbot.AimbotEnable);
                RenderBoolSetting("Aim On Team", () => Aimbot.Team, v => Aimbot.Team = v);
                RenderFloatSlider("Smoothing X", ref Aimbot.SmoothingX, 0, 20, "%.2f");
                RenderFloatSlider("Smoothing Y", ref Aimbot.SmoothingY, 0, 20, "%.2f");
                RenderBoolSetting("Draw FOV", () => Aimbot.DrawFov, v => Aimbot.DrawFov = v);
                RenderBoolSetting("Use FOV", () => Aimbot.UseFOV, v => Aimbot.UseFOV = v);
                RenderBoolSetting("Scoped Check", () => Aimbot.ScopedOnly, v => Aimbot.ScopedOnly = v);
                RenderIntSlider("FOV Size", ref Aimbot.FovSize, 10, 1000, "%d");
                RenderColorPicker("FOV Color", ref Aimbot.FovColor, ref Aimbot.RGB);
                RenderBoolSetting("Visibility Check", () => Aimbot.VisibilityCheck, v => Aimbot.VisibilityCheck = v);
                RenderBoolSetting("Target Line", () => Aimbot.TargetLine, v => Aimbot.TargetLine = v);
            }),
            new("RCS", 1, () =>
            {
                RenderBoolSetting("RCS", () => RCS.Enabled, v => RCS.Enabled = v);
                RenderFloatSlider("Strength", ref RCS.Strength, 0f, 1f, "%.2f");
            }),
            new("Triggerbot", 1, () =>
            {
                RenderBoolSetting("Triggerbot", () => TriggerBot.Enabled, v => TriggerBot.Enabled = v);
                RenderBoolSetting("Team Check", () => TriggerBot.TeamCheck, v => TriggerBot.TeamCheck = v);
                Keybind.RenderKeybindChooser("Trigger Bot Keybind", ref TriggerBot.TriggerKey, () => TriggerBot.Enabled);
                RenderIntSlider("Max Delay", ref TriggerBot.MaxDelay, 0, 1000, "%d");
                RenderIntSlider("Min Delay", ref TriggerBot.MinDelay, 0, 1000, "%d");
            }),

            new("Box ESP", 2, () =>
            {
                RenderBoolSetting("Enable ESP", () => BoxESP.EnableESP, v => BoxESP.EnableESP = v);
                RenderIntCombo("ESP Shape", ref BoxESP.CurrentShape, BoxESP.Shapes.ToList(), BoxESP.Shapes.Length, false);
                RenderBoolSetting("Team Check", () => BoxESP.TeamCheck, v => BoxESP.TeamCheck = v);
                RenderBoolSettingWith2ColorPickers("Box Fill Gradient", () => BoxESP.BoxFillGradient, v => BoxESP.BoxFillGradient = v, ref BoxESP.GradientColors.TeamRGB, ref BoxESP.GradientColors.EnemyRGB, ref BoxESP.GradientColors.TeamColor, ref BoxESP.GradientColors.EnemyColor); // TODO impl rgb
                RenderBoolSettingWith2ColorPickers("Filled Boxes", () => BoxESP.FillBox, v => BoxESP.FillBox = v, ref BoxESP.FillColors.TeamRGB, ref BoxESP.FillColors.EnemyRGB, ref BoxESP.FillColors.TeamColor, ref BoxESP.FillColors.EnemyColor);
                RenderBoolSettingWith1ColorPicker("Inner Outline", () => BoxESP.InnerOutline, v => BoxESP.InnerOutline = v, ref BoxESP.InnerOutlineColors.TeamRGB, ref BoxESP.InnerOutlineColors.TeamColor);
                RenderFloatSlider("ESP Rounding", ref BoxESP.Rounding, 0, 5f);
                RenderFloatSlider("ESP Glow", ref BoxESP.GlowAmount, 0f, 5f);
                RenderBoolSettingWith2ColorPickers("Outer Outline", () => BoxESP.OuterOutline, v => BoxESP.OuterOutline = v, ref BoxESP.OutlineColors.TeamRGB, ref BoxESP.OutlineColors.EnemyRGB, ref BoxESP.OutlineColors.TeamColor, ref BoxESP.OutlineColors.EnemyColor);
                RenderBoolSettingWith2ColorPickers("Visibility Check", () => BoxESP.VisibilityCheck, v => BoxESP.VisibilityCheck = v, ref BoxESP.OccludedColors.TeamRGB, ref BoxESP.OccludedColors.EnemyRGB, ref BoxESP.OccludedColors.TeamColor, ref BoxESP.OccludedColors.EnemyColor);
                RenderBoolSetting("Flash Check", () => BoxESP.FlashCheck, v => BoxESP.FlashCheck = v);
            }),
            new("Player ESP", 2, () =>
            {
                RenderBoolSetting("Enable Health Bar", () => HealthBar.EnableHealthBar, v => HealthBar.EnableHealthBar = v);
                RenderBoolSettingWith2ColorPickers("Enable Armor Bar", () => ArmorBar.EnableArmorBar, v => ArmorBar.EnableArmorBar = v, ref ArmorBar.ArmorColor.TeamRGB, ref ArmorBar.ArmorColor.EnemyRGB, ref ArmorBar.ArmorColor.TeamColor, ref ArmorBar.ArmorColor.EnemyColor);
                RenderBoolSetting("Eye Ray", () => EyeRay.Enabled, v => EyeRay.Enabled = v);
            }),
            new("Flags", 2, () =>
            {
                Render2ColorPickers("Text Color", ref Flags.TextColors.TeamRGB, ref Flags.TextColors.EnemyRGB, ref Flags.TextColors.TeamColor, ref Flags.TextColors.EnemyColor);
                RenderBoolSetting("Scoped", () => Flags.ScopedEnabled, v => Flags.ScopedEnabled = v);
                RenderBoolSetting("Flashed", () => Flags.FlashEnabled, v => Flags.FlashEnabled = v);
                RenderBoolSetting("Show Distance Text", () => DistanceText.Enabled, v => DistanceText.Enabled = v);
                RenderBoolSetting("Show Name", () => NameDisplay.Enabled, v => NameDisplay.Enabled = v);
                RenderBoolSetting("Gun Icon", () => Flags.GunEnabled, v => Flags.GunEnabled = v);
                RenderBoolSettingWith1ColorPicker("Ping Display", () => PingDisplay.Enabled, v => PingDisplay.Enabled = v, ref PingDisplay.PingTextColor);
            }),
            new("Bone ESP", 2, () =>
            {
                RenderBoolSettingWith2ColorPickers("Enable Bone ESP", () => BoneESP.EnableBoneESP, v => BoneESP.EnableBoneESP = v, ref BoneESP.VisibleColors.TeamRGB, ref BoneESP.VisibleColors.EnemyRGB, ref BoneESP.VisibleColors.TeamColor, ref BoneESP.VisibleColors.EnemyColor);
                RenderBoolSettingWith2ColorPickers("Visibility Check", () => BoneESP.visibilityCheck, v => BoneESP.visibilityCheck = v, ref BoneESP.OccludedColors.TeamRGB, ref BoneESP.OccludedColors.EnemyRGB, ref BoneESP.OccludedColors.TeamColor, ref BoneESP.OccludedColors.EnemyColor);
                RenderIntCombo("Bone ESP Type", ref BoneESP.CurrentType, BoneESP.Types.ToList(), BoneESP.Types.Length);
                RenderBoolSetting("Team Check##BoneESP", () => BoneESP.TeamCheck, v => BoneESP.TeamCheck = v);
                RenderFloatSlider("Bone Glow", ref BoneESP.GlowAmount, 0, 1f);
            }),
            new("Tracers", 2, () =>
            {
                RenderBoolSettingWith2ColorPickers("Enable Tracers", () => Tracers.EnableTracers, v => Tracers.EnableTracers = v, ref Tracers.TracerColors.TeamRGB, ref Tracers.TracerColors.EnemyRGB, ref Tracers.TracerColors.TeamColor, ref Tracers.TracerColors.EnemyColor);
                RenderIntCombo("Tracer Start Position", ref Tracers.CurrentStartPos, Tracers.StartPositions, Tracers.StartPositions.Count, false);
                RenderIntCombo("Tracer End Position", ref Tracers.CurrentEndPos, Tracers.EndPositions.ToList(), Tracers.EndPositions.Length);
                RenderFloatSlider("Tracer Thickness", ref Tracers.LineThickness, 0.05f, 5f);
            }),
            new("Chams", 2, () =>
            {
                RenderBoolSettingWith2ColorPickers("Chams", () => Chams.Enabled, v => Chams.Enabled = v, ref Chams.VisibleColors.TeamRGB, ref Chams.VisibleColors.EnemyRGB,  ref Chams.VisibleColors.TeamColor, ref Chams.VisibleColors.EnemyColor);
                RenderBoolSetting("Team Check##Chams", () => Chams.TeamCheck, v => Chams.TeamCheck = v);
                RenderIntCombo("Chams Style", ref Chams.StyleIndex, Chams.StyleNames.ToList(), Chams.StyleNames.Length);
                RenderBoolSetting("Pixel Perfect Depth", () => Chams.PixelPerfect, v => Chams.PixelPerfect = v);
            }),
            new("Other Visuals", 2, () =>
            {
                RenderBoolSetting("Bomb Timer", () => BombTimerOverlay.EnableTimeOverlay, v => BombTimerOverlay.EnableTimeOverlay = v);
                RenderBoolSetting("Spectator List", () => SpectatorList.Enabled, v => SpectatorList.Enabled = v);
                RenderBoolSettingWithWarning("Anti Flash", () => NoFlash.NoFlashEnable, v => NoFlash.NoFlashEnable = v);
                RenderBoolSettingWithWarning("FOV Changer", () => FovChanger.Enabled, v => FovChanger.Enabled = v);
                RenderBoolSettingWithWarning("Third Person", () => ThirdPerson.Enabled, v => ThirdPerson.Enabled = v);
                RenderIntSlider("Desired FOV", ref FovChanger.FOV, 60, 160);
                RenderBoolSettingWith2ColorPickers("Radar", () => Radar.IsEnabled, v => Radar.IsEnabled = v, ref Radar.PointColors.TeamRGB, ref Radar.PointColors.EnemyRGB, ref  Radar.PointColors.TeamColor, ref Radar.PointColors.EnemyColor);
                RenderBoolSetting("Draw Team", () => Radar.DrawOnTeam, v => Radar.DrawOnTeam = v);
                RenderBoolSetting("Draw Cross", () => Radar.DrawCrossb, v => Radar.DrawCrossb = v);
                        RenderBoolSettingWith2ColorPickers("Sound ESP", () => SoundESP.Enabled, v => SoundESP.Enabled = v, ref SoundESP.VisibleColors.TeamRGB, ref SoundESP.VisibleColors.EnemyRGB, ref SoundESP.VisibleColors.TeamColor, ref SoundESP.VisibleColors.EnemyColor);
            }),
            new("World ESP", 2, () =>
            {
                RenderBoolSettingWith2ColorPickers("C4 ESP", () => C4ESP.Enabled, v => C4ESP.Enabled = v, ref C4ESP.Colors.PrimaryRGB, ref C4ESP.Colors.SecondaryRGB, ref C4ESP.Colors.PrimaryColor, ref C4ESP.Colors.SecondaryColor);
                RenderBoolSettingWith1ColorPicker("Dropped Weapon ESP", () => WorldESP.DroppedWeaponESP, v => WorldESP.DroppedWeaponESP = v, ref WorldESP.WeaponTextColor);
                RenderBoolSettingWith1ColorPicker("Dropped Hostage ESP", () => WorldESP.HostageESP, v => WorldESP.HostageESP = v, ref WorldESP.HostageTextColor);
                RenderBoolSettingWith1ColorPicker("Chicken ESP", () => WorldESP.ChickenESP, v => WorldESP.ChickenESP = v, ref WorldESP.ChickenTextColor);
                RenderBoolSettingWith1ColorPicker("Projectile ESP", () => WorldESP.ProjectileESP, v => WorldESP.ProjectileESP = v, ref WorldESP.ProjectileTextColor);
                RenderBoolSettingWith1ColorPicker("World ESP Boxes", () => WorldESP.DrawBoxes, v => WorldESP.DrawBoxes = v, ref WorldESP.BoxColor);
                RenderBoolSettingWith2ColorPickers("Molotov Bounds", () => WorldESP.MolotovBoundsESP, v => WorldESP.MolotovBoundsESP = v, ref WorldESP.MolotovColors.PrimaryRGB, ref WorldESP.MolotovColors.SecondaryRGB, ref WorldESP.MolotovColors.PrimaryColor, ref WorldESP.MolotovColors.SecondaryColor);
                RenderBoolSetting("World ESP Text", () => WorldESP.DrawText, v => WorldESP.DrawText = v);
                RenderBoolSetting("World Chams", () => WorldESP.DrawChams, v => WorldESP.DrawChams = v);
            }),

            new("GUI", 4, () =>
            {
                RenderFloatSlider("Window Alpha", ref Renderer.WindowAlpha, 0.1f, 1.0f, "%.2f");
                Render2ColorPickers("Menu Colors", ref Renderer.MenuColors.PrimaryRGB, ref Renderer.MenuColors.SecondaryRGB, ref Renderer.MenuColors.PrimaryColor, ref Renderer.MenuColors.SecondaryColor, () =>{Renderer.ApplyColors(); });
                RenderFloatSlider("Animation Speed", ref Renderer.AnimationSpeed, 0.01f, 1.0f, "%.2f");
                RenderFloatSlider("Particle Speed", ref Renderer.ParticleSpeed, 0, 10);
                RenderColorPicker("Particle Color", ref Renderer.BackgroundEffectColors.PrimaryColor, ref Renderer.BackgroundEffectColors.PrimaryRGB, () =>{Renderer.ApplyColors(); });
                RenderColorPicker("Line Color", ref Renderer.BackgroundEffectColors.SecondaryColor, ref Renderer.BackgroundEffectColors.SecondaryRGB, () =>{Renderer.ApplyColors(); });
                Keybind.RenderKeybindChooser("Open Keybind", ref Renderer.OpenKeyInt, () => Renderer.DrawWindow);
                RenderBoolSetting("Menu Sounds", () => Renderer.MenuSounds, v => Renderer.MenuSounds = v);
                RenderFloatSlider("Menu Sounds Volume", ref Renderer.MenuSoundsVolume, 0, 1);
                RenderBoolSetting("Watermark", () => Renderer.EnableWatermark, v => Renderer.EnableWatermark = v);
            }),
            new("Performance", 4, () =>
            {
                RenderBoolSetting("Use Old Visibility Check", () => EntityManager.UseOldVisibilityCheck, v => EntityManager.UseOldVisibilityCheck = v);
                RenderBoolSetting("VSync", () => Renderer.EnableVsync, v => Renderer.EnableVsync = v);
            }),
            new("About", 4, () =>
            {
                ImGui.Text($"Titled GUI V{Configs.Version}");
                ImGui.Text("External Cheat Made By xfi0 / domok.");
                ImGui.Text("More Info On: " + Configs.Link);
                ImGui.TextWrapped("If you paid for this you have been scammed.\nThis never was, and will never be paid.\nPlease report any paid versions of this to my github or discord.");
            }),
        };

        return sections;
    }

    public static void BeginSection(string label, Action content, Vector2 size)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ChildRounding);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Renderer.MenuColors.SecondaryRGB ? Colors.Rgb(Renderer.WindowAlpha) : Renderer.MenuColors.SecondaryColor);
        ImGui.BeginChild(label, size, ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY);
        ImGui.Text(label);
        ImGui.Separator();
        content();
        ImGui.EndChild();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }
}