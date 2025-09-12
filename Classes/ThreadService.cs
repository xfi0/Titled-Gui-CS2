using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Titled_Gui.Classes
{
    public abstract class ThreadService : IDisposable // testing stuff idk if ts works
    {
        public virtual string Name => nameof(ThreadService);

        public virtual Thread? thread {  get; set; }
        protected ThreadService()
        {
            thread = new Thread(ThreadStart)
            {
                Name = Name
            };
        }
        public void Dispose()
        {
            thread?.Interrupt();
            thread?.Join(5);
        }
        public void Start()
        {
            thread?.Start();
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
                Console.WriteLine("Null Refrence Exception" + e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception" + e);
            }
        }
        protected abstract void FrameAction();
        public static void StartAllThreadServices()
        {
            var threadServiceTypes = Assembly.GetExecutingAssembly()?.GetTypes()?.Where(t => t.IsSubclassOf(typeof(ThreadService)) && !t.IsAbstract);
            if (threadServiceTypes != null)
            {
                foreach (var type in threadServiceTypes)
                {
                    if (type != null)
                    {
                        var service = (ThreadService)Activator.CreateInstance(type);

                        Thread serviceThread = new Thread(service.Start)
                        {
                            IsBackground = true
                        };
                        serviceThread.Start();
                    }
                }
            }
        }
    }
}
