using CommandLine;
using System.Globalization;
using System.IO;

namespace RInterop
{
    public class CommandLineOptions
    {
        [Option('s', "schema", Required = true,
            HelpText = "Path to schema binary file containing types to serialize and deserialize input data and output data sent to and received from the R package, respectively")]
        public string SchemaBinaryPath { get; set; }

        [Option('r', "rpackage", Required = true,
            HelpText = "Path to R package file containing statistical functions (optional if packages are already installed)")]
        public string RPackagePath { get; set; }

        // Omitting long name, default --verbose
        [Option(HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        public static bool ParseArguments(string[] args, CommandLineOptions options)
        {
            var logger = DependencyFactory.Resolve<ILogger>();
            if (!Parser.Default.ParseArguments(args, options))
            {
                return false;
            }
            
            if (!File.Exists(options.RPackagePath))
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "RPackage {0} not found", options.RPackagePath));
                return false;
            }

            if (!File.Exists(options.SchemaBinaryPath))
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Schema binary {0} not found", options.SchemaBinaryPath));
                return false;
            }

            return true;
        }
    }
}
