# R Interop
.NET managed abstraction layer for communicating with R

## Installation
#### [x86]: <https://github.com/Microsoft/R-Interop/releases/download/v1.1.41577.1/RInteropSetup-x86.msi>
#### [x64]: <https://github.com/Microsoft/R-Interop/releases/download/v1.1.41577.1/RInteropSetup-x64.msi>

## Command-line arguments
```sh
--schema Path to schema binary file containing types to serialize and deserialize input data and output data sent to and received from the R package, respectively
--s Same as -schema

--rpackage Path to R package file containing statistical functions (optional if packages are already installed)
--r Same as --rpackage

--typemap Mapping from R function to input and output type schema.
--t Same as --typemap
```

## Example
```sh
RInterop.exe --r C:\RPackages\MyStatsPackage_0.1.zip --s C:\RPackages\Schemas.dll --t C:\RPackages\TypeMap.json
```

## Overview
R Interop starts a WCF service with named-pipe endpoints for inter process communication. Your application talks to R Interop through the named pipe. The following are the steps involved:
1. Client application sends request to R Interop as an Input object
2. R Interop serializes the input object and sends the request to R package loaded in R
3. R function evaluates the input object and returns the output
4. R Interop deserializes the output and returns to client application

## Named pipe endpoints
R Interop makes available 2 endpoints.

#### net.pipe://RInterop/
Metadata exchange (MEX) endpoint for generating a service reference.

#### net.pipe://RInterop/Execute
Executes the R function provided in the Input object with members describing the parameters. Returns the result as the Output type. The types are as described in the dictionaries initialized when calling the endpoint net.pipe://RInterop/Initialize
