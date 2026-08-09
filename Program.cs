using ImGuiNET;
using System.Diagnostics;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;

try
{
    GameState.renderer = new();
    EntityManager entityManager = new();
    ImGui.CreateContext();
    Renderer.LoadFonts();
    await GameState.renderer.Start();
    GernadeLineup.Initialize();

    // entities
    List<Entity>? entities = [];
    while (!GameState.CS2Open())
    {
        Console.WriteLine("CS2 Not Found...");
        Thread.Sleep(1000);
    }

    GameState.memory = new("cs2");
    GameState.client = GameState.memory.GetModuleBase("client.dll");
    await OffsetGetter.UpdateOffsetsAsync();

    while (GameState.memory != null && !OffsetGetter.Updated)
    {
        if (GameState.CS2Open())
        {
            Process[] cs2 = GameState.GetCS2Process();
            Renderer.CS2ProcessId = cs2.FirstOrDefault()?.Id ?? 0;
            Process[] overlay = Process.GetProcessesByName("Titled GUI");
            Renderer.OverlayProcessId = overlay.FirstOrDefault()?.Id ?? 0;

            await OffsetGetter.CheckIfOffsetsAreValid();
        }
        await Task.Delay(500);
    }

    OffsetGetter.ApplySecondarySources();
    Thread entityUpdateThread = new(() =>
     {
         while (true)
         {
             try
             {
                 if (entityManager != null)
                 {
                     entities = EntityManager.GetEntities();
                 }
                 if (entities != null)
                 {
                     GameState.renderer.UpdateEntities(entities);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                     GameState.Entities = entities;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
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
        Thread.Sleep(20);
    }
}
catch (IndexOutOfRangeException)
{
    Console.WriteLine("IndexOutOfRangeException, Please Make Sure Your Game Is Running.");
}
catch (Exception e)
{
    Console.WriteLine("Main function Exception: " + e.Message);
}

