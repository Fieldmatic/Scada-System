using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.serviceContracts
{
    [ServiceContract(CallbackContract = typeof(IAlarmDisplayCallback), SessionMode = SessionMode.Required)]
    public interface IAlarmDisplay
    {
        [OperationContract]
        string AlarmDisplayConnection();

    }

    public interface IAlarmDisplayCallback
    {
        [OperationContract(IsOneWay = true)]
        void DisplayAlarm(string message);
    }
}
