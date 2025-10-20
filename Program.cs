using ImGuiNET;
using System.Numerics;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

try
{
    // initialization
    EntityManager entityManager = new();
    await OffsetGetter.UpdateOffsetsAsync();

    ImGui.CreateContext();
    Renderer.LoadFonts();
    Thread renderThread = new(() => GameState.renderer.Start().Wait())
    {
        IsBackground = true
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
                    entities = entityManager?.GetEntities();
                    Entity localPlayer = EntityManager.GetLocalPlayer();

                    GameState.LocalPlayer = localPlayer;

                    GameState.renderer.UpdateLocalPlayer(localPlayer);
                }
                if (entities != null)
                {
                    GameState.renderer.UpdateEntities(entities);
                    GameState.Entities = [.. entities];
                    foreach (Entity entity in entities)
                    {

                    }
                }
                //float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                //foreach (float vm in ViewMatrix)
                //{
                //    Console.WriteLine(vm);
                //}
                //Console.WriteLine(GameState.swed.ReadFloat(LocalPlayerPawn, Offsets.m_flFlashBangTime));
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
        Thread.Sleep(1);
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

