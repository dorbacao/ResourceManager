using CoreWCF;
using System;
using System.Runtime.Serialization;

namespace MyService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]        
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
    }
}
