using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace ScadaCoreWCF.models
{
   [DataContract]
   public class Alarm
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]

        public AlarmType Type { get; set; }
        [DataMember]
        public double Limit { get; set; }

        public Alarm()
        {

        }

        public Alarm (string Name, int Priority, AlarmType Type, double Limit)
        {
            this.Name = Name;
            this.Priority = Priority;
            this.Type = Type;
            this.Limit = Limit;
        }


    }

    public enum AlarmType
    {
        LOW,
        HIGH
    }
}


