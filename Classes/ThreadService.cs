using System.Reflection;

namespace Titled_Gui.Classes
{
    public abstract class ThreadService : IDisposable
    {
        public virtual string Name => nameof(ThreadService);
        public virtual ThreadPriority ThreadPriority => ThreadPriority.Normal;
        public virtual bool IsBackground => false;
        public virtual Thread? Thread { get; set; }
        protected ThreadService()
        {
            Thread = new Thread(ThreadStart)
            {
                Name = Name,
                Priority = ThreadPriority,
                IsBackground = IsBackground,
            };
        }
        public void Dispose()
        {
            Thread?.Interrupt();
            Thread?.Join(5);
            GC.SuppressFinalize(this);
        }
        public void Start() => Thread?.Start();

        public void ThreadStart()
        {
            try
            {
                while (true)
                {
                    FrameAction();
                    Thread.Sleep(1);
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Null reference exception: " + e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
        }

        protected abstract void FrameAction();

        public static void StartAllThreadServices()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ThreadService)) && !t.IsAbstract);

            foreach (Type type in types)
            {
                if (Activator.CreateInstance(type) is ThreadService service)
                {
                    service.Start();
                }
                else
                {
                    throw new InvalidOperationException($"Failed To Create Service At {type.FullName}");
                }
            }
        }
    }
}
