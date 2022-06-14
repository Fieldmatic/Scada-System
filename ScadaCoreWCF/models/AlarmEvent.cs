using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.models
{

    public class AlarmEvent
    {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    
        public int Priority { get; set; }

        public DateTime Time { get; set; }


        public AlarmType Type { get; set; }
      
        public double Limit { get; set; }

        public AlarmEvent()
        {

        }

        public AlarmEvent(Alarm alarm, DateTime Time)
        {
            this.Name = alarm.Name;
            this.Priority = alarm.Priority;
            this.Type = alarm.Type;
            this.Limit = alarm.Limit;
            this.Time = Time;
        }

        public override string ToString()
        {
            return $"Name:{Name} Priority:{Priority} Time:{Time} Type:{Type} Limit:{Limit}";
        }
    }
}
