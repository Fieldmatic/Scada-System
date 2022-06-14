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
    public class DigitalOutput : Tag
    {
        [DataMember]
        public double InitialValue { get; set; }
    }
}
