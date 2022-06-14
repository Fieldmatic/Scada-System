using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.serviceContracts
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    interface IReportManager
    {
        [OperationContract]
        string ReportAlarmsInPeriod(DateTime start, DateTime end);
        [OperationContract]
        string ReportAlarmsWithSelectedPriority(int priority);
        [OperationContract]
        string ReportTagValueChangesInPeriod(DateTime start, DateTime end);
        [OperationContract]
        string ReportLastAnalogInputValues();
        [OperationContract]
        string ReportLastDigitalInputValues();
        [OperationContract]
        string ReportValuesOfSelectedID(string ID);
    }
}
