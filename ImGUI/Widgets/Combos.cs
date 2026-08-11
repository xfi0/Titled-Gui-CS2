using ImGuiNET;
using static Titled_Gui.Classes.UploadHelper;
using static Titled_Gui.ImGUI.Widgets.Misc;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Combos
    {
        private static string? pendingUpload = null;

        public static void RenderIntCombo(string label, string? id, ref int current, List<string> items, int itemCount,
         bool withUpload = false, float widgetWidth = 160f)
        {
            string widgetId = id ?? label;

            if (withUpload)
                LoadUploads(widgetId + "Uploads", ref items);

            int temp = current;
            if (items.Count == 0)
            {
                return;
            }

            if (temp < 0 || temp >= items.Count)
            {
                temp = 0;
            }
            RenderRowRightAligned(label, () =>
            {
                ImGui.PushID(widgetId);

                if (pendingUpload != null)
                {
                    Console.WriteLine($"Saving: '{pendingUpload}'");
                    string temPendingUpload = pendingUpload;

                    SaveUpload(widgetId + "Uploads", temPendingUpload);
                    items.Add(pendingUpload);
                    temp = items.Count - 1;
                    pendingUpload = null;
                }

                if (withUpload)
                {
                    if (!ImGui.BeginCombo("##combo", items[temp]))
                    {
                        ImGui.PopID();
                        return;
                    }

                    for (int i = 0; i < items.Count; i++)
                    {
                        bool isSelected = (temp == i);
                        if (ImGui.Selectable(items[i], isSelected))
                        {
                            temp = i;
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }

                    if (ImGui.Selectable("[Upload New Sound]"))
                    {
                        var thread = new Thread(() =>
                        {
                            using OpenFileDialog openFile = new();
                            openFile.Filter = "Audio files (*.wav)|*.wav|*.mp3|*.mp3|(*.ogg)|*.ogg|All files (*.*)|*.*";
                            openFile.Title = "Select a sound file";

                            if (openFile.ShowDialog() == DialogResult.OK)
                            {
                                pendingUpload = openFile.FileName;
                            }
                        });
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.IsBackground = true;
                        thread.Start();
                    }

                    ImGui.EndCombo();

                }
                else
                    ImGui.Combo("##combo", ref temp, items.ToArray(), items.Count);


                ImGui.PopID();
            }, widgetWidth);


            current = temp;
        }
    }
}