using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ScadaCoreWCF.models;
using ScadaCoreWCF.serviceContracts;
using SimulationDriver;

namespace ScadaCoreWCF
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession)]
    class ScadaCoreWCF : IDatabaseManager, ITrending, IRealTimeUnit, IAlarmDisplay, IReportManager
    {
        private static RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        private static Dictionary<string, User> authenticatedUsers = new Dictionary<string, User>();

        private static Dictionary<string, Tag> tags = new Dictionary<string, Tag>();
        private static Dictionary<string, Thread> tagThreads = new Dictionary<string, Thread>();
        private static Dictionary<string, Tuple<string,double>> realTimeDriverEntries = new Dictionary<string, Tuple<string, double>>();

        public delegate void ITagValueScanned(string message);
        public static event ITagValueScanned onInputTagScan;
        private static ITrendingCallback trendingProxy;
        
        public delegate void AlarmDisplay(string message);
        public static event AlarmDisplay onAlarmOccurence;
        private static IAlarmDisplayCallback alarmDisplayProxy;

        public CspParameters csp = new CspParameters();
        public RSACryptoServiceProvider rsa;
        public string KEY_STORE_NAME = "KEY_STORE";
        public string RTUKeyPath = "";

        private readonly object fileLocker = new object();
        private readonly object rtuValuesLocker = new object();
        private readonly object tagsLocker = new object();
        private readonly object inputThreadsLocker = new object();
        private readonly object tagChangeDBLock = new object();
        private readonly object usersDBLock = new object();
        private readonly object alarmEventsDBLock = new object();


        public string GetAlarmList()
        {
            lock (tagsLocker)
            {
                string result = "Alarms: \n";
                result += "Name        Priority           Type            Limit        TagID\n";
                foreach (Tag t in tags.Values)
                {
                    if (t is AnalogInput)
                    {
                        foreach (Alarm a in ((AnalogInput)t).Alarms)
                        {
                            result += $"{a.Name}        {a.Priority}           {a.Type}            {a.Limit}        {t.Id}\n";
                        }
                    }
                }
                return result;
            }

        }

        public string GetOutputTagList()
        {
            lock (tagsLocker)
            {
                string result = "Output tags: \n";
                if (tags.Values.Count == 0) return "There are 0 output tags!";
                result += "Type                   Name                      Value\n";
                foreach (Tag tag in tags.Values)
                {
                    if (tag is AnalogOutput) result += $"AnalogOutput           {tag.Id}                    {tag.Value}\n";
                    else if (tag is DigitalOutput) result += $"DigitalOutput          {tag.Id}                    {tag.Value}\n";
                }
                return result;
            }
        }

        public string GetInputTagList()
        {
            lock (tagsLocker)
            {
                string result = "Input tags: \n";
                if (tags.Values.Count == 0) return "There are 0 input tags!";
                result += "Type                    Name           ScanOn\n";
                foreach (Tag tag in tags.Values)
                {
                    if (tag is AnalogInput)
                    {
                        AnalogInput AI = (AnalogInput)tag;
                        result += $"AnalogInput             {AI.Id}           {AI.ScanOn}\n";
                    }
                    else if (tag is DigitalInput)
                    {
                        DigitalInput DI = (DigitalInput)tag;
                        result += $"DigitalInput            {DI.Id}           {DI.ScanOn}\n";
                    }

                }
                return result;
            }
        }

        public string GetAnalogInputList()
        {
            lock (tagsLocker)
            {
                string result = "Analog Input tags: \n";
                result += "Type          Name           ScanOn\n";
                foreach (Tag tag in tags.Values)
                {
                    if (tag is AnalogInput)
                    {
                        AnalogInput AI = (AnalogInput)tag;
                        result += $"AnalogInput           {AI.Id}           {AI.ScanOn}\n";
                    }

                }
                return result;
            }

        }

        public void ChangeScanning(String Name, bool Scanning)
        {
            lock (tagsLocker)
            {
                ((DigitalInput)tags[Name]).ScanOn = Scanning;
            }
            WriteData();
        }

        public void ChangeOTagValue(string name, double value)
        {
            lock (tagsLocker)
            {
                tags[name].Value = value;
            }
            WriteData();
            AddTagChange(new TagChange(name, DateTime.Now, value));
        }

        public string GetTagList()
        {
            lock (tagsLocker)
            {
                string result = "Tag name list: \n";
                if (tags.Values.Count == 0) return "No tags to remove !";
                foreach (Tag tag in tags.Values)
                {
                    result += tag.Id + "\n";
                }
                return result;
            }
        }

        public void LoadData()
        {
            if (tags.Count == 0)
            {
                if (File.Exists(@"..\..\files\scadaConfig.xml"))
                {
                    FileStream fs = new FileStream(@"..\..\files\scadaConfig.xml", FileMode.Open);
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                    DataContractSerializer ser = new DataContractSerializer(typeof(List<Tag>));
                    List<Tag> xmlTags = (List<Tag>)ser.ReadObject(reader, true);
                    reader.Close();
                    fs.Close();
                    foreach (Tag tag in xmlTags)
                    {
                        AddTag(tag);
                    }
                }
            }
        }

        public bool AddTag(Tag tag)
        {
            lock (tagsLocker)
            {
                tags.Add(tag.Id, tag);
            }

            lock (inputThreadsLocker)
            {
                if (tag is AnalogInput)
                {
                    tagThreads.Add(tag.Id, new Thread(AISimulation));
                    tagThreads[tag.Id].Start(tag);
                }

                else if (tag is DigitalInput)
                {
                    tagThreads.Add(tag.Id, new Thread(DISimulation));
                    tagThreads[tag.Id].Start(tag);
                }
            }
            WriteData();
            return true;

        }

        public void DISimulation(Object obj) 
        {
            DigitalInput DI = (DigitalInput) obj;
            while (true)
            {
                if (DI.ScanOn)
                {
                    Thread.Sleep(DI.ScanTime * 1000);
                    double oldVal = DI.Value;
                    double value = oldVal;
                    lock (tagsLocker)
                    {
                        if (DI.Driver == DriverType.SimDriver) value = Driver.ReturnValue(DI.IO_Address);
                        else if (realTimeDriverEntries.ContainsKey(DI.IO_Address)) value = realTimeDriverEntries[DI.IO_Address].Item2;
                        if (value > 5 && value != oldVal) tags[DI.Id].Value = 1;
                        else if (value < 5 && value != oldVal) tags[DI.Id].Value = 0;
                        value = tags[DI.Id].Value;
                    }
                    WriteData();
                    if ((realTimeDriverEntries.ContainsKey(DI.IO_Address) && DI.Driver == DriverType.RealTimeDriver) || (DI.Driver == DriverType.SimDriver))
                        onInputTagScan?.Invoke($"Digital input tag {DI.Id} scanned value {tags[DI.Id].Value}");
                    if (value != oldVal)
                    {
                        
                        AddTagChange(new TagChange(DI.Id, DateTime.Now, tags[DI.Id].Value));
                    }

                }
            }

        }

        public void AISimulation(Object obj)
        {
            AnalogInput AI = (AnalogInput)obj;
            while (true)
            {
                if (AI.ScanOn)
                {
                    Thread.Sleep(AI.ScanTime * 1000);
                    double oldVal = AI.Value;
                    double value = oldVal;
                    lock (tagsLocker)
                    {
                        if (AI.Driver == DriverType.SimDriver) value = Driver.ReturnValue(AI.IO_Address);
                        else if (realTimeDriverEntries.ContainsKey(AI.IO_Address)) value = realTimeDriverEntries[AI.IO_Address].Item2;
                        if (value < AI.LowLimit) value = AI.LowLimit;
                        if (value > AI.HighLimit) value = AI.HighLimit;
                        tags[AI.Id].Value = value;
                    }
                    WriteData();
                    if ((realTimeDriverEntries.ContainsKey(AI.IO_Address) && AI.Driver == DriverType.RealTimeDriver) || (AI.Driver == DriverType.SimDriver))
                        onInputTagScan?.Invoke($"Analog input tag {AI.Id} scanned value {tags[AI.Id].Value}");
                    CheckForAlarms(AI.Id);
                    if (value != oldVal)
                    {                                              
                        AddTagChange(new TagChange(AI.Id, DateTime.Now, tags[AI.Id].Value));
                    }

                }
            }
        }

        public void CheckForAlarms(string keyID)
        {
            lock (tagsLocker)
            {
                foreach (Alarm a in ((AnalogInput)tags[keyID]).Alarms)
                {
                    if (a.Type == AlarmType.LOW && ((AnalogInput)tags[keyID]).Value < a.Limit)
                    {
                        string message = $"INPUT TAG WITH ID {keyID} HAS DETECTED VALUE {((AnalogInput)tags[keyID]).Value }, LOWEST LIMIT IS {a.Limit}!!!!! ";
                        for (int i = 0; i < a.Priority; i++) onAlarmOccurence?.Invoke(message);
                        WriteAlarmsTxt(message);
                        AddAlarmEvent(new AlarmEvent(a, DateTime.Now));

                    }
                    else if (a.Type == AlarmType.HIGH && ((AnalogInput)tags[keyID]).Value > a.Limit)
                    {
                        string message = $"INPUT TAG WITH ID {keyID} HAS DETECTED VALUE {((AnalogInput)tags[keyID]).Value }, HIGHEST LIMIT IS {a.Limit} ";
                        for (int i = 0; i < a.Priority; i++) onAlarmOccurence?.Invoke(message);
                        WriteAlarmsTxt(message);
                        AddAlarmEvent(new AlarmEvent(a, DateTime.Now));
                    }

                }
            }
        }

        public bool AddAlarm(Alarm alarm, string tagID)
        {
            lock (tagsLocker)
            {
                ((AnalogInput)tags[tagID]).Alarms.Add(alarm);
                WriteData();
                return true;
            }
        }

        public bool RemoveAlarm(string alarmName)
        {
            lock (tagsLocker)
            {
                foreach (Tag t in tags.Values)
                {
                    if (t is AnalogInput)
                    {
                        foreach (Alarm a in ((AnalogInput)t).Alarms)
                        {
                            if (a.Name == alarmName) ((AnalogInput)t).Alarms.Remove(a);
                            WriteData();
                            return true;
                        }
                    }
                }
                return false;
            }
        }


        public bool RemoveTag(String name)
        {
            lock (tagsLocker)
            {
                if (tags.ContainsKey(name))
                {
                    tags.Remove(name);
                    
                    if (tagThreads.ContainsKey(name))
                    {
                        lock (inputThreadsLocker)
                        {
                            tagThreads[name].Abort();
                            tagThreads.Remove(name);
                        }
                    }
                    WriteData();
                    return true;
                }
                return false;
            }
        }
        public void WriteData()
        {
            lock (fileLocker)
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Tag>));
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

                using (XmlWriter writer = XmlWriter.Create(@"..\..\files\scadaConfig.xml", settings))
                {
                    serializer.WriteObject(writer, tags.Values.ToList());
                    writer.Close();
                }
            }
        }
        public string Login(string username, string password)
        {
            using (var db = new UsersContext())
            {
                foreach (var user in db.Users)
                {
                    if (username == user.Username &&
                   ValidateEncryptedData(password, user.EncryptedPassword))
                    {
                        string token = GenerateToken(username);
                        authenticatedUsers.Add(token, user);
                        return token;
                    }
                }
            }
            return "Login failed";
        }

        public bool Logout(string token)
        {
            return authenticatedUsers.Remove(token);

        }

        public void WriteAlarmsTxt(string message)
        {
            lock (fileLocker)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(@"..\..\files\alarms.txt", append: true);
                    sw.WriteLine(message);
                    sw.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
            }
        }

        public bool AddAlarmEvent(AlarmEvent ae)
        {
            lock (alarmEventsDBLock)
            {
                using (var db = new AlarmEventContext())
                {
                    try
                    {
                        db.alarmEvents.Add(ae);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool AddTagChange(TagChange tg)
        {
            lock (tagChangeDBLock)
            {

                using (var db = new TagChangeContext())
                {
                    try
                    {
                        db.tagChanges.Add(tg);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool Registration(string username, string password)
        {
            lock (usersDBLock)
            {
                string encryptedPassword = EncryptData(password);
                User user = new User(username, encryptedPassword);
                using (var db = new UsersContext())
                {
                    try
                    {
                        db.Users.Add(user);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private string GenerateToken(string username)
        {
            byte[] randVal = new byte[32];
            crypto.GetBytes(randVal);
            string randStr = Convert.ToBase64String(randVal);
            return username + randStr;
        }

        private static string EncryptData(string valueToEncrypt)
        {
            string GenerateSalt()
            {
                byte[] salt = new byte[32];
                crypto.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
            string EncryptValue(string strValue)
            {
                string saltValue = GenerateSalt();
                byte[] saltedPassword = Encoding.UTF8.GetBytes(saltValue + strValue);
                using (SHA256Managed sha = new SHA256Managed())
                {
                    byte[] hash = sha.ComputeHash(saltedPassword);
                    return $"{Convert.ToBase64String(hash)}:{saltValue}";
                }
            }
            return EncryptValue(valueToEncrypt);
        }

        private static bool ValidateEncryptedData(string valueToValidate, string valueFromDatabase)
        {
            string[] arrValues = valueFromDatabase.Split(':');
            string encryptedDbValue = arrValues[0];
            string salt = arrValues[1];
            byte[] saltedValue = Encoding.UTF8.GetBytes(salt + valueToValidate);
            using (var sha = new SHA256Managed())
            {
                byte[] hash = sha.ComputeHash(saltedValue);
                string enteredValueToValidate = Convert.ToBase64String(hash);
                return encryptedDbValue.Equals(enteredValueToValidate);
            }
        }

        public bool TagNameAvailable(string Name)
        {
            lock (tagsLocker)
            {
                if (tags.ContainsKey(Name)) return false;
                return true;
            }
        }

        public Tag GetTagByName(string Name)
        {
            lock (tagsLocker)
            {
                return tags[Name];
            }
        }

        public string TrendingConnection()
        {
            trendingProxy = OperationContext.Current.GetCallbackChannel<ITrendingCallback>();
            onInputTagScan += trendingProxy.ITagValueChanged;
            return "Successfully connected";
        }

        public string AlarmDisplayConnection()
        {
            alarmDisplayProxy = OperationContext.Current.GetCallbackChannel<IAlarmDisplayCallback>();
            onAlarmOccurence += alarmDisplayProxy.DisplayAlarm;
            return "Successfully connected";
        }


        public void LeavePublicKey(string path)
        {
            RTUKeyPath = path;
            ImportPublicKey();
        }

        private void ImportPublicKey()
        {
            FileInfo fi = new FileInfo(RTUKeyPath);
            if (fi.Exists)
            {
                using (StreamReader reader = new StreamReader(RTUKeyPath))
                {
                    csp.KeyContainerName = KEY_STORE_NAME;
                    rsa = new RSACryptoServiceProvider(csp);
                    string publicKeyText = reader.ReadToEnd();
                    rsa.FromXmlString(publicKeyText);
                    rsa.PersistKeyInCsp = true;
                }
            }
        }

        public bool AddressAvailable(string address)
        {
            lock (rtuValuesLocker)
            {
                return realTimeDriverEntries.ContainsKey(address);
            }
        }

        public bool RTUIdAvailable(string id)
        {
            lock (rtuValuesLocker)
            {
                foreach (Tuple<string, double> t in realTimeDriverEntries.Values)
                {
                    if (t.Item1 == id) return false;
                }                   
            }
            return true;
        }

        public bool AlarmNameAvailable(string Name)
        {
            lock (tagsLocker)
            {
                foreach (Tag t in tags.Values)
                {
                    if (t is AnalogInput)
                    {
                        foreach (Alarm a in ((AnalogInput)t).Alarms)
                        {
                            if (a.Name == Name) return false;
                        }
                    }
                }
                return true;
            }

        }

        public string WriteRtuMessage(string message, byte[] signature)
        {
            lock (rtuValuesLocker) 
            {
                string[] data = message.Split(',');
                string id = data[0];
                string address = data[1];
                string valueString = data[2];
                double value;
                if (!double.TryParse(valueString, out value))
                {
                    return "Error with value";
                }
                if (VerifySignedMessage(message, signature))
                {
                    realTimeDriverEntries[address] = new Tuple<string, double>(id, value);
                    return "Value written successfully";
                }
                else
                {
                    return "Message signature not correct!";
                }
            }
        }

        public string StopRTU(string id)
        {
            lock (rtuValuesLocker)
            {
                foreach (string key in realTimeDriverEntries.Keys) 
                { 
                    if (realTimeDriverEntries[key].Item1 == id) realTimeDriverEntries.Remove(key);
                    return $"RTU {id} successfully removed";
                }
                return "RTU isn't connected on service!";
            }
        }

        public byte[] ComputeMessageHash(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }

        private bool VerifySignedMessage(string message, byte[] signature)
        {
            byte[] hash = ComputeMessageHash(message);
            var deformatter = new RSAPKCS1SignatureDeformatter(rsa);
            deformatter.SetHashAlgorithm("SHA256");
            return deformatter.VerifySignature(hash, signature);

        }

        public string ReportAlarmsInPeriod(DateTime start, DateTime end)
        {
            lock (alarmEventsDBLock)
            {
                string report = "";
                using (var db = new AlarmEventContext())
                {
                    var alarms = db.alarmEvents.Where(a => a.Time > start && a.Time < end).OrderBy(a => a.Priority).ThenBy(a => a.Time);
                    foreach (AlarmEvent a in alarms)
                    {
                        report += a.ToString() + "\n";
                    }
                }
                return report;
            }
        }

        public string ReportAlarmsWithSelectedPriority(int priority)
        {
            lock (alarmEventsDBLock)
            {
                string report = "";
                using (var db = new AlarmEventContext())
                {
                    var alarms = db.alarmEvents.Where(a => a.Priority == priority).OrderBy(a => a.Time);
                    foreach (AlarmEvent a in alarms)
                    {
                        report += a.ToString() + "\n";
                    }
                }
                return report;
            }
        }

        public string ReportTagValueChangesInPeriod(DateTime start, DateTime end)
        {
            lock (tagChangeDBLock)
            {
                string report = "";
                using (var db = new TagChangeContext())
                {
                    var tagChanges = db.tagChanges.Where(t => t.Time > start && t.Time < end).OrderBy(t => t.Time);
                    foreach (TagChange t in tagChanges)
                    {
                        report += t.ToString() + "\n";
                    }
                }
                return report;
            }
        }

        public string ReportLastAnalogInputValues()
        {
            lock (tagChangeDBLock)
            {
                string report = "";
                List<TagChange> lastChanges = new List<TagChange>();
                using (var db = new TagChangeContext())
                {
                    lock (tagsLocker)
                    {
                        foreach (Tag t in tags.Values)
                        {
                            if (t is AnalogInput)
                            {
                                TagChange lastChange = db.tagChanges.OrderByDescending(tagChange => tagChange.Time).FirstOrDefault(tagChange => tagChange.TagID == t.Id);
                                if (lastChange is object) lastChanges.Add(lastChange);
                            }

                        }
                    }                
                 }
                    lastChanges.OrderBy(t => t.Time);
                foreach (TagChange t in lastChanges)
                {
                    report += t.ToString() + "\n";
                }

                return report;
            }
        }

        public string ReportLastDigitalInputValues()
        {
            lock (tagChangeDBLock)
            {
                string report = "";
                List<TagChange> lastChanges = new List<TagChange>();
                using (var db = new TagChangeContext())
                {
                    lock (tagsLocker)
                    {
                        foreach (Tag t in tags.Values)
                        {
                            if (t is DigitalInput && !(t is AnalogInput))
                            {
                                TagChange lastChange = db.tagChanges.OrderByDescending(tagChange => tagChange.Time).FirstOrDefault(tagChange => tagChange.TagID == t.Id);
                                if (lastChange is object) lastChanges.Add(lastChange);
                            }
                        }
                    }
                }
                lastChanges.OrderBy(t => t.Time);
                foreach (TagChange t in lastChanges)
                {
                    report += t.ToString() + "\n";
                }

                return report;
            }
        }

        public string ReportValuesOfSelectedID(string ID)
        {
            lock (tagChangeDBLock)
            {
                string report = "";
                using (var db = new TagChangeContext())
                {
                    var tagChanges = db.tagChanges.Where(t => t.TagID == ID).OrderBy(t => t.Value);
                    foreach (TagChange t in tagChanges)
                    {
                        report += t.ToString() + "\n";
                    }
                }
                return report;
            }
        }

    }
}
