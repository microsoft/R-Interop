using System.Collections.Generic;
using System.ServiceModel;

namespace RInterop
{
    [ServiceContract]
    [ServiceKnownType("GetKnownTypes", typeof(KnownTypesProvider))]
    public interface IR
    {
        [OperationContract]
        object Execute(dynamic input);

        [OperationContract]
        void Initialize(Dictionary<string, string> inputTypeMap, Dictionary<string, string> outputTypeMap);
    }
}
