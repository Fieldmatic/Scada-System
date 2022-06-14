using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AlarmDisplay.ScadaCoreAlarmDisplayReference;

namespace AlarmDisplay
{
    class Program
    {
        class AlarmDisplayCallback : IAlarmDisplayCallback
        {
            public void DisplayAlarm(string message)
            {
                Console.WriteLine(message);
            }

           
        }
        static void Main(string[] args)
        {
            AlarmDisplayClient proxy = new AlarmDisplayClient(new InstanceContext(new AlarmDisplayCallback()));
            Console.WriteLine(proxy.AlarmDisplayConnection());
            Console.ReadKey();
        }
    }
}
