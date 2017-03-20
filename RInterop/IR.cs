using System.ServiceModel;

namespace RInterop
{
    [ServiceContract]
    [ServiceKnownType("GetKnownTypes", typeof (KnownTypesProvider))]
    public interface IR
    {
        [OperationContract]
        object Execute(dynamic input);
    }
}