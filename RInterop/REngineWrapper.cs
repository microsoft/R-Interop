using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
                            var logger = DependencyFactory.Resolve<ILogger>();

                            _engine = REngine.GetInstance();
                        }
                    }
                }

                return _engine;
            }
        }

        public static void InstallPackages(string rPackagePath, string schemaBinaryPath)
        {
            var engine = REngineWrapper.REngine;
            var logger = DependencyFactory.Resolve<ILogger>();
            engine.Evaluate(string.Format(
                CultureInfo.InvariantCulture,
                @"install.packages(""{0}"", verbose = TRUE, dependencies = TRUE, type = ""win.binary"")",
                rPackagePath.Replace(@"\", @"\\")));

            logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Installed R package {0}", rPackagePath));
            
            List<string> imports = engine
                .Evaluate(@"packageDescription(""" + Config.RPackageName + @""")$Imports")
                .AsCharacter()
                .First()
                .Replace(" ", string.Empty)
                .Split(',')
                .ToList();

            foreach (string packageName in imports)
            {
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Installing dependent R package {0}", packageName));
                engine.Evaluate(string.Format(CultureInfo.InvariantCulture,
                    @"if (!require(""{0}"")) 
install.packages(""{0}"", repo = ""https://cran.rstudio.com/"", dependencies = TRUE)",
                    packageName));
                logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Installed R package {0}", packageName));
            }
        }
    }
}
