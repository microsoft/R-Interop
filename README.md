# R Interop
.NET managed abstraction layer for communicating with R

## Installation
#### [x86]: <https://github.com/Microsoft/R-Interop/releases/download/v1.0.1.0/RInteropSetup-x86.msi>
#### [x64]: <https://github.com/Microsoft/R-Interop/releases/download/v1.0.1.0/RInteropSetup-x64.msi>

## Command-line arguments
```sh
--schema Path to schema binary file containing types to serialize and deserialize input data and output data sent to and received from the R package, respectively
--s Same as -schema

--rpackage Path to R package file containing statistical functions (optional if packages are already installed)
--r Same as --rpackage
```

## Example
```sh
RInterop.exe --r C:\RPackages\MyStatsPackage_0.1.zip --s C:\RPackages\Schemas.dll
```

## Overview
R Interop starts a WCF service with named-pipe endpoints for inter process communication. Your application talks to R Interop through the named pipe. The following are the steps involved:
1. Client application sends request to R Interop as an Input object
2. R Interop serializes the input object and sends the request to R package loaded in R
3. R function evaluates the input object and returns the output
4. R Interop deserializes the output and returns to client application

## Named pipe endpoints
R Interop makes available 3 endpoints.

#### net.pipe://RInterop/
Metadata endpoint for generating a service reference

#### net.pipe://RInterop/Initialize
R Interop communicates with R through a simple JSON serialization/deserialization contract. The contract with R package expects an input type, an output type and the R function name, at minimum. There are two dictionaries - one for Input and one for Output - that enable this contract to be successfully created between the client application and R Interop. The key for the dictionary is the R function name. The value is the type provided in the class library passed as the --schema parameter. 

For example, use the following code to initialize the type mapping for the R function.
```sh
Dictionary<string, string> inputMap["DistributionTest"] = "Schemas.TTest.Input";
Dictionary<string, string> outputMap["DistributionTest"] = "Schemas.TTest.Output";

RClient client = new RClient("NetNamedPipeBinding_IR1");
client.Initialize(inputMap, outputMap);
client.Close();
```

Call the Initialize endpoint to pass the input and output type Dictionary mapping for each of your R functions.

#### net.pipe://RInterop/Execute
Executes the R function provided in the Input object with members describing the parameters. Returns the result as the Output type. The types are as described in the dictionaries initialized when calling the endpoint net.pipe://RInterop/Initialize
