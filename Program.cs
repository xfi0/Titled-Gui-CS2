using Swed64;
using System.Numerics;
using System.Reflection;
using System.Threading;
using Titled_Gui;
using Titled_Gui.Classes;
using Titled_Gui.Data;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using static Titled_Gui.Classes.GetGunName;

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
            foreach (var file in Directory.GetFiles(Configs.ConfigDirPath))
            {
                if (!Configs.SavedConfigs.Contains(file))
                {
                    Configs.SavedConfigs.Add(file.Replace(Configs.ConfigDirPath, ""));
                }
            }
            entities = entityManager.GetEntities();
            Entity localPlayer = entityManager.GetLocalPlayer();
            GameState.localPlayer = localPlayer;
            GameState.renderer.UpdateLocalPlayer(localPlayer);
            GameState.renderer.UpdateEntities(entities);
            GameState.Entities = new List<Entity>(entities);
            foreach (Entity entity in entities)
            {
                //Console.WriteLine(GetAllGuns(entity));
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

