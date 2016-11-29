using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace RInterop
{
    class Program
    {
        public static void Main(string[] args)
        {
            ILogger logger = DependencyFactory.Resolve<ILogger>();

            logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Working directory: {0}", Environment.CurrentDirectory));
            logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Arguments: {0}", string.Join(" ", args)));

            var options = new CommandLineOptions();
            if (!CommandLineOptions.ParseArguments(args, options))
            {
                DependencyFactory.Resolve<ILogger>().LogInformation(string.Format(CultureInfo.InvariantCulture, "Invalid arguments {0}", string.Join(" ", args)));

                logger.LogInformation(@"Usage: RInterop.exe --s ""<path to schema.dll>"" --r ""<path to R package file>""");
                logger.LogInformation(@"Example: RInterop.exe --s ""C:\Temp\Schemas.dll"" --r ""C:\Temp\RPackage.zip""");
                logger.LogInformation("Press any key to continue...");
                Console.ReadLine();
                return;
            }

            try
            {
                var filename = Path.GetFileName(options.RPackagePath);
                Config.RPackageName = filename.Substring(0, filename.IndexOf("_", StringComparison.Ordinal));
            }
            catch (Exception exception)
            {
                logger.LogInformation("Exception while extracting R package name: {0}", exception);
                return;
            }
            
            Config.SchemaBinaryPath = options.SchemaBinaryPath;
            REngineWrapper.InstallPackages(options.RPackagePath, options.SchemaBinaryPath);

            StartService();
        }
        
        private static void StartService()
        {
            EventWaitHandle startedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, @"Global\RInteropStarted");

            using (ServiceHost host = new ServiceHost(typeof(R), new Uri("net.pipe://RInterop")))
            {
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                binding.MaxReceivedMessageSize = Int32.MaxValue;
                binding.MaxBufferSize = Int32.MaxValue;

                host.AddServiceEndpoint(typeof(IR),
                    binding,
                    "Execute");

                host.AddServiceEndpoint(typeof(IR),
                    binding,
                    "Initialize");

                // Check to see if the service host already has a ServiceMetadataBehavior
                ServiceMetadataBehavior smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                // If not, add one
                if (smb == null)
                    smb = new ServiceMetadataBehavior();

                smb.HttpGetEnabled = false;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                // Add MEX endpoint
                host.AddServiceEndpoint(
                    ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexNamedPipeBinding(),
                    "mex");

                // Add application endpoint
                host.AddServiceEndpoint(typeof(IR), new NetNamedPipeBinding(), "");

                host.Open();
                
                startedEvent.Set();

                DependencyFactory.Resolve<ILogger>().LogInformation("Service is available. Press <ENTER> to exit.");
                Console.ReadLine();

                host.Close();
            }
        }
    }
}
