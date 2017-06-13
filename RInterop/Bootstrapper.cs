using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Logging;
using Newtonsoft.Json;

namespace RInterop
{
    public class Bootstrapper : IBootstrapper
    {
        private readonly ILogger _logger = DependencyFactory.Resolve<ILogger>();

        public void Start(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Working directory: {0}",
                Environment.CurrentDirectory));
            _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Arguments: {0}", string.Join(" ", args)));

            var options = new CommandLineOptions();
            if (!CommandLineOptions.ParseArguments(args, options))
            {
                _logger.LogError(string.Format(CultureInfo.InvariantCulture, "Invalid arguments {0}",
                    string.Join(" ", args)));
                LogUsage();
                return;
            }

            Config.SchemaBinaryPath = options.SchemaBinaryPath;
            Assembly assembly = Assembly.LoadFrom(Config.SchemaBinaryPath);

            if (!string.IsNullOrEmpty(options.RPackagePath))
            {
                try
                {
                    var filename = Path.GetFileName(options.RPackagePath);
                    if (!string.IsNullOrEmpty(filename))
                        Config.RPackageName = filename.Substring(0, filename.IndexOf("_", StringComparison.Ordinal));
                }
                catch (Exception exception)
                {
                    _logger.LogError(string.Format(CultureInfo.InvariantCulture,
                        "Exiting. Exception while extracting R package name: {0}", exception));
                    LogUsage();
                    return;
                }
            }

            if (!string.IsNullOrEmpty(options.TypeMapJsonPath))
            {
                Config.SerializationTypeMap = new SerializationTypeMap();
                if (!string.IsNullOrEmpty(options.TypeMapJsonPath))
                {
                    TypeMap o =
                        JsonConvert.DeserializeObject<TypeMap>(new StreamReader(options.TypeMapJsonPath).ReadToEnd());

                    foreach (Map i in o.Mapping)
                    {
                        DependencyFactory.Resolve<ILogger>()
                            .LogInformation(string.Format(CultureInfo.InvariantCulture,
                                "Got input type mapping: {0} > {1} > {2}", i.Function, i.InputType, i.OutputType));
                        Config.SerializationTypeMap.InputTypeMap[i.Function] = assembly
                            .GetTypes()
                            .First(a => a.FullName.Equals(i.InputType));
                        Config.SerializationTypeMap.OutputTypeMap[i.Function] = assembly
                            .GetTypes()
                            .First(a => a.FullName.Equals(i.OutputType));
                    }
                }
            }

            using (var service = new Service())
            {
                service.Start();
                service.StartedEvent.WaitOne();
            }
        }

        private void LogUsage()
        {
            _logger.LogInformation(
                @"Usage: RInterop.exe --s ""<path to schema.dll>"" --r ""<path to R package file>""");
            _logger.LogInformation(
                @"Example: RInterop.exe --s ""C:\Temp\Schemas.dll"" --r ""C:\Temp\RPackage.zip"" --t ""C:\Temp\TypeMap.json""");
        }
    }
}