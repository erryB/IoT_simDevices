using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Devices.Provisioning.Service;
using System.Configuration;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace Alfred
{
    class Program
    {

        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
        private static string s_idScope = ConfigurationManager.AppSettings["IDscope"];
        private static string s_certificateFileName = ConfigurationManager.AppSettings["certPath"];
        private static DeviceClient iotClient;
        private static string deviceID = "alfred";
        private static int messN = 10;

        static void Main(string[] args)
        {

            string certificatePassword = ReadCertificatePassword();

            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(s_certificateFileName, certificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 certificate = null;

            foreach (X509Certificate2 element in certificateCollection)
            {
                Console.WriteLine($"Found certificate: {element?.Thumbprint} {element?.Subject}; PrivateKey: {element?.HasPrivateKey}");
                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            if (certificate == null)
            {
                Console.WriteLine($"ERROR: {s_certificateFileName} did not contain any certificate with a private key.");
                return;
            }
            else
            {
                Console.WriteLine($"Using certificate {certificate.Thumbprint} {certificate.Subject}");
            }

            RunSample(certificate).GetAwaiter().GetResult();


        }

        private static string ReadCertificatePassword()
        {
            //In this sample the psw is in the app settings 
            return ConfigurationManager.AppSettings["psw"];
            

        }

        public static async Task RunSample(X509Certificate2 certificate)
        {
            using (var security = new SecurityProviderX509Certificate(certificate))
            // using (var transport = new ProvisioningTransportHandlerHttp())
            //using (var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly))
            using (var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.TcpOnly))
            {
                ProvisioningDeviceClient provClient =
                    ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, s_idScope, security, transport);

                Console.WriteLine($"RegistrationID = {security.GetRegistrationID()}");
                Console.Write("ProvisioningClient RegisterAsync . . . ");
                DeviceRegistrationResult result = await provClient.RegisterAsync();

                Console.WriteLine($"{result.Status}");
                Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");

                if (result.Status != ProvisioningRegistrationStatusType.Assigned) return;

                IAuthenticationMethod auth = new DeviceAuthenticationWithX509Certificate(result.DeviceId, certificate);
                iotClient = DeviceClient.Create(result.AssignedHub, auth);

                Console.WriteLine($"Now {deviceID} can start sending messages to assigned IoT Hub: {result.AssignedHub}");

                await iotClient.OpenAsync();

                SendDeviceToCloudMessagesAsync();


                Console.ReadLine();
            }
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            int messageId = 1;
            Random rand = new Random();

            for (int i = 0; i < messN; i++)
            {
                double currentTemperature = Math.Round((minTemperature + rand.NextDouble() * 15), 2);
                double currentHumidity = Math.Round((minHumidity + rand.NextDouble() * 20), 2);
                string level = (currentTemperature > 30 ? "critical" : "normal");

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = deviceID,
                    temperature = currentTemperature,
                    humidity = currentHumidity,

                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("level", level);

                await iotClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message - level: {1} - {2}", DateTime.Now, level, messageString);

                await Task.Delay(100);
            }


            Console.WriteLine("Closing...");
            await iotClient.CloseAsync();

        }

    }
}
