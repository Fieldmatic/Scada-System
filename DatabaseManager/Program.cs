using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseManager.ScadaCoreReference;
using ScadaCoreWCF.models;

namespace DatabaseManager
{
    class Program
    {
        public static DatabaseManagerClient proxy = new DatabaseManagerClient();
        static void Main(string[] args)
        {
            proxy.LoadData();
            bool running = true;
            while (running)
            {
                Console.WriteLine("Enter username:");
                string username = Console.ReadLine();
                Console.WriteLine("Enter password:");
                string password = Console.ReadLine();

                string token = proxy.Login(username, password);

                if (token != "Login failed")
                {
                    Console.Clear();
                    Console.WriteLine($"Welcome {username} \n");
                    bool loggedIn = true;
                    while (loggedIn)
                    {
                        MainMenu();
                        string option = Console.ReadLine();
                        switch (option)
                        {
                            case "1":
                                Registration();
                                break;
                            case "2":
                                AddTag();
                                break;
                            case "3":
                                RemoveTag();
                                break;
                            case "4":
                                ChangeOutputValue();
                                break;
                            case "5":
                                ShowOutputValues();
                                break;
                            case "6":
                                TurnScanOnOff();
                                break;
                            case "7":
                                AddRemoveAlarm();
                                break;
                            case "9":
                                proxy.Logout(token);
                                loggedIn = false;
                                break;
                            case "10":
                                loggedIn = false;
                                running = false;
                                break;
                            default:
                                Console.Clear();
                                Console.WriteLine("Wrong option!");
                                continue;
                        }
                    }

                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Wrong credentials!");
                }
            }
        }

        private static void AddRemoveAlarm()
        {
            while (true)
            {
                Console.WriteLine("1- Add new alarm");
                Console.WriteLine("2- Remove alarm");
                Console.WriteLine("3- Return");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        AddNewAlarm();
                        break;
                    case "2":
                        RemoveAlarm();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Wrong option");
                        continue;
                }
            }

        }

        private static void AddNewAlarm()
        {

            string TagName = GetAlarmTagName();
            string AlarmName = GetAlarmName();
            int Priority = GetAlarmPriority();
            AlarmType type = GetAlarmType();
            double Limit = GetLimit();
            Alarm a = new Alarm();
            a.Name = AlarmName;
            a.Priority = Priority;
            a.Type = type;
            a.Limit = Limit;
            if (proxy.AddAlarm(a, TagName)) Console.WriteLine("Alarm Successfully added");



        }

        private static double GetLimit()
        {
            string limitString;
            double Limit;
            while (true)
            {
                Console.WriteLine("Limit: ");
                limitString = Console.ReadLine();
                if (double.TryParse(limitString, out Limit))
                {
                    return Limit;
                }
                else
                {
                    Console.WriteLine("Not an double value !");
                    continue;
                }
                

            }
        }

        private static AlarmType GetAlarmType()
        {
            string type;
            while (true)
            {           
                Console.WriteLine("1- LOW");
                Console.WriteLine("2- HIGH");
                Console.WriteLine("Choose AlarmType: ");
                type = Console.ReadLine();
                if (type == "1")
                {
                    return AlarmType.LOW;
                }
                else if (type == "2")
                {
                    return AlarmType.HIGH;
                }
                else
                {
                    Console.WriteLine("Wrong option!");
                    continue;
                }

            }
        }

        private static int GetAlarmPriority()
        {
            string Priority;
            while (true)
            {
                Console.Write("Priority(1 or 2 or 3): ");
                Priority = Console.ReadLine();
                if (Priority == "1" || Priority == "2" || Priority == "3") return Int32.Parse(Priority);
                else
                {
                    Console.WriteLine("Wrong priority!");
                    continue;
                }
            }
            
        }

        private static string GetAlarmName()
        {
            string Name;
            while (true)
            {
                Console.Write("Write alarm name: ");
                Name = Console.ReadLine();
                if (proxy.AlarmNameAvailable(Name)) return Name;
                else
                {
                    Console.WriteLine("Alarm name taken!");
                    continue;
                }
            }
        }

        private static string GetAlarmTagName()
        {
            string tagName;
            while (true)
            {
                Console.WriteLine(proxy.GetAnalogInputList());
                Console.Write("Choose input tag for alarm: ");
                tagName = Console.ReadLine();
                try
                {
                    Tag t = proxy.GetTagByName(tagName);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("That analog input tag doesnt exist");
                    continue;
                }
            }
            return tagName;
        }

        private static void RemoveAlarm()
        {
            while (true)
            {
                string Name;
                Console.WriteLine(proxy.GetAlarmList());
                Console.Write("Write alarm name( 0 to return): ");
                Name = Console.ReadLine();
                if (!proxy.AlarmNameAvailable(Name))
                {
                    if (proxy.RemoveAlarm(Name)) Console.WriteLine($"Successfully removed alarm {Name}");
                    break;
                }
                else if (Name == "0") return;
                else
                {
                    Console.WriteLine("Alarm doesnt exist!");
                    continue;
                }
            }

        }

        private static void TurnScanOnOff()
        {
            Console.Clear();
            Console.WriteLine(proxy.GetInputTagList());
            while (true)
            {
                Console.WriteLine("Write tag name: ");
                String Name = Console.ReadLine();
                if (proxy.TagNameAvailable(Name))
                {
                    Console.WriteLine("Tag doesn't exist");
                    continue;
                }
                Tag Chosen = proxy.GetTagByName(Name);
                DigitalInput DI;
                if (Chosen is AnalogInput) DI = (AnalogInput) Chosen;
                else if (Chosen is DigitalInput) DI = (DigitalInput)Chosen;
                else
                {
                    Console.WriteLine("You didnt select an input tag !!!!");
                    continue;
                }
                Console.WriteLine("1 - Turn scan on");
                Console.WriteLine("2 - Turn scan off");
                String option = Console.ReadLine();
                if (option != "1" && option != "2")
                {
                    Console.WriteLine("Wrong option!");
                    continue;
                }
                if (option == "1" && DI.ScanOn || option == "2" && !DI.ScanOn)
                {
                    Console.WriteLine("You chose same option as before");
                    return;
                }
                else
                {
                    if (option == "1") proxy.ChangeScanning(DI.Id, true);
                    else proxy.ChangeScanning(DI.Id, false);
                    Console.Clear();
                    Console.WriteLine("Scanning successfully changed!");
                    return;
                }
            }
        }

        private static void ShowOutputValues()
        {
            Console.Clear();
            String OutputTagsView = proxy.GetOutputTagList();
            Console.WriteLine(OutputTagsView);
            return;
        }

        private static void ChangeOutputValue()
        {
            Console.Clear();
            String OutputTagsView = proxy.GetOutputTagList();
            Console.WriteLine(OutputTagsView);
            if (OutputTagsView == "There are 0 output tags!") return;
            while (true)
            {
                Console.WriteLine("Write tag name ( 0 to return): ");
                String Name = Console.ReadLine();
                if (proxy.TagNameAvailable(Name))
                {
                    Console.WriteLine("Tag doesn't exist");
                    continue;
                }
                else if (Name == "0") return;
                Tag Chosen = proxy.GetTagByName(Name);
                Double Value = 0;
                if (Chosen is AnalogOutput) Value = GetAnalogValue(Chosen);
                else if (Chosen is DigitalOutput) Value = GetDigitalValue();
                else
                {
                    Console.WriteLine("You didnt select an output tag !!!!");
                    continue;
                }

                proxy.ChangeOTagValue(Name, Value);
                return;
            }

        }

        private static double GetAnalogValue(Tag tag)
        {
            AnalogOutput AO = (AnalogOutput) tag;
            Console.WriteLine("Write tag value: ");
            String StringValue = Console.ReadLine();
            Double value = Double.Parse(StringValue);
            if (value < AO.LowLimit) return AO.LowLimit;
            if (value > AO.HighLimit) return AO.HighLimit;
            return value;

        }

        private static double GetDigitalValue()
        {
            while (true) {
                Console.WriteLine("Write tag value (0 or 1): ");
                String StringValue = Console.ReadLine();
                Double value = Double.Parse(StringValue);
                if (value != 0 && value != 1)
                {
                    Console.Clear();
                    Console.WriteLine("Wrong value, digital value output must be 0 or 1");
                    continue;
                }
                return value;
            }

            }


            private static void AddTag()
            {
            TagMenu();
            string option = Console.ReadLine();
            switch (option)
                {
                    case "1":
                        CreateDigitalInput();
                        break;
                    case "2":
                        CreateDigitalOutput();
                        break;
                    case "3":
                        CreateAnalogInput();
                        break;
                    case "4":
                        CreateAnalogOutput();
                        break;

                    default:
                        Console.Clear();
                        Console.WriteLine("Wrong option!");
                        AddTag();
                        break;
                }


            }

        private static void CreateDigitalInput()
        {
            Console.WriteLine("Enter tag name: ");
            string Name = Console.ReadLine();
            if (proxy.TagNameAvailable(Name))
            {
                Console.WriteLine("Enter description: ");
                string description = Console.ReadLine();
                DriverType driver;
                while (true)
                {
                    Console.WriteLine("Choose Driver Type:");
                    Console.WriteLine("1 - Simulation Driver");
                    Console.WriteLine("2 - Real-Time Driver");
                    string option = Console.ReadLine();
                    if (option == "1")
                    {
                        driver = DriverType.SimDriver;
                        break;
                    }

                    else if (option == "2")
                    {
                        driver = DriverType.RealTimeDriver;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Wrong option!");
                        continue;
                    }
                }
                string address;
                if (driver == DriverType.SimDriver)
                {
                    while (true)
                    {
                        Console.WriteLine("Enter address (S or C or R): ");
                        address = Console.ReadLine();
                        if (address != "S" && address != "C" && address != "R") continue;
                        else break;
                    }
                }
                else
                {
                    Console.WriteLine("Enter address: ");
                    address = Console.ReadLine();
                }
                Console.WriteLine("Enter Scan Time:");
                string scanTime = Console.ReadLine();
                DigitalInput di = new DigitalInput();

                di.Id = Name;
                di.Description = description;
                di.ScanOn = true;
                di.ScanTime = Int32.Parse(scanTime);
                di.IO_Address = address;
                di.Driver = driver;
                di.Value = 0;
                proxy.AddTag(di);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Tag with that name already exists!");
                CreateDigitalInput();
            }
        }

        private static void CreateDigitalOutput()
        {
            Console.WriteLine("Enter tag name: ");
            string Name = Console.ReadLine();
            if (proxy.TagNameAvailable(Name))
            {
                Console.WriteLine("Enter description: ");
                string description = Console.ReadLine();
                string address;
                Console.WriteLine("Enter address: ");
                address = Console.ReadLine();
                Console.WriteLine("Enter initial value (0 or 1):");
                int Value;
                while (true)
                {
                    string initialValue = Console.ReadLine();
                    Value = Int32.Parse(initialValue);
                    if (Value != 0 && Value != 1)
                    {
                        Console.Clear();
                        Console.WriteLine("Wrong value, digital value output must be 0 or 1");
                        continue;
                    }
                    break;
                }
                DigitalOutput d = new DigitalOutput();

                d.Id = Name;
                d.Description = description;
                d.InitialValue = Value;
                d.IO_Address = address;
                d.Value = d.InitialValue;
                proxy.AddTag(d);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Tag with that name already exists!");
                CreateDigitalOutput();
            }


        }

        private static void CreateAnalogInput()
        {
            Console.WriteLine("Enter tag name: ");
            string Name = Console.ReadLine();
            if (proxy.TagNameAvailable(Name))
            {
                Console.WriteLine("Enter description: ");
                string description = Console.ReadLine();
                DriverType driver;
                while (true)
                {
                    Console.WriteLine("Choose Driver Type:");
                    Console.WriteLine("1 - Simulation Driver");
                    Console.WriteLine("2 - Real-Time Driver");
                    string option = Console.ReadLine();
                    if (option == "1")
                    {
                        driver = DriverType.SimDriver;
                        break;
                    }

                    else if (option == "2")
                    {
                        driver = DriverType.RealTimeDriver;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Wrong option!");
                        continue;
                    }
                }
                string address;
                if (driver == DriverType.SimDriver)
                {
                    while (true)
                    {
                        Console.WriteLine("Enter address (S or C or R): ");
                        address = Console.ReadLine();
                        if (address != "S" && address != "C" && address != "R") continue;
                        else break;
                    }
                }
                else
                {
                    Console.WriteLine("Enter address: ");
                    address = Console.ReadLine();
                }
                Console.WriteLine("Enter Scan Time:");
                string scanTime = Console.ReadLine();
                Console.WriteLine("Enter low limit:");
                string lowLimit = Console.ReadLine();
                Console.WriteLine("Enter high limit:");
                string highLimit = Console.ReadLine();
                Console.WriteLine("Enter units: ");
                string units = Console.ReadLine();
                AnalogInput ai = new AnalogInput();
                ai.Id = Name;
                ai.Description = description;
                ai.ScanOn = true;
                ai.ScanTime = Int32.Parse(scanTime);
                ai.IO_Address = address;
                ai.ScanTime = Int32.Parse(scanTime);
                ai.LowLimit = Int32.Parse(lowLimit);
                ai.HighLimit = Int32.Parse(highLimit);
                ai.Driver = driver;
                ai.Alarms = new List<Alarm>();
                ai.Value = 0;
                proxy.AddTag(ai);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Tag with that name already exists!");
                CreateAnalogInput();
            }


        }

        private static void CreateAnalogOutput()
        {
            Console.WriteLine("Enter tag name: ");
            string Name = Console.ReadLine();
            if (proxy.TagNameAvailable(Name))
            {
                Console.WriteLine("Enter description: ");
                string description = Console.ReadLine();
                string address;
                Console.WriteLine("Enter address : ");
                address = Console.ReadLine();
                Console.WriteLine("Enter initial value:");
                string initialValue = Console.ReadLine();
                Console.WriteLine("Enter low limit:");
                string lowLimit = Console.ReadLine();
                Console.WriteLine("Enter high limit:");
                string highLimit = Console.ReadLine();
                Console.WriteLine("Enter units: ");
                string units = Console.ReadLine();
                AnalogOutput ai = new AnalogOutput();
                ai.Id = Name;
                ai.Description = description;
                ai.IO_Address = address;
                ai.InitialValue = Int32.Parse(initialValue);
                ai.LowLimit = Int32.Parse(lowLimit);
                ai.HighLimit = Int32.Parse(highLimit);
                ai.Units = units;
                ai.Value = ai.InitialValue;
                proxy.AddTag(ai);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Tag with that name already exists!");
                CreateAnalogOutput();
            }

        }

        private static void RemoveTag()
        {
            Console.Clear();
            while (true)
            {
                string taglist = proxy.GetTagList();
                Console.WriteLine(taglist);
                if (taglist == "No tags to remove !")
                {
                    return;
                }
                Console.WriteLine("Enter tag name to remove or 0 to stop: ");
                string option = Console.ReadLine();
                if (option == "0") return;
                else
                {
                    if (proxy.RemoveTag(option))
                    {
                        Console.WriteLine($"Tag {option} successfully removed");
                        return;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine($"Tag {option} doesn't exist!");
                        continue;
                    }
                }

            }
        }
        private static void Registration()
        {
            Console.WriteLine("Enter username:");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();

            bool response = proxy.Registration(username, password);
            if (response)
            {
                Console.WriteLine($"User {username} successfully registered");
            }
            else
            {
                Console.WriteLine($"User {username} already exists!");

            }
        }

        private static void TagMenu()
        {        
            Console.WriteLine("Choose tag type:");
            Console.WriteLine("1-Digital Input");
            Console.WriteLine("2-Digital Output");
            Console.WriteLine("3-Analog Input");
            Console.WriteLine("4-Analog Output");
            Console.WriteLine("10-back");
            Console.WriteLine();
        }

        private static void MainMenu()
        {
            Console.WriteLine("1-Register new user");
            Console.WriteLine("2-Add tag");
            Console.WriteLine("3-Remove tag");
            Console.WriteLine("4-Change output value");
            Console.WriteLine("5-Show output tag values");
            Console.WriteLine("6-Turn scan on/off for input tags");
            Console.WriteLine("7-Add/Remove Alarm");
            Console.WriteLine("9-Logout");
            Console.WriteLine("10-Exit");
            Console.WriteLine();

        }
    }
}
