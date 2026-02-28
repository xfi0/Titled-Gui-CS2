using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Notifications
{
    internal class Library
    {
        public static List<string> notifis = new();
        public static string NotificationText = string.Empty;
        public static void LoadOnce()
        {
            Console.WriteLine("\nLoading notis once.");
            ImGui.Begin("Titled_NotificationHud");

            GameState.renderer.drawList.AddRectFilled(new Vector2(50, 150), new Vector2(50, 150), ImGui.ColorConvertFloat4ToU32(new Vector4(128, 0, 0, 255)));

            ImGui.Text(NotificationText);

            ImGui.End();
        }

        public static void SendNotification(string title, string message)
        {
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(message))
            {
                notifis.Add(title);
                Console.WriteLine($"\nSending notification: {title} - {message}");
                NotificationText = $"{title}: {message}";
            }
        }

        public static void ClearAllNotifications()
        {
            foreach (var item in notifis)
            {
                notifis.Clear();
            }
            NotificationText = "";
        }

        public static void Wrap()
        {

        }
    }
}