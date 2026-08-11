using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Classes;

namespace Titled_Gui.ImGUI.Widgets
{
    public class Keybind
    {
        public struct Bind
        {
            public string name;
            public bool choosing;
            public bool enabled;
            public Func<bool>? moduleState;
        }

        public static List<Bind> KeyBinds = [];

        private static Bind GetBind(string name, Func<bool>? moduleState = null)
        {
            for (int i = 0; i < KeyBinds.Count; i++)
            {
                Bind bind = KeyBinds[i];
                if (bind.name == name)
                {
                    if (moduleState != null)
                    {
                        bind.moduleState = moduleState;
                        bind.enabled = moduleState.Invoke();
                        KeyBinds[i] = bind;
                    }
                    return bind;
                }
            }

            Bind newBind = new()
            {
                name = name,
                choosing = false,
                moduleState = moduleState,
                enabled = moduleState != null ? moduleState.Invoke() : false
            };
            KeyBinds.Add(newBind);
            return newBind;
        }

        private static void SetBind(string name, Func<Bind, Bind> update, Func<bool>? moduleState = null)
        {
            for (int i = 0; i < KeyBinds.Count; i++)
            {
                Bind bind = KeyBinds[i];
                if (bind.name == name)
                {
                    if (moduleState != null)
                        bind.moduleState = moduleState;
                    bind = update(bind);
                    KeyBinds[i] = bind;
                    return;
                }
            }

            Bind newBind = new()
            {
                name = name,
                choosing = false,
                moduleState = moduleState,
                enabled = moduleState != null ? moduleState.Invoke() : false
            };
            KeyBinds.Add(update(newBind));
        }

        public static void RenderKeybindChooser(string label, string? id, ref int key, Func<bool>? moduleState = null)
        {
            string keyId = id ?? label;
            ImGui.PushID(keyId);

            Bind bind = GetBind(keyId, moduleState);

            if (ImGui.Button(bind.choosing ? "Press Any Key..." : (key == (int)Keys.None ? "None" : Enum.GetName(typeof(Keys), key) ?? key.ToString()), new Vector2(100, 0)))
                SetBind(keyId, b => { b.choosing = true; return b; }, moduleState);

            ImGui.SameLine();

            if (ImGui.Button("X"))
                key = (int)Keys.None;

            if (bind.choosing)
            {
                foreach (Keys k in Enum.GetValues<Keys>())
                {
                    if (k == Keys.None) continue;

                    short state = (short)User32.GetAsyncKeyState((int)k);
                    bool pressed = (state & 0x8000) != 0;

                    if (!pressed) continue;
                    key = (int)k;

                    SetBind(keyId, b => { b.choosing = false; return b; });
                    break;
                }
            }

            ImGui.PopID();
        }

        public static void RenderKeybindChooser(string label, string? id, ref ImGuiKey key, Func<bool>? moduleState = null)
        {
            string keyId = id ?? label;
            ImGui.PushID(keyId);

            Bind bind = GetBind(keyId, moduleState);

            string keyName = bind.choosing ? "Press Any Key..." : (key == ImGuiKey.None ? "None" : key.ToString());

            if (ImGui.Button(keyName, new Vector2(100, 0)))
                SetBind(keyId, b => { b.choosing = true; return b; }, moduleState);

            if (bind.choosing)
            {
                foreach (ImGuiKey imguiKey in Enum.GetValues<ImGuiKey>())
                {
                    if (!ImGui.IsKeyPressed(imguiKey))
                        continue;
                    if (imguiKey >= ImGuiKey.MouseLeft && imguiKey <= ImGuiKey.MouseWheelY) continue;

                    if (imguiKey == ImGuiKey.Escape)
                        key = ImGuiKey.Insert;

                    else
                        key = imguiKey;

                    SetBind(keyId, b => { b.choosing = false; return b; });
                    break;
                }
            }


            ImGui.PopID();
        }

        public static void RenderKeybindMenu()
        {
            ImGui.Begin("Keybinds");
            float padding = 5;
            for (int i = 0; i < KeyBinds.Count; i++)
            {
                ImGui.SetCursorPosX(padding);
                Bind bind = KeyBinds[i];
                if (bind.moduleState != null)
                {
                    bind.enabled = bind.moduleState.Invoke();
                    KeyBinds[i] = bind;
                }
                ImGui.Text(bind.name.Replace("Keybind", "") + " [" + (bind.enabled ? "ON" : "OFF") + "]");
            }
            ImGui.End();
        }
    }
}
