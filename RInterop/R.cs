using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Logging;
using Newtonsoft.Json;
using RDotNet;

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

        private readonly bool _shouldLogCommand;
        private readonly bool _shouldLogResult;

        public R() : this(DependencyFactory.Resolve<ILogger>())
        {
            _engine = REngineWrapper.REngine;
        }

        public R(ILogger logger)
        {
            _logger = logger;
            _shouldLogCommand = Convert.ToBoolean(ConfigurationManager.AppSettings["LogCommand"]);
            _shouldLogResult = Convert.ToBoolean(ConfigurationManager.AppSettings["LogResult"]);
        }

        public object Execute(dynamic input)
        {
            // R.Net's REngine cannot perform parallel evaluation of R scripts
            lock (LockObject)
            {
                try
                {
                    Type inputType = Config.SerializationTypeMap.InputTypeMap[input.config.fn];
                    Type outputType = Config.SerializationTypeMap.OutputTypeMap[input.config.fn];

                    object deserializedInput = Convert.ChangeType(input, inputType);
                    var command = string.Format(CultureInfo.InvariantCulture,
                        @"{0}::run(""{1}"")",
                        Config.RPackageName,
                        JsonConvert.SerializeObject(deserializedInput).Replace("\"", "\\\""));
                    
                    if (_shouldLogCommand) _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Command: {0}", command));

                    var result = _engine.Evaluate(command)
                        .AsList()
                        .First()
                        .AsCharacter()
                        .First();

                    if (_shouldLogResult) _logger.LogInformation(string.Format(CultureInfo.InvariantCulture, "Result: {0}", result));

                    return JsonConvert.DeserializeObject(result, outputType);
                }
                catch (KeyNotFoundException e)
                {
                    string reason = string.Format(CultureInfo.InvariantCulture,
                        @"Could not find function ""{0}"" in the TypeMap.json configuration. Exception: {1}",
                        input.config.fn,
                        e);
                    _logger.LogError(reason);
                    throw new FaultException(new FaultReason(reason));
                }
                catch (Exception e)
                {
                    string reason = string.Format(CultureInfo.InvariantCulture,
                        "Evaluation failed. Exception: {0}",
                        e);
                    _logger.LogError(reason);
                    throw new FaultException(new FaultReason(reason));
                }
            }
        }
    }
}