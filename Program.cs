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

Thread renderThread = new Thread(() => renderer.Start().Wait());
renderThread.IsBackground = true;
renderThread.Start();

// entities
List<Entity> entities = new List<Entity>();

// loop for mods and gettings sum stuff
while (true)
{
    entities = entityManager.GetEntities(); // get all entities
    Entity localPlayer = entityManager.GetLocalPlayer(); // get the local player
    GameState.localPlayer = localPlayer; 

    renderer.UpdateLocalPlayer(localPlayer); // update local player
    renderer.UpdateEntities(entities); //update entites
    GameState.Entities = new List<Entity>(entities);

    if (Titled_Gui.Modules.Rage.Aimbot.AimbotEnable)
    {
        Titled_Gui.Modules.Rage.Aimbot.EnableAimbot();
    }
    if (Titled_Gui.Modules.Visual.BombTimerOverlay.EnableTimeOverlay)
    {
        BombTimerOverlay.TimeOverlay(renderer);
    }
    GetGunNameFunction(localPlayer);
    Console.WriteLine($"sensitivity:{localPlayer.Sensitivity}DWSensitivity:{localPlayer.dwSensitivity}");
    Thread.Sleep(1); // you may want to adjust this if you dont got a great computer, works for me tho
}