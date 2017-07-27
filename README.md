# R Interop
.NET managed abstraction layer for communicating with R

## Installation
Please note that there are breaking changes in terms of how RInterop works as well as command line arguments it takes in to start up. 
#### <https://github.com/Microsoft/R-Interop/releases/download/v3.0.51061.5/RInteropSetup.msi>

## Command-line arguments
```
--schema Path to schema binary file containing types to serialize and deserialize input data and output data sent to and received from the R package, respectively
--s Same as -schema

--rpackage Path to R package file containing statistical functions (optional if packages are already installed)
--r Same as --rpackage

--typemap Mapping from R function to input and output type schema.
--t Same as --typemap
```

## Example
```
RInterop.exe --r C:\RPackages\MyStatsPackage_0.1.zip --s C:\RPackages\Schemas.dll --t C:\RPackages\TypeMap.json
```

## Overview
R Interop starts a WCF service with named-pipe endpoints for inter process communication. Your application talks to R Interop through the named pipe. The following are the steps involved:
1. Client application sends request to R Interop as an Input object.
2. R Interop serializes the input object and sends the request to R package loaded in R.
3. R function evaluates the input object and returns the output.
4. R Interop deserializes the output and returns it to client application.

Note that both the client application and RInterop utilize the same Schemas library so that they can understand each other.

## Named pipe endpoints
R Interop makes available 2 endpoints.

#### net.pipe://RInterop/
Metadata exchange (MEX) endpoint for generating a service reference within the client or caller application.

#### net.pipe://RInterop/R
Executes the R function provided in the Input object with members describing the parameters. Returns the result as the corresponding Output type. The types are as described in the dictionaries provided in TypeMap.json that map to the types in the Schemas assembly.

There are 3 Operations that can be performed on this API - Execute, InstallPackage, and RemovePackage. When a Service Reference is created, these operations will be available as methods on the Client object.

The format of TypeMap.json is as follows:

```
{
  "Mapping": [
    {
      "Function": "TTest",
      "InputType": "Schemas.TTest.Input",
      "OutputType": "Schemas.TTest.Output"
    },
    {
      "Function": "ChiSquareTest",
      "InputType": "Schemas.ChiSquareTest.Input",
      "OutputType": "Schemas.ChiSquareTest.Output"
    }
  ]
}
```

## Windows events for developers
When RInterop starts up successfully, it will fire an event named ```Global\RInteropStarted```.
When RInterop is unable to start up successfully, it will fire an event named ```Global\RInteropStartupError``` and exit. The logs located in the ```%temp%\RInterop``` folder will help diagnose and/or resolve issues. Feel free to post questions on Github.
