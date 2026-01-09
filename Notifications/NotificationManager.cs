
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Classes
{
    public enum NotificationType
    {
        Success,
        Error,
        Info,
        Warning
    }

    public class Notification
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public float Lifetime { get; set; }
        public float Timer { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 EndPosition { get; set; }
        public float AnimationProgress { get; set; }
        public float SlideOutProgress { get; set; }
        public bool IsRemoving { get; set; }
        public bool IsSlidingOut { get; set; }

        public Notification(string message, NotificationType type, float lifetime = 3.0f)
        {
            Message = message;
            Type = type;
            Lifetime = lifetime;
            Timer = 0f;
            AnimationProgress = 0f;
            SlideOutProgress = 0f;
            IsRemoving = false;
            IsSlidingOut = false;
            Position = Vector2.Zero;
            StartPosition = Vector2.Zero;
            EndPosition = Vector2.Zero;
        }
    }

    public static class NotificationManager
    {
        private static List<Notification> notifications = new List<Notification>();
        private static float notificationWidth = 280f;
        private static float notificationHeight = 80f;
        private static float spacing = 15f;
        private static float animationSpeed = 12f;
        private static float slideOutSpeed = 5f;
        private static float startY = 90f;
        private static float startX = 0f;
        private static float slideDistance = 400f;

        private static ImFontPtr glyphFont = default;
        private static ImFontPtr textFont = default;
        private static bool fontsLoaded = false;
        private static bool fontLoadAttempted = false;

        private static Dictionary<NotificationType, Vector4> notificationBorderColors = new()
        {
            { NotificationType.Success, new Vector4(0.2f, 0.6f, 1.0f, 1.0f) },  // Niebieski
            { NotificationType.Error, new Vector4(0.2f, 0.6f, 1.0f, 1.0f) },    // Niebieski
            { NotificationType.Warning, new Vector4(0.2f, 0.6f, 1.0f, 1.0f) },  // Niebieski
            { NotificationType.Info, new Vector4(0.2f, 0.6f, 1.0f, 1.0f) }      // Niebieski
        };

        private static Dictionary<NotificationType, string> notificationIcons = new()
        {
            { NotificationType.Success, "A" },  // glyph.ttf
            { NotificationType.Error, "F" },    // glyph.ttf
            { NotificationType.Warning, "C" },  // glyph.ttf
            { NotificationType.Info, "D" }      // glyph.ttf
        };

        private static void LoadFonts()
        {
            if (fontsLoaded || fontLoadAttempted) return;

            fontLoadAttempted = true;

            try
            {
                // Sprawdzamy czy główne fonty z Renderer są już załadowane
                if (Renderer.IsTextFontNormalLoaded)
                {
                    textFont = Renderer.TextFontNormal;

                    // Próbujemy załadować font glyph jeśli jest dostępny
                    if (Renderer.IsIconFontLoaded)
                    {
                        glyphFont = Renderer.IconFont;
                    }

                    fontsLoaded = true;
                    System.Console.WriteLine("NotificationManager: Fonty załadowane pomyślnie!");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"NotificationManager: Błąd ładowania fontów: {ex.Message}");
                fontsLoaded = false;
            }
        }

        public static void AddNotification(string message, NotificationType type)
        {
            var notification = new Notification(message, type);
            notifications.Add(notification);

            if (notifications.Count > 5)
            {
                var toRemove = notifications.Find(n => n.IsSlidingOut || n.IsRemoving);
                if (toRemove != null)
                {
                    notifications.Remove(toRemove);
                }
                else if (notifications.Count > 5)
                {
                    notifications.RemoveAt(0);
                }
            }
        }

        public static void Update()
        {
            if (GameState.renderer == null) return;

            float deltaTime = ImGui.GetIO().DeltaTime;
            float screenWidth = GameState.renderer.screenSize.X;
            startX = screenWidth + 20f;

            for (int i = notifications.Count - 1; i >= 0; i--)
            {
                var notif = notifications[i];

                if (notif.IsSlidingOut)
                {
                    notif.SlideOutProgress += deltaTime * slideOutSpeed;
                    if (notif.SlideOutProgress >= 1f)
                    {
                        notifications.RemoveAt(i);
                        continue;
                    }

                    float slideX = notif.StartPosition.X + (slideDistance * notif.SlideOutProgress);
                    notif.Position = new Vector2(slideX, notif.StartPosition.Y);
                    continue;
                }

                notif.Timer += deltaTime;

                if (notif.AnimationProgress < 1f)
                {
                    notif.AnimationProgress += deltaTime * animationSpeed;
                    if (notif.AnimationProgress > 1f)
                        notif.AnimationProgress = 1f;
                }

                if (notif.Timer >= notif.Lifetime && !notif.IsRemoving)
                {
                    notif.IsSlidingOut = true;
                    notif.SlideOutProgress = 0f;
                    notif.StartPosition = notif.Position;
                    continue;
                }

                if (!notif.IsSlidingOut)
                {
                    float targetY = startY + (notificationHeight + spacing) * i;
                    float targetX = screenWidth - notificationWidth - 20f;

                    if (notif.Position == Vector2.Zero)
                    {
                        notif.Position = new Vector2(screenWidth + 50f, targetY);
                        notif.EndPosition = new Vector2(targetX, targetY);
                    }
                    else
                    {
                        float ease = 0.1f;
                        float newX = notif.Position.X + (targetX - notif.Position.X) * ease;
                        float newY = notif.Position.Y + (targetY - notif.Position.Y) * ease;
                        notif.Position = new Vector2(newX, newY);
                    }
                }
            }
        }

        public static void Render()
        {
            if (!fontsLoaded)
            {
                LoadFonts();
            }

            Update();

            foreach (var notif in notifications)
            {
                if (notif.AnimationProgress <= 0f && !notif.IsSlidingOut) continue;

                float alpha = notif.AnimationProgress;
                if (notif.IsSlidingOut)
                {
                    alpha = 1f - notif.SlideOutProgress;
                }

                // Szare tło
                Vector4 bgColor = new Vector4(0.25f, 0.25f, 0.25f, 0.95f * alpha);

                // Niebieska obwódka
                Vector4 borderColor = new Vector4(
                    notificationBorderColors[notif.Type].X,
                    notificationBorderColors[notif.Type].Y,
                    notificationBorderColors[notif.Type].Z,
                    notificationBorderColors[notif.Type].W * alpha
                );

                // Biały tekst
                Vector4 textColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f * alpha);

                Vector2 pos = notif.Position;
                Vector2 size = new Vector2(notificationWidth, notificationHeight);

                // Cień
                if (!notif.IsSlidingOut && alpha > 0.3f)
                {
                    Vector4 shadowColor = new Vector4(0f, 0f, 0f, 0.3f * alpha);
                    Vector2 shadowOffset = new Vector2(3f, 3f);

                    ImGui.GetBackgroundDrawList().AddRectFilled(
                        pos + shadowOffset,
                        pos + size + shadowOffset,
                        ImGui.ColorConvertFloat4ToU32(shadowColor),
                        12.0f
                    );
                }

                // Szare tło
                ImGui.GetBackgroundDrawList().AddRectFilled(
                    pos,
                    pos + size,
                    ImGui.ColorConvertFloat4ToU32(bgColor),
                    12.0f
                );

                // Niebieska obramówka
                float borderSize = 3f;
                ImGui.GetBackgroundDrawList().AddRect(
                    pos,
                    pos + size,
                    ImGui.ColorConvertFloat4ToU32(borderColor),
                    12.0f,
                    ImDrawFlags.RoundCornersAll,
                    borderSize
                );

                // Ikona z glyph.ttf
                string icon = notificationIcons[notif.Type];

                if (fontsLoaded && !glyphFont.Equals(default(ImFontPtr)))
                {
                    ImGui.PushFont(glyphFont);
                }

                Vector2 iconSize = ImGui.CalcTextSize(icon);
                Vector2 iconPos = new Vector2(
                    pos.X + 20f,
                    pos.Y + (notificationHeight - iconSize.Y) / 2
                );

                ImGui.GetBackgroundDrawList().AddText(
                    iconPos,
                    ImGui.ColorConvertFloat4ToU32(textColor),
                    icon
                );

                if (fontsLoaded && !glyphFont.Equals(default(ImFontPtr)))
                {
                    ImGui.PopFont();
                }

                // Tekst z NotoSans-Bold
                if (fontsLoaded && !textFont.Equals(default(ImFontPtr)))
                {
                    ImGui.PushFont(textFont);
                }

                Vector2 textPos = new Vector2(
                    pos.X + 55f,
                    pos.Y + (notificationHeight - ImGui.CalcTextSize(notif.Message).Y) / 2
                );

                ImGui.GetBackgroundDrawList().AddText(
                    textPos,
                    ImGui.ColorConvertFloat4ToU32(textColor),
                    notif.Message
                );

                if (fontsLoaded && !textFont.Equals(default(ImFontPtr)))
                {
                    ImGui.PopFont();
                }

                // Pasek czasu
                if (!notif.IsSlidingOut && alpha > 0.5f)
                {
                    float timeLeft = 1f - (notif.Timer / notif.Lifetime);
                    timeLeft = System.Math.Max(0f, timeLeft);

                    float barWidth = (notificationWidth - 40f) * timeLeft;
                    Vector4 timeBarColor = new Vector4(0.2f, 0.6f, 1.0f, 0.8f * alpha); // Niebieski pasek
                    Vector4 timeBarBg = new Vector4(0.15f, 0.15f, 0.15f, 0.6f * alpha);

                    // Tło paska
                    ImGui.GetBackgroundDrawList().AddRectFilled(
                        new Vector2(pos.X + 20f, pos.Y + notificationHeight - 12f),
                        new Vector2(pos.X + notificationWidth - 20f, pos.Y + notificationHeight - 7f),
                        ImGui.ColorConvertFloat4ToU32(timeBarBg),
                        3.0f
                    );

                    // Wypełniony pasek czasu
                    if (barWidth > 2f)
                    {
                        ImGui.GetBackgroundDrawList().AddRectFilled(
                            new Vector2(pos.X + 20f, pos.Y + notificationHeight - 12f),
                            new Vector2(pos.X + 20f + barWidth, pos.Y + notificationHeight - 7f),
                            ImGui.ColorConvertFloat4ToU32(timeBarColor),
                            3.0f
                        );

                        // Efekt "płynącego" paska
                        if (timeLeft > 0.1f)
                        {
                            Vector4 glowColor = new Vector4(1f, 1f, 1f, 0.3f * alpha * (float)System.Math.Sin(notif.Timer * 5f));
                            float glowWidth = 10f;

                            ImGui.GetBackgroundDrawList().AddRectFilled(
                                new Vector2(pos.X + 20f + barWidth - glowWidth, pos.Y + notificationHeight - 12f),
                                new Vector2(pos.X + 20f + barWidth, pos.Y + notificationHeight - 7f),
                                ImGui.ColorConvertFloat4ToU32(glowColor),
                                2.0f
                            );
                        }
                    }

                    // Tekst z czasem (biały)
                    if (fontsLoaded && !textFont.Equals(default(ImFontPtr)))
                    {
                        ImGui.PushFont(textFont);
                    }

                    string timeText = $"{System.Math.Ceiling(notif.Lifetime - notif.Timer)}s";
                    Vector2 timeTextSize = ImGui.CalcTextSize(timeText);
                    Vector2 timeTextPos = new Vector2(
                        pos.X + notificationWidth - 20f - timeTextSize.X,
                        pos.Y + notificationHeight - 25f
                    );

                    ImGui.GetBackgroundDrawList().AddText(
                        timeTextPos,
                        ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.7f * alpha)),
                        timeText
                    );

                    if (fontsLoaded && !textFont.Equals(default(ImFontPtr)))
                    {
                        ImGui.PopFont();
                    }
                }

                // Efekt "slide out"
                if (notif.IsSlidingOut && notif.SlideOutProgress > 0.3f)
                {
                    float fadeProgress = (notif.SlideOutProgress - 0.3f) / 0.7f;
                    Vector4 fadeColor = new Vector4(0f, 0f, 0f, 0.5f * fadeProgress);

                    ImGui.GetBackgroundDrawList().AddRectFilled(
                        pos,
                        pos + size,
                        ImGui.ColorConvertFloat4ToU32(fadeColor),
                        12.0f
                    );
                }
            }
        }

        public static void ClearAll()
        {
            foreach (var notif in notifications)
            {
                notif.IsSlidingOut = true;
                notif.SlideOutProgress = 0f;
                notif.StartPosition = notif.Position;
            }
        }
    }
}
