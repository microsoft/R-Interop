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
                            _engine = REngine.GetInstance();

                            _engine.Evaluate(string.Format(
                                CultureInfo.InvariantCulture,
                                @"install.packages(""{0}"", verbose = TRUE, dependencies = TRUE, type = ""win.binary"")",
                                Config.RPackagePath.Replace(@"\", @"\\")));

                            List<string> imports = _engine
                                .Evaluate(@"packageDescription(""" + Config.RPackageName + @""")$Imports")
                                .AsCharacter()
                                .First()
                                .Replace(" ", string.Empty)
                                .Split(',')
                                .ToList();

                            foreach (string packageName in imports)
                            {
                                _engine.Evaluate(string.Format(CultureInfo.InvariantCulture,
                                    @"if (!require(""{0}"")) 
install.packages(""{0}"", repo = ""https://cran.rstudio.com/"", dependencies = TRUE)",
                                    packageName));
                            }
                        }
                    }
                }

                return _engine;
            }
        }
    }
}
