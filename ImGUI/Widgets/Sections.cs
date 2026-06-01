using ImGuiNET;
using System.Numerics;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
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
    public class SectionT
    {
        public string label;
        public Action content;
        public int tab;

        public SectionT(string label, int tab, Action content)
        {
            this.label = label;
            this.tab = tab;
            this.content = content;
        }
    }

    public static List<SectionT> sections = new()
{
    new("Misc", 0, () =>
    {
        RenderBoolSettingWithWarning("Auto Bunny Hop", ref Bhop.BhopEnable);
        RenderBoolSetting("Hit Sound", ref HitStuff.Enabled);
        RenderFloatSlider("Hit Sound Volume", ref HitStuff.Volume, 0, 1);
        RenderIntCombo("Current Hit Sound", ref HitStuff.CurrentHitSound, HitStuff.HitSoundDisplays, HitStuff.HitSounds.Count, true);
        RenderBoolSettingWith1ColorPicker("Headshot Text", ref HitStuff.EnableHeadshotText, ref HitStuff.TextColor);
    }),
    new("Gernade Helper", 0, () =>
    {
        RenderBoolSetting("Enable Gernade Helper", ref GernadeLineup.Enabled);
        RenderBoolSetting("Always Show", ref GernadeLineup.AlwaysShow);
        Render2ColorPickers("Position Circle Color", ref GernadeLineup.PositionColorInside, ref GernadeLineup.PositionColorOutside);
        ImGui.InputText("Lineup Name", ref GernadeLineup.LineupName, 128);
        RenderIntCombo("Linup Type", ref GernadeLineup.SelectedType, GernadeLineup.TypesList, GernadeLineup.TypesList.Count, false);
        Titled_Gui.ImGUI.Widgets.Button.RenderButton("Save Lineup", () => {
            GernadeLineup.SaveLineup(GernadeLineup.LineupName, (Titled_Gui.Data.Menu.Types.GernadeLaunchType)GernadeLineup.SelectedType); 
        });
    }),

    new("Aimbot", 1, () =>
    {
        RenderBoolSetting("Enable Aimbot", ref Aimbot.AimbotEnable);
        RenderIntCombo("Aim Bone", ref Aimbot.CurrentBone, Aimbot.Bones.ToList(), Aimbot.Bones.Length);
        Renderer.RenderKeybindChooser("Aimbot Keybind", ref Aimbot.AimbotKey);
        RenderBoolSetting("Aim On Team", ref Aimbot.Team);
        RenderFloatSlider("Smoothing X", ref Aimbot.SmoothingX, 0, 20, "%.2f");
        RenderFloatSlider("Smoothing Y", ref Aimbot.SmoothingY, 0, 20, "%.2f");
        RenderBoolSetting("Draw FOV", ref Aimbot.DrawFov);
        RenderBoolSetting("Use FOV", ref Aimbot.UseFOV);
        RenderBoolSetting("Scoped Check", ref Aimbot.ScopedOnly);
        RenderIntSlider("FOV Size", ref Aimbot.FovSize, 10, 1000, "%d");
        RenderColorPicker("FOV Color", ref Aimbot.FovColor);
        RenderBoolSetting("Visibility Check", ref Aimbot.VisibilityCheck);
        RenderBoolSetting("Target Line", ref Aimbot.TargetLine);
    }),
    new("RCS", 1, () =>
    {
        RenderBoolSetting("RCS", ref RCS.Enabled);
        RenderFloatSlider("Strength", ref RCS.Strength, 0f, 1f, "%.2f");
    }),
    new("Triggerbot", 1, () =>
    {
        RenderBoolSetting("Triggerbot", ref TriggerBot.Enabled);
        RenderBoolSetting("Team Check", ref TriggerBot.TeamCheck);
        Renderer.RenderKeybindChooser("Trigger Bot Keybind", ref TriggerBot.TriggerKey);
        RenderIntSlider("Max Delay", ref TriggerBot.MaxDelay, 0, 1000, "%d");
        RenderIntSlider("Min Delay", ref TriggerBot.MinDelay, 0, 1000, "%d");
        RenderBoolSetting("Require Keybind", ref TriggerBot.RequireKeybind);
    }),

    new("Box ESP", 2, () =>
    {
        RenderBoolSetting("Enable ESP", ref BoxESP.EnableESP);
        RenderIntCombo("ESP Shape", ref BoxESP.CurrentShape, BoxESP.Shapes.ToList(), BoxESP.Shapes.Length, false);
        RenderBoolSetting("Team Check", ref BoxESP.TeamCheck);
        RenderBoolSetting("Enable RGB", ref Colors.RGB);
        RenderBoolSettingWith2ColorPickers("Box Fill Gradient", ref BoxESP.BoxFillGradient, ref BoxESP.BoxFillGradientColorTop, ref BoxESP.BoxFillGradientBottom);
        RenderBoolSettingWith2ColorPickers("Filled Boxes", ref BoxESP.FillBox, ref BoxESP.TeamFill, ref BoxESP.EnemyFill);
        RenderFloatSlider("Box Fill Opacity", ref BoxESP.BoxFillOpacity, 0.0f, 1.0f, "%.2f");
        RenderBoolSettingWith1ColorPicker("Inner Outline", ref BoxESP.InnerOutline, ref BoxESP.InnerOutlineColor);
        RenderFloatSlider("ESP Rounding", ref BoxESP.Rounding, 1f, 5f);
        RenderFloatSlider("ESP Glow", ref BoxESP.GlowAmount, 0f, 5f);
        RenderBoolSettingWith2ColorPickers("Outer Outline", ref BoxESP.OuterOutline, ref BoxESP.OutlineEnemyColor, ref BoxESP.OutlineTeamColor);
        RenderBoolSettingWith2ColorPickers("Visibility Check", ref BoneESP.visibilityCheck, ref BoxESP.OccludedEnemy, ref BoxESP.OccludedTeam);
        RenderBoolSetting("Flash Check", ref BoxESP.FlashCheck);
    }),
    new("Player ESP", 2, () =>
    {
        RenderBoolSetting("Enable Health Bar", ref HealthBar.EnableHealthBar);
        RenderBoolSetting("Enable Armor Bar", ref ArmorBar.EnableArmorhBar);
        RenderBoolSetting("Eye Ray", ref EyeRay.Enabled);
    }),
    new("Flags", 2, () =>
    {
        RenderBoolSetting("Show Distance Text", ref DistanceText.Enabled);
        RenderBoolSetting("Show Name", ref NameDisplay.Enabled);
        RenderBoolSettingWith1ColorPicker("Gun Icon", ref GunDisplay.Enabled, ref GunDisplay.TextColor);
        RenderBoolSettingWith1ColorPicker("Ping Display", ref PingDisplay.Enabled, ref PingDisplay.PingTextColor);
    }),
    new("Bone ESP", 2, () =>
    {
        RenderBoolSettingWith2ColorPickers("Enable Bone ESP", ref BoneESP.EnableBoneESP, ref BoneESP.VisibleBoneColor, ref BoneESP.OccludedBoneColor);
        RenderIntCombo("Bone ESP Type", ref BoneESP.CurrentType, BoneESP.Types.ToList(), BoneESP.Types.Length);
        RenderBoolSetting("Team Check", ref BoneESP.TeamCheck);
        RenderFloatSlider("Bone Glow", ref BoneESP.GlowAmount, 0, 1f);
    }),
    new("Tracers", 2, () =>
    {
        RenderBoolSettingWith2ColorPickers("Enable Tracers", ref Tracers.EnableTracers, ref Tracers.TeamColor, ref Tracers.EnemyColor);
        RenderIntCombo("Tracer Start Position", ref Tracers.CurrentStartPos, Tracers.StartPositions, Tracers.StartPositions.Count, false);
        RenderIntCombo("Tracer End Position", ref Tracers.CurrentEndPos, Tracers.EndPositions.ToList(), Tracers.EndPositions.Length);
        RenderFloatSlider("Tracer Thickness", ref Tracers.LineThickness, 0.05f, 5f);
    }),
    new("Chams & Sound", 2, () =>
    {
        RenderBoolSettingWith2ColorPickers("Chams", ref Chams.Enabled, ref Chams.TeamColor, ref Chams.EnemyColor);
        RenderBoolSettingWith2ColorPickers("Sound ESP", ref SoundESP.Enabled, ref SoundESP.TeamColor, ref SoundESP.EnemyColor);
    }),
    new("Other Visuals", 2, () =>
    {
        RenderBoolSetting("Enable Bomb Timer", ref BombTimerOverlay.EnableTimeOverlay);
        RenderBoolSettingWithWarning("Anti Flash", ref NoFlash.NoFlashEnable);
        RenderBoolSettingWithWarning("FOV Changer", ref FovChanger.Enabled);
        RenderBoolSettingWithWarning("Third Person", ref ThirdPerson.Enabled);
        RenderIntSlider("Desired FOV", ref FovChanger.FOV, 60, 160);
        RenderBoolSettingWith2ColorPickers("Radar", ref Radar.IsEnabled, ref Radar.EnemyPointColor, ref Radar.TeamPointColor);
        RenderBoolSetting("Draw Team", ref Radar.DrawOnTeam);
        RenderBoolSetting("Draw Cross", ref Radar.DrawCrossb);
    }),
    new("World ESP", 2, () =>
    {
        RenderBoolSettingWith1ColorPicker("C4 Box ESP", ref C4ESP.BoxEnabled, ref C4ESP.BoxColor);
        RenderBoolSettingWith1ColorPicker("C4 Text ESP", ref C4ESP.TextEnabled, ref C4ESP.TextColor);
        RenderBoolSettingWith1ColorPicker("Dropped Weapon ESP", ref WorldESP.DroppedWeaponESP, ref WorldESP.WeaponTextColor);
        RenderBoolSettingWith1ColorPicker("Dropped Hostage ESP", ref WorldESP.HostageESP, ref WorldESP.HostageTextColor);
        RenderBoolSettingWith1ColorPicker("Chicken ESP", ref WorldESP.ChickenESP, ref WorldESP.ChickenTextColor);
        RenderBoolSettingWith1ColorPicker("Projectile ESP", ref WorldESP.ProjectileESP, ref WorldESP.ProjectileTextColor);
        RenderBoolSettingWith1ColorPicker("World ESP Boxes", ref WorldESP.DrawBoxes, ref WorldESP.BoxColor);
        RenderBoolSettingWith2ColorPickers("Molotov Bounds", ref WorldESP.MolotovBoundsESP, ref WorldESP.molotovFillColor, ref WorldESP.molotovOutlineColor);
        RenderBoolSetting("World ESP Text", ref WorldESP.DrawText);
    }),

    new("GUI", 4, () =>
    {
        RenderFloatSlider("Window Alpha", ref Renderer.WindowAlpha, 0.1f, 1.0f, "%.2f");
        RenderColorPicker("Primary Color", ref Renderer.PrimaryColor);
        RenderColorPicker("Accent Color", ref Renderer.AccentColor);
        RenderFloatSlider("Animation Speed", ref Renderer.AnimationSpeed, 0.01f, 1.0f, "%.2f");
        RenderFloatSlider("Particle Speed", ref Renderer.ParticleSpeed, 0, 10);
        RenderColorPicker("Particle Color", ref Renderer.ParticleColor);
        RenderColorPicker("Line Color", ref Renderer.LineColor);
        Renderer.RenderKeybindChooser("Open Keybind", ref Renderer.OpenKeyInt);
        RenderBoolSetting("Menu Sounds", ref Renderer.MenuSounds);
        RenderFloatSlider("Menu Sounds Volume", ref Renderer.MenuSoundsVolume, 0, 1);
        RenderBoolSetting("Watermark", ref Renderer.EnableWatermark);
    }),
    new("Performance", 4, () =>
    {
        RenderBoolSetting("Use Old Visibility Check", ref EntityManager.UseOldVisibilityCheck);
        RenderBoolSetting("VSync", ref Renderer.EnableVsync);
    }),
    new("About", 4, () =>
    {
        ImGui.Text($"Titled GUI V{Configs.Version}");
        ImGui.Text("External Cheat Made By xfi0 / domok.");
        ImGui.Text("More Info On: " + Configs.Link);
        ImGui.TextWrapped("If you paid for this you have been scammed.\nThis never was, and will never be paid.\nPlease report any paid versions of this to my github or discord.");
    }),
};

    public static void BeginSection(string label, Action content, Vector2 size)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ChildRounding);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Renderer.AccentColor);
        ImGui.BeginChild(label, size, ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY);
        ImGui.Text(label);
        ImGui.Separator();
        content();
        ImGui.EndChild();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }
}