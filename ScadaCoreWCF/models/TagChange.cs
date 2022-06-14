using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.models
{
    public class TagChange
    {
        [Key]
        public int Id { get; set; }
        public string TagID { get; set; }

        public DateTime Time { get; set; }

        public double Value { get; set; }

        public TagChange() { }

        public TagChange(string tagID, DateTime time, double value)
        {
            TagID = tagID;
            Time = time;
            Value = value;
        }

        public override string ToString()
        {
            return $"Name:{TagID} scanned new value:{Value} on {Time}";
        }
    }
}
