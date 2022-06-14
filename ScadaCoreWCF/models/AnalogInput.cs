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
    public class AnalogInput : DigitalInput
    {

        [DataMember]
        public List<Alarm> Alarms { get; set; }

        [DataMember]
        public double LowLimit { get; set; }
        [DataMember]
        public double HighLimit { get; set; }
        [DataMember]
        public string Units { get; set; }

    }
}
