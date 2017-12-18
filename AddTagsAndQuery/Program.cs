using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddTagsAndQuery
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = ConfigurationManager.AppSettings["IoTHubConnectionString"];

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddTagsAndQuery().Wait();
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        public static async Task AddTagsAndQuery()
        {
            var twin = await registryManager.GetTwinAsync("Joker");
            var patch =
                @"{
                tags: {
                    location: {
                        region: 'IT',
                        plant: 'MicrosoftHouse'
                    }
                }
            }";
            await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);

            var query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.plant = 'MicrosoftHouse'", 100);
            var twinsInMSHouse = await query.GetNextAsTwinAsync();
            Console.WriteLine("Devices in Microsoft House: {0}", string.Join(", ", twinsInMSHouse.Select(t => t.DeviceId)));

            query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.plant = 'MicrosoftHouse' AND properties.reported.connectivity.type = 'cellular'", 100);
            var twinsInMSHouseUsingCellular = await query.GetNextAsTwinAsync();
            Console.WriteLine("Devices in Microsoft House using cellular network: {0}", string.Join(", ", twinsInMSHouseUsingCellular.Select(t => t.DeviceId)));
        }
    }
}
