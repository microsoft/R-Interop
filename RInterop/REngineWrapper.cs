using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Logging;
using RDotNet;

namespace RInterop
{
    public static class REngineWrapper
    {
        private static REngine _engine;

        private static readonly object LockObject = new object();

        public static REngine REngine
        {
            get
            {
                if (_engine == null)
                {
                    lock (LockObject)
                    {
                        if (_engine == null)
                        {
                            _engine = REngine.GetInstance();
                        }
                    }
                }

                return _engine;
            }
        }

        public static void InstallPackages(string rPackagePath)
        {
            var engine = REngine;
            var logger = DependencyFactory.Resolve<ILogger>();
            var userDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DEARPackages").Replace("\\", "/");
            if (!Directory.Exists(userDirectory)) Directory.CreateDirectory(userDirectory);
            engine.Evaluate($@".libPaths(""{userDirectory}"")");
            engine.Evaluate(@".libPaths()");
            engine.Evaluate(string.Format(CultureInfo.InvariantCulture,
                @"install.packages(""{0}"", verbose = TRUE, dependencies = TRUE, type = ""win.binary"")",
                rPackagePath.Replace(@"\", @"\\")));

            logger.LogInformation(string.Format(CultureInfo.InvariantCulture, @"Installed R package ""{0}""", rPackagePath));

            List<string> imports;
            try
            {
                imports = engine
                    .Evaluate(string.Format(CultureInfo.InvariantCulture,
                        @"packageDescription(""{0}"")$Imports",
                        Config.RPackageName))
                    .AsCharacter()
                    .FirstOrDefault()?
                    .Replace(" ", string.Empty)
                    .Split(',')
                    .ToList();
            }
            catch (Exception e)
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture,
                    @"Could not get the dependent packages from provided package. Please manually install package using the ""install.packages"" command. Exception: {0}",
                    e));
                throw;
            }

            if (imports == null) return;

            foreach (string packageName in imports)
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, 
                    @"Installing dependent R package ""{0}""",
                    packageName));

                try
                {
                    engine.Evaluate(string.Format(CultureInfo.InvariantCulture,
                        @"if (!require(""{0}"")) 
install.packages(""{0}"", repo = ""https://cran.rstudio.com/"", dependencies = TRUE)",
                        packageName));
                    engine.Evaluate(string.Format(CultureInfo.InvariantCulture,
                        @"if (!require(""{0}"")) stop(""Package {0} did not install correctly."")",
                        packageName));
                }
                catch (Exception e)
                {
                    logger.LogInformation(string.Format(CultureInfo.InvariantCulture,
                        @"Could not automatically install dependent package ""{0}"". Please manually install R package using the following command: install.packages(""{0}"")
Exception: {1}", 
                        packageName,
                        e));
                    throw;
                }

                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, @"Installed dependent R package ""{0}""", packageName));
            }
        }
    }
}