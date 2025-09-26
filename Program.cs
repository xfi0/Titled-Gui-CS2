using ImGuiNET;
using System.Numerics;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

try
{
    // initialization
    EntityManager entityManager = new EntityManager();
    await OffsetGetter.UpdateOffsetsAsync();

    ImGui.CreateContext();
    Renderer.LoadFonts();

    Thread renderThread = new(() => GameState.renderer.Start().Wait())
    {
        IsBackground = true
    };
    renderThread.Start();
    // entities
    List<Entity>? entities = new List<Entity>();
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

                    GameState.localPlayer = localPlayer;

                    GameState.renderer.UpdateLocalPlayer(localPlayer);
                }
                if (entities != null)
                {
                    GameState.renderer.UpdateEntities(entities);
                    GameState.Entities = new List<Titled_Gui.Data.Entity.Entity>(entities);

                    foreach (Entity entity in entities)
                    {
                        //GetGunName.UpdateEntityWeaponName(entity);
                        //Console.WriteLine(entity.Ping);
                    }
                }
                //Console.WriteLine(EntityManager.ReturnLocalPlayer()?.IsShooting);
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
catch (Exception e)
{
    Console.WriteLine("Exception: " + e.Message);
}

