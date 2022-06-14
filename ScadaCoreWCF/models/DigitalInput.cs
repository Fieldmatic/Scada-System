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
    public class DigitalInput : Tag
    {
        [DataMember]
        public int ScanTime { get; set; }

        [DataMember]
        public bool ScanOn { get; set; }

        [DataMember]
        public DriverType Driver { get; set; }

        public DigitalInput() { }

    }
}
