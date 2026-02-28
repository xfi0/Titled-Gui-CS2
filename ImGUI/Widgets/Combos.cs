using ImGuiNET;
using static Titled_Gui.Classes.UploadHelper;
using static Titled_Gui.ImGUI.Widgets.Misc;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Combos
    {
        private static string? pendingUpload = null;

        public static void RenderIntCombo(string label, ref int current, List<string> items, int itemCount,
         bool withUpload = false, float widgetWidth = 160f)
        {
            if (withUpload) 
                LoadUploads(label + "Uploads", ref items);
            

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
                if (pendingUpload != null)
                {
                    Console.WriteLine($"Saving: '{pendingUpload}'");
                    string temPendingUpload = pendingUpload;

                    Task.Run(async () =>
                    {
                        await SaveUpload(label + "Uploads", temPendingUpload);
                    });
                    items.Add(pendingUpload);
                    temp = items.Count - 1;
                    pendingUpload = null;
                }

                if (withUpload)
                {
                    if (!ImGui.BeginCombo("##" + label, items[temp]))
                        return;

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
                    ImGui.Combo("##" + label, ref temp, items.ToArray(), items.Count);
            }, widgetWidth);


            current = temp;
        }
    }
}
