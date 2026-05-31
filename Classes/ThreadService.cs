using System.Reflection;

namespace Titled_Gui.Classes
{
    public abstract class ThreadService : IDisposable
    {
        public virtual string name => nameof(ThreadService);
        public virtual ThreadPriority threadPriority => ThreadPriority.Normal;
        public virtual bool IsBackground => false;
        public virtual Thread? thread {  get; set; }
        protected ThreadService()
        {
            thread = new Thread(ThreadStart)
            {
                Name = name,
                Priority = threadPriority,
                IsBackground = IsBackground,
            };
        }
        public void Dispose()
        {
            thread?.Interrupt();
            thread?.Join(5);
            GC.SuppressFinalize(this);
        }
        public void Start() => thread?.Start();
        
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
