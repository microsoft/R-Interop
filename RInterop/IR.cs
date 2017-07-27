using System.ServiceModel;

namespace RInterop
{
    [ServiceContract]
    [ServiceKnownType("GetKnownTypes", typeof (KnownTypesProvider))]
    public interface IR
    {
        [OperationContract]
        object Execute(dynamic input);

        [OperationContract]
        bool InstallPackage(string packagePath);

        [OperationContract]
        bool RemovePackage(string packageName);
    }
}