using System.Reflection;

namespace Titled_Gui.Classes
{
    public abstract class ThreadService : IDisposable
    {
        public virtual string Name => nameof(ThreadService);

        public virtual Thread? Thread {  get; set; }
        protected ThreadService()
        {
            Thread = new Thread(ThreadStart)
            {
                Name = Name
            };
        }
        public void Dispose()
        {
            Thread?.Interrupt();
            Thread?.Join(5);
        }
        public void Start()
        {
            Thread?.Start();
        }
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
                Console.WriteLine("Null Refrence Exception: " + e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
        }

        protected abstract void FrameAction();

        public static void StartAllThreadServices()
        {
            var types = Assembly.GetExecutingAssembly()?.GetTypes()?.Where(t => t.IsSubclassOf(typeof(ThreadService)) && !t.IsAbstract);

            if (types != null)
            {
                foreach (Type type in types)
                {
                    if (type != null)
                    {
                        if (Activator.CreateInstance(type) is ThreadService service)
                        {
                            Thread serviceThread = new(service.Start)
                            {
                                IsBackground = true
                            };
                            serviceThread.Start();
                        }
                        else
                        {
                            throw new InvalidOperationException($"Failed To Create Service At {type.FullName}");
                        }
                    }
                }
            }
        }
    }
}
