using ImGuiNET;
using System.Drawing.Imaging;
using System.Numerics;
using Titled_Gui.Data.Game;
using static Titled_Gui.Notifications.Library;

namespace Titled_Gui.Notifications
{
    internal class Library
    {
        public class Notification
        {
            public string NotificationTitle = string.Empty;
            public string NotificationMessage = string.Empty;
            public float DisappearDelay = 5f;
            public float SlideInProgress = 0f;
            public float SlideOutProgress = 0f;
            public float PositionY = 0f;
        }
        public static List<Notification> Notifications = new();
        private static float LastNotificationPositionY = 0f;
        private static int MaxNotifications = 10;
        public static void SendNotification(string title, string message)
        {
            lock (Notifications)
            {
                if (Notifications.Count >= MaxNotifications && Notifications.Count > 0)
                {
                    return;
                }


                if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message))
                    return;

                Notification notification = new()
                {
                    NotificationTitle = title,
                    NotificationMessage = message,
                    PositionY = LastNotificationPositionY
                };
                LastNotificationPositionY += 65f;

                Notifications.Add(notification);

                Console.WriteLine("Notif Sent");
            }
        }

        public static void UpdateNotifications(float deltaTime)
        {
            lock (Notifications)
            {
                for (int i = 0; i < Notifications.Count; i++)
                {
                    var notification = Notifications[i];

                    if (notification.DisappearDelay > 0)
                        notification.DisappearDelay -= deltaTime;
                    else
                    {
                        if (notification.SlideOutProgress < 1f)
                            notification.SlideOutProgress += deltaTime;
                    }

                    notification.DisappearDelay = Math.Max(notification.DisappearDelay, 0f);

                    if (notification.SlideInProgress < 5f)
                        notification.SlideInProgress += 0.03f;  

                    if (notification.SlideOutProgress < 5f)
                        notification.SlideInProgress += 0.03f;

                    SlideIn(notification);

                    //Console.WriteLine("updating: " + Notifications.Count);

                    //Console.WriteLine(notification.DisappearDelay);
                    if (notification.DisappearDelay != 0)
                        continue;

                    SlideOut(notification);
                    Notifications.RemoveAt(i);
                    LastNotificationPositionY -= 65f;
                    i--;
                }
            }
        }

        public static void DrawNotification(Notification notification, Vector2 position)
        {
            GameState.renderer?.drawList.AddRectFilled(new Vector2(10f, 10f), new Vector2(position.X + 50f, position.Y + 50f),
                ImGui.ColorConvertFloat4ToU32(new(0.094f, 0.101f, 0.117f, 1.0f)), 3f);

            GameState.renderer?.drawList.AddText(new Vector2(15f, position.Y + 5f),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 255)), notification.NotificationTitle);

            GameState.renderer?.drawList.AddText(new Vector2(15f, position.Y + 25f),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 255)), notification.NotificationMessage);
        }

        public static void SlideIn(Notification notification)
        {
            Vector2 position = new((float)Math.Clamp(200f * notification.SlideInProgress, 0f, 200f), notification.PositionY);

            DrawNotification(notification, position);
        }

        public static void SlideOut(Notification notification)
        {
            Vector2 position = new((float)Math.Clamp(200f - (notification.SlideOutProgress * 200f), 0f, 200f), notification.PositionY);

            DrawNotification(notification, position);
        }

        public static void ClearAllNotifications()
        {
            lock (Notifications)
                Notifications.Clear();
        }

        public static void Wrap()
        {

        }
    }
}