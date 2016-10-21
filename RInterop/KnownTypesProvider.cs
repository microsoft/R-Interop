using System;
using System.Collections.Generic;
using System.Reflection;

namespace RInterop
{
    public static class KnownTypesProvider
    {
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider = null)
        {
            return Assembly
                .LoadFrom(Config.SchemaBinaryPath)
                .GetExportedTypes();
        }
    }
}
