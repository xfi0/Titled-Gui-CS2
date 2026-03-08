using ImGuiNET;
using System.Diagnostics;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;
using Types = Titled_Gui.Data.Menu.Types;

try
{
    if (GameState.swed == null) return;
    // initialization
    await OffsetGetter.UpdateOffsetsAsync();

    EntityManager entityManager = new();
    ImGui.CreateContext();
    Renderer.LoadFonts();
    Thread renderThread = new(() => GameState.renderer.Start().Wait())
    {
        IsBackground = true,
    };
    renderThread.Start();
    // entities
    List<Entity>? entities = [];
    Thread entityUpdateThread = new(() =>
    {
        while (true)
        {
            try
            {
                if (entityManager != null)
                {
                    entities = EntityManager.GetEntities();
                    Entity localPlayer = EntityManager.GetLocalPlayer();

                    GameState.LocalPlayer = localPlayer;

                    GameState.renderer.UpdateLocalPlayer(localPlayer);
                }
                if (entities != null)
                {
                    GameState.renderer.UpdateEntities(entities);
                    GameState.Entities = [.. entities];
                }
                Thread.Sleep(1);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception At Entity Update Thread" + e.StackTrace);
            }
        }
    })
    {
        IsBackground = true,
        Priority = ThreadPriority.Highest
    };
    entityUpdateThread.Start();

    ThreadService.StartAllThreadServices();
    while (true)
    {
        Thread.Sleep(100);
    }
}
catch (IndexOutOfRangeException)
{
    Console.WriteLine("IndexOutOfRangeException, Please Make Sure Your Game Is Running.");
}
catch (Exception e)
{
    Console.WriteLine("Exception: " + e.Message);
}

