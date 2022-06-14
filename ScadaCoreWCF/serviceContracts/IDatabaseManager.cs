using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ScadaCoreWCF.models;

namespace ScadaCoreWCF.serviceContracts
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IDatabaseManager
    {
        [OperationContract]
        bool Registration(string username, string password);
        [OperationContract]
        string Login(string username, string password);
        [OperationContract]
        bool Logout(string token);

        [OperationContract]
        bool TagNameAvailable(string Name);
        [OperationContract]
        bool AddTag(Tag tag);

        [OperationContract]
        bool RemoveTag(String name);
        [OperationContract(IsOneWay = true)]
        void LoadData();
        [OperationContract]
        string GetTagList();
        [OperationContract]
        void ChangeOTagValue(string name, double value);
        [OperationContract]
        string GetOutputTagList();

        [OperationContract]
        string GetInputTagList();

        [OperationContract(IsOneWay = true)]
        void ChangeScanning(String Name, bool Scanning);

        [OperationContract]
        Tag GetTagByName(string Name);
        [OperationContract]
        bool AddAlarm(Alarm alarm, string tagID);
        [OperationContract]
        string GetAlarmList();
        [OperationContract]
        string GetAnalogInputList();

        [OperationContract]
        bool AlarmNameAvailable(string Name);
        [OperationContract]
        bool RemoveAlarm(string alarmName);
    }
}
