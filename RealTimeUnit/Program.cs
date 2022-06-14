using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RealTimeUnit.ScadaCoreRtuReference;

namespace RealTimeUnit
{
    class Program
    {
        private static RealTimeUnitClient proxy = new RealTimeUnitClient();
        private static string id;
        private static int lowLimit;
        private static int highLimit;
        private static string address;
        public static CspParameters csp = new CspParameters();
        public static RSACryptoServiceProvider rsa;
        public static string EXPORT_FOLDER = @"C:\keys\";
        public static string KEY_STORE_NAME = "KEY_STORE";

        static void Main(string[] args)
        {

            SetRtuID();
            SetLimits();
            SetAddress();
            CreateAsmKeys(true);
            ExportPublicKey();
            proxy.LeavePublicKey(Path.Combine(EXPORT_FOLDER,$"{id}.txt"));
            Console.Clear();
            Console.WriteLine($"Successfully created RTU with identifier {id}, low limit {lowLimit}, high limit {highLimit} and address {address}");
            Thread.Sleep(1000);
            while (true)
            {
                Console.WriteLine($"You are currently writing on address {address}");
                Console.WriteLine("Select option:");
                Console.WriteLine("1-Write value");
                Console.WriteLine("2-Exit");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":SendValue();
                             Console.Clear();
                             break;
                    case "2":Console.WriteLine(proxy.StopRTU(id));
                             Environment.Exit(0);
                             break;
                    default:
                        Console.WriteLine("Wrong option!");
                        continue;
                }
            }



        }

        private static void SendValue()
        {
            double value;
            string valueString;
            while (true)
            {
                Console.WriteLine("Enter Value:");
                valueString = Console.ReadLine();
                if (double.TryParse(valueString, out value))
                    break;
                else
                {
                    Console.WriteLine("Invalid input");
                    continue;
                }
            }
            if (value < lowLimit)
                value = lowLimit;
            if (value > highLimit)
                value = highLimit;
            Console.WriteLine($"Trying to send value {value} to address {address}... ");
            string message = $"{id},{address},{value}";
            string response = proxy.WriteRtuMessage(message, SignMessage(message));
            Console.WriteLine($"Response from server : {response}");
            Thread.Sleep(2000);


        }

        private static void SetAddress()
        {
            while (true)
            {
                Console.WriteLine("Enter RTU address: ");
                address = Console.ReadLine();
                if (!proxy.AddressAvailable(address)) break;
                else
                {
                    Console.WriteLine("That address is already taken by another RTU!");
                    continue;
                }

            }
        }

        private static void SetRtuID()
        {
            while (true)
            {
                Console.WriteLine("Enter RTU identifier: ");
                id = Console.ReadLine();
                if (proxy.RTUIdAvailable(id)) break;
                else
                {
                    Console.WriteLine("RTU with that id already exists!");
                    continue;
                }
            }
        }
        
        private static void SetLimits()
        {
            while (true)
            {
                Console.WriteLine("Enter Low Limit:");
                string lowLimitString = Console.ReadLine();
                if (int.TryParse(lowLimitString, out lowLimit))
                    break;
                else
                    Console.WriteLine("Use integer value!");
            }

            while (true)
            {
                Console.WriteLine("Enter High Limit:");
                string highLimitString = Console.ReadLine();
                if (int.TryParse(highLimitString, out highLimit))
                    if (highLimit > lowLimit)
                        break;
                    else
                    {
                        Console.WriteLine("High limit cannot be lower than low limit!");
                        continue;
                    }
                else
                {
                    Console.WriteLine("Use integer value!");
                    continue;
                }

             }
        }

        private static void ExportPublicKey()
        {
            if (!Directory.Exists(EXPORT_FOLDER))
            {
                Directory.CreateDirectory(EXPORT_FOLDER);
            }
            using (StreamWriter writer = new StreamWriter(Path.Combine(EXPORT_FOLDER,$"{id}.txt")))
            {
                writer.WriteLine(rsa.ToXmlString(false));
            }
        }

        private static void CreateAsmKeys(bool useMachineKeyStore)
        {
            csp.KeyContainerName = KEY_STORE_NAME;
            if (useMachineKeyStore)
                csp.Flags = CspProviderFlags.UseMachineKeyStore;
            rsa = new RSACryptoServiceProvider(csp);
            rsa.PersistKeyInCsp = true;
           
          
        }

        public static byte[] ComputeMessageHash(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }


        public static byte[] SignMessage(string message)
        {

            byte[] hashValue = ComputeMessageHash(message);
            var formatter = new RSAPKCS1SignatureFormatter(rsa);
            formatter.SetHashAlgorithm("SHA256");
            return formatter.CreateSignature(hashValue);
        }

    }
}
