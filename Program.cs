using Swed64;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using static Titled_Gui.Data.Game.GameState;

// initialization
EntityManager entityManager = new EntityManager(GameState.swed,GameState.renderer);
await OffsetGetter.UpdateOffsetsAsync();

Thread renderThread = new(() => GameState.renderer.Start().Wait())
{
    IsBackground = true
};
renderThread.Start();
// entities
List<Entity> entities = new List<Entity>();

Thread entityUpdateThread = new(() =>
{
    while (true)
    {
        try
        {
            if (!Directory.Exists(Configs.ConfigDirPath))
            {
                Directory.CreateDirectory(Configs.ConfigDirPath);
            }
            var files = Directory.EnumerateFiles(Configs.ConfigDirPath).Select(Path.GetFileName).Where(f => f != null).ToHashSet(); // refresh so if any thing changes the dic updates
            foreach (var file in files)
            {
                Configs.SavedConfigs.TryAdd(file!, true);
            }
            foreach (var key in Configs.SavedConfigs.Keys)
            {
                if (!files.Contains(key))
                {
                    Configs.SavedConfigs.TryRemove(key, out _);
                }
            }
            entities = entityManager.GetEntities();
            Entity localPlayer = entityManager.GetLocalPlayer();
            GameState.localPlayer = localPlayer;
            GameState.renderer.UpdateLocalPlayer(localPlayer);
            GameState.renderer.UpdateEntities(entities);
            GameState.Entities = new List<Titled_Gui.Data.Entity.Entity>(entities);
            foreach (Entity entity in entities)
            {
                WeaponReader.UpdateEntityWeaponName(entity);
            }

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

