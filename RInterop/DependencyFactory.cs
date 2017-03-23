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
            Resolve<ILogger>().LogInformation("Completed registering dependencies");
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