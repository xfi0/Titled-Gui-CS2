using ImGuiNET;
using Swed64;
using System.Diagnostics;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;
using Types = Titled_Gui.Data.Menu.Types;

try
{
    // initialization
    await OffsetGetter.UpdateOffsetsAsync();

    EntityManager entityManager = new();
    GameState.renderer = new();
    ImGui.CreateContext();
    Renderer.LoadFonts();
    await GameState.renderer.Start();

    //foreach (string name in names) // liusts all embeddded resources
    //{
    //Console.WriteLine(name);
    //}

    // entities
    List<Entity>? entities = [];
    if (Process.GetProcessesByName("cs2").Length == 0)
    {
        Console.WriteLine("CS2 Not Found...");
        Thread.Sleep(1000);
    }

    GameState.swed = new("cs2");
    GameState.client = GameState.swed.GetModuleBase("client.dll");

    while (GameState.swed != null && !OffsetGetter.Updated)
    {
        await OffsetGetter.CheckIfOffsetsAreValid();
        Thread.Sleep(10);
        continue;
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
        Thread.Sleep(20);
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

