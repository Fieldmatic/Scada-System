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
    [KnownType(typeof(AnalogInput))]
    [KnownType(typeof(AnalogOutput))]
    [KnownType(typeof(DigitalInput))]
    [KnownType(typeof(DigitalOutput))]
    public class Tag
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string IO_Address { get; set; }

        [DataMember]
        public double Value { get; set; }

        public Tag() { }


    }

    public enum DriverType
    {
        SimDriver,
        RealTimeDriver
    }
}
