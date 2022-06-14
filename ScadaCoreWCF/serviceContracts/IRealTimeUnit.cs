using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.serviceContracts
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IRealTimeUnit
    {
        [OperationContract]
        bool AddressAvailable(string address);

        [OperationContract]
        bool RTUIdAvailable(string id);

        [OperationContract]
        string WriteRtuMessage(string message, byte[] signature);
        [OperationContract]
        string StopRTU(string id);

        [OperationContract(IsOneWay = true)]
        void LeavePublicKey(string path);



    }
}
