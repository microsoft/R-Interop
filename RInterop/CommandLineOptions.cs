using System;
using System.Globalization;
using System.IO;
using CommandLine;
using Logging;

namespace RInterop
{
    public class CommandLineOptions
    {
        [Option('s', "schema", Required = true,
            HelpText =
                "Path to schema binary file containing types to serialize and deserialize input data and output data sent to and received from the R package, respectively"
            )]
        public string SchemaBinaryPath { get; set; }

        [Option('r', "rpackage", Required = false,
            HelpText =
                "Path to R package file containing statistical functions (optional if packages are already installed)")]
        public string RPackagePath { get; set; }

        [Option('t', "typemap", Required = false,
            HelpText = "Path to JSON file with type map (optional)")]
        public string TypeMapJsonPath { get; set; }

        // Omitting long name, default --verbose
        [Option(HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        public static bool ParseArguments(string[] args, CommandLineOptions options)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var logger = DependencyFactory.Resolve<ILogger>();
            if (!Parser.Default.ParseArguments(args, options))
            {
                return false;
            }

            if (!File.Exists(options.SchemaBinaryPath))
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Schema binary {0} not found",
                    options.SchemaBinaryPath));
                return false;
            }

            if (!string.IsNullOrEmpty(options.RPackagePath) && !File.Exists(options.RPackagePath))
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "R Package {0} not found",
                    options.RPackagePath));
                return false;
            }

            if (!string.IsNullOrEmpty(options.TypeMapJsonPath) && !File.Exists(options.TypeMapJsonPath))
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Type Map JSON file {0} not found",
                    options.TypeMapJsonPath));
                return false;
            }

            return true;
        }
    }
}