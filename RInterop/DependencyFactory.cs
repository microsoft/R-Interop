using Logging;
using Microsoft.Practices.Unity;

namespace RInterop
{
    public class DependencyFactory
    {
        public static void Initialize()
        {
            var container = new UnityContainer();
            Container = container;
            Container.RegisterInstance<ILogger>(new TraceLogger("%temp%", "RInterop"));
            var logger = Resolve<ILogger>();
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;
            logger.LogInformation(string.Format("Starting RInterop {0}", version != null ? version.Version : "N/A"));
            logger.LogInformation(string.Format("Install Location {0}", System.AppDomain.CurrentDomain.BaseDirectory));
            logger.LogInformation("Completed registering dependencies");
        }

        public static IUnityContainer Container { get; private set; }

        public static T Resolve<T>()
        {
            T ret = default(T);

            if (Container.IsRegistered(typeof (T)))
            {
                ret = Container.Resolve<T>();
            }

            return ret;
        }
    }
}