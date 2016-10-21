using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;

namespace RInterop
{
    public class DependencyFactory
    {
        public static IUnityContainer Container { get; private set; }

        static DependencyFactory()
        {
            var container = new UnityContainer();

            var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            section?.Configure(container);
            Container = container;

            Container.RegisterType<ILoggerFactory>("TraceLoggerFactory",
                new InjectionFactory(c => new TraceLoggerFactory()));
            Container.RegisterInstance<ILogger>(
                Container
                    .Resolve<ILoggerFactory>("TraceLoggerFactory")
                    .Create("RInterop"));

            Resolve<ILogger>().LogInformation("Type registrations completed");
        }

        public static T Resolve<T>()
        {
            T ret = default(T);

            if (Container.IsRegistered(typeof(T)))
            {
                ret = Container.Resolve<T>();
            }

            return ret;
        }
    }
}
