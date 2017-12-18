using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batman
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = ConfigurationManager.AppSettings["IoTHubURI"];
        static string deviceKey = ConfigurationManager.AppSettings["DeviceKey"];

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device: Batman\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("Batman", deviceKey), TransportType.Mqtt);

            SendDeviceToCloudMessagesAsync();
            ReceiveC2dAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            int messageId = 1;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = Math.Round((minTemperature + rand.NextDouble() * 15), 2);
                double currentHumidity = Math.Round((minHumidity + rand.NextDouble() * 20), 2);
                string level = (currentTemperature > 30 ? "critical" : "normal");

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = "Batman",
                    temperature = currentTemperature,
                    humidity = currentHumidity,

                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("level", level);

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message - level: {1} - {2}", DateTime.Now, level, messageString);

                await Task.Delay(100);
            }
        }

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

    }
}
