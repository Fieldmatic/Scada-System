using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportManager.ScadaCoreReportManagerReference;

namespace ReportManager
{
    
    class Program
    {
        public static ReportManagerClient proxy = new ReportManagerClient();
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Select report :");
                Console.WriteLine("1-Alarms in period");
                Console.WriteLine("2-Alarms with selected priority");
                Console.WriteLine("3-Tag value changes in period");
                Console.WriteLine("4-Last Analog input values");
                Console.WriteLine("5-Last Digital Input values");
                Console.WriteLine("6-All values of tag with selected id");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":AlarmsInPeriod();
                             break;
                    case "2": AlarmsWithSelectedPriority();
                              break;
                    case "3":TagValueChangesInPeriod();
                              break;
                    case "4": LastAnalogInputValues();
                              break;
                    case "5": LastDigitalInputValues();
                              break;
                    case "6": ValuesOfSelectedID();
                              break;
                    default: Console.WriteLine("Report doesn't exist");
                             continue;
                }

            }
        }

        private static void ValuesOfSelectedID()
        {
            string ID;
            Console.Write("Tag name: ");
            ID = Console.ReadLine();
            Console.Clear();
            Console.WriteLine(proxy.ReportValuesOfSelectedID(ID));
        }

        private static void LastDigitalInputValues()
        {
            Console.Clear();
            Console.WriteLine(proxy.ReportLastDigitalInputValues());
        }

        private static void LastAnalogInputValues()
        {
            Console.Clear();
            Console.WriteLine(proxy.ReportLastAnalogInputValues());
        }

        private static void TagValueChangesInPeriod()
        {
            DateTime start;
            DateTime end;
            while (true)
            {
                Console.Write("Enter starting date (DD.MM.YYYY. HH:MM): ");
                if (DateTime.TryParse(Console.ReadLine(), out start))
                {
                    Console.Write("Enter ending date (DD.MM.YYYY. HH:MM): ");
                    if (DateTime.TryParse(Console.ReadLine(), out end) && end > start)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect format !");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect format !");
                    continue;
                }

               
            }
            Console.Clear();
            Console.WriteLine(proxy.ReportTagValueChangesInPeriod(start, end));
        }

        private static void AlarmsWithSelectedPriority()
        {
            int priority;
            while (true)
            {
                Console.Write("Priority(1 or 2 or 3): ");
                if (int.TryParse(Console.ReadLine(), out priority))
                {
                    break;

                }
                else
                {
                    Console.WriteLine("Priority must be 1 or 2 or 3 !");
                    continue;
                }
            }
            Console.Clear();
            Console.WriteLine(proxy.ReportAlarmsWithSelectedPriority(priority));
        }

        private static void AlarmsInPeriod()
        {
            DateTime start;
            DateTime end;
            while (true)
            {
                Console.Write("Enter starting date (DD.MM.YYYY. HH:MM): ");
                if (DateTime.TryParse(Console.ReadLine(), out start))
                {
                    Console.Write("Enter ending date (DD.MM.YYYY. HH:MM): ");
                    if (DateTime.TryParse(Console.ReadLine(), out end) && end > start)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect format !");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect format !");
                    continue;
                }             
            }
            Console.Clear();
            Console.WriteLine(proxy.ReportAlarmsInPeriod(start, end));
        }
    }
}
