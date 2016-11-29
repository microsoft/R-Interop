using Newtonsoft.Json;
using RDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace RInterop
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple, 
        IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.PerCall)]
    public class R : IR
    {
        private static readonly object LockObject = new object();

        private readonly REngine _engine;

        private readonly ILogger _logger;

        public R() : this(DependencyFactory.Resolve<ILogger>())
        {
            _engine = REngineWrapper.REngine;
        }
        
        public R(ILogger logger)
        {
            _logger = logger;
        }

        public object Execute(dynamic input)
        {
            // R.Net's REngine cannot perform parallel evaluation of R scripts
            lock(LockObject)
            {
                try
                {
                    Type inputType = Config.SerializationTypeMaps.InputTypeMap[input.config.fn];
                    Type outputType = Config.SerializationTypeMaps.OutputTypeMap[input.config.fn];

                    object deserializedInput = Convert.ChangeType(input, inputType);
                    var command = string.Format(CultureInfo.InvariantCulture,
                        "{0}::run(\"{1}\")",
                        Config.RPackageName,
                        JsonConvert.SerializeObject(deserializedInput).Replace("\"", "\\\""));

                    _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Evaluating: {0}", command));

                    var result = _engine
                        .Evaluate(command)
                        .AsList()
                        .First()
                        .AsCharacter()
                        .First();

                    _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Result: {0}", result));

                    return JsonConvert.DeserializeObject(result, outputType);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Exception during evaluation {0}", e));
                    return null;
                }
            }
        }

        public void Initialize(Dictionary<string, string> inputTypeMap, Dictionary<string, string> outputTypeMap)
        {
            Assembly assembly = Assembly.LoadFrom(Config.SchemaBinaryPath);
            Config.SerializationTypeMaps = new SerializationTypeMaps();
            foreach (string key in inputTypeMap.Keys)
            {
                Config.SerializationTypeMaps.InputTypeMap[key] = assembly
                    .GetTypes()
                    .First(a => a.FullName.Equals(inputTypeMap[key]));
                _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Got input type mapping: {0} > {1}", key, inputTypeMap[key]));
            }

            foreach (string key in outputTypeMap.Keys)
            {
                Config.SerializationTypeMaps.OutputTypeMap[key] = assembly
                    .GetTypes()
                    .First(a => a.FullName.Equals(outputTypeMap[key]));
                _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Got output type mapping: {0} > {1}", key, outputTypeMap[key]));
            }
        }
    }
}
