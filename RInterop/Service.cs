using System;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using Logging;

namespace RInterop
{
    public class Service : IDisposable
    {
        private readonly EventWaitHandle _startedEvent;

        public Service()
        {
            _startedEvent = new EventWaitHandle(false, EventResetMode.ManualReset,
                @"Global\RInteropStarted");
        }

        public EventWaitHandle StartedEvent => _startedEvent;

        public void Start()
        {
            try
            {
                using (ServiceHost host = new ServiceHost(typeof (R)))
                {
                    host.Closed += HostOnClosed;
                    host.Faulted += HostOnFaulted;
                    host.Opened += HostOnOpened;

                    host.Open();
                    Console.ReadLine();
                    host.Close();
                }
            }
            catch (Exception e)
            {
                DependencyFactory.Resolve<ILogger>().LogInformation(string.Format(CultureInfo.InvariantCulture,
                    "RInterop hit an unexpected exception. Exception: {0}", e));
            }
        }

        private void HostOnClosed(object sender, EventArgs eventArgs)
        {
            DependencyFactory.Resolve<ILogger>().LogInformation("RInterop has shutdown.");
        }

        private void HostOnFaulted(object sender, EventArgs eventArgs)
        {
            DependencyFactory.Resolve<ILogger>().LogInformation("RInterop is in faulted state.");
        }

        private void HostOnOpened(object sender, EventArgs eventArgs)
        {
            DependencyFactory.Resolve<ILogger>().LogInformation("Service is available. Press <ENTER> to exit.");
            DependencyFactory.Resolve<ILogger>().LogInformation(string.Format(CultureInfo.InvariantCulture, @"R Path : ""{0}""", new RDotNet.NativeLibrary.NativeUtility().FindRPath()));
            _startedEvent.Set();
        }

        public void Dispose()
        {
            _startedEvent.Dispose();
        }
    }
}