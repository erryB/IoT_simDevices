using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joker
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = ConfigurationManager.AppSettings["IoTHubURI"];
        static string deviceKey = ConfigurationManager.AppSettings["DeviceKey"];

        static void Main(string[] args)
        {
            try
            {
                InitClient();
                ReportConnectivity();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
            Console.ReadLine();

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            //bool doorOpen = false;
            int messageId = 1;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = Math.Round((minTemperature + rand.NextDouble() * 15), 2);
                bool currentlyOpen = rand.NextDouble() > 0.5;
                string level = ((currentTemperature > 30) && currentlyOpen ? "critical" : "normal");

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = "Joker",
                    temperature = currentTemperature,
                    doorOpen = currentlyOpen

                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("level", level);

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message - level: {1} - {2}", DateTime.Now, level, messageString);

                await Task.Delay(100);
            }
        }

        public static async void InitClient()
        {
            try
            {
                Console.WriteLine("Simulated device: Joker\n");
                deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("Joker", deviceKey), TransportType.Mqtt);
                Console.WriteLine("Retrieving twin");
                await deviceClient.GetTwinAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        public static async void ReportConnectivity()
        {
            try
            {
                Console.WriteLine("Sending connectivity data as reported property");

                TwinCollection reportedProperties, connectivity;
                reportedProperties = new TwinCollection();
                connectivity = new TwinCollection();
                connectivity["type"] = "cellular";
                reportedProperties["connectivity"] = connectivity;
                await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

    }
}
