using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Robin
{
    class Program
    {
        static DeviceClient deviceClient;
        static string deviceID = "Robin";

        static void Main(string[] args)
        {
            Console.WriteLine($"Simulated device: {deviceID}. Check certificate");
            InitCert();
            Console.WriteLine("Certificate ok");

            //deviceClient = DeviceClient.CreateFromConnectionString(ConfigurationManager.AppSettings["connStringIOTHUB"]);
            deviceClient = DeviceClient.CreateFromConnectionString(ConfigurationManager.AppSettings["connStringTGWVM"], TransportType.Mqtt);

            Console.WriteLine("Connection established");

            SendDeviceToCloudMessagesAsync().Wait();
            Console.ReadLine();
        }

        private static async Task SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            int messageId = 1;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = Math.Round((minTemperature + rand.NextDouble() * 15), 2);
                string level = (currentTemperature > 30 ? "critical" : "normal");

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = deviceID,
                    temperature = currentTemperature,

                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("level", level);

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message - level: {1} - {2}", DateTime.Now, level, messageString);

                await Task.Delay(100);
            }
        }

        private static void InitCert()
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(ConfigurationManager.AppSettings["certPath"])));
            store.Close();
        }
    }
}
