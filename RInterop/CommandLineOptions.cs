using CommandLine;
using System.IO;

namespace RInterop
{
    public class CommandLineOptions
    {
        [Option('s', "schema", Required = false,
            HelpText = "Path to schema binary file containing types to serialize and deserialize input data and output data sent to and received from the R package, respectively")]
        public string SchemaBinaryPath { get; set; }

        [Option('r', "rpackage", Required = false,
            HelpText = "Path to R package file containing statistical functions (optional if packages are already installed)")]
        public string RPackagePath { get; set; }

        // Omitting long name, default --verbose
        [Option(HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        public static bool ParseArguments(string[] args, CommandLineOptions options)
        {
            if (!Parser.Default.ParseArguments(args, options))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(options.RPackagePath) && !File.Exists(options.RPackagePath))
            {
                DependencyFactory.Resolve<ILogger>().LogInformation("RPackagePath {0} not found", options.RPackagePath);
                return false;
            }

            if (!string.IsNullOrEmpty(options.SchemaBinaryPath) && !File.Exists(options.SchemaBinaryPath))
            {
                DependencyFactory.Resolve<ILogger>().LogInformation("SchemaBinaryPath {0} not found", options.SchemaBinaryPath);
                return false;
            }

            return true;
        }
    }
}
