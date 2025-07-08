using Titled_Gui;
using Titled_Gui.Data;
using Swed64;
using System.Numerics;
using Titled_Gui.Modules.Visual;
using System.Threading;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.ModuleHelpers;
using static Titled_Gui.ModuleHelpers.GetGunName;

// initialization
Swed swed = new Swed("cs2");
Renderer renderer = new Renderer();
EntityManager entityManager = new EntityManager(swed, renderer);
await OffsetGetter.UpdateOffsetsAsync();

Thread renderThread = new Thread(() => renderer.Start().Wait());
renderThread.IsBackground = true;
renderThread.Start();

// entities
List<Entity> entities = new List<Entity>();

// entity thread
Thread entityUpdateThread = new Thread(() =>
{
    var timer = System.Diagnostics.Stopwatch.StartNew();

    while (true)
    {
        long frameStart = timer.ElapsedMilliseconds;

        entities = entityManager.GetEntities();
        Entity localPlayer = entityManager.GetLocalPlayer();
        GameState.localPlayer = localPlayer;
        renderer.UpdateLocalPlayer(localPlayer);
        renderer.UpdateEntities(entities);
        GameState.Entities = new List<Entity>(entities);
        GetGunNameFunction(localPlayer);

        long frameTime = timer.ElapsedMilliseconds - frameStart;
        if (frameTime < 1)
        {
            Thread.SpinWait(50);
        }
    }
});
entityUpdateThread.IsBackground = true;
entityUpdateThread.Priority = ThreadPriority.Highest;
entityUpdateThread.Start();

while (true)
{
    Thread.Sleep(1);
}