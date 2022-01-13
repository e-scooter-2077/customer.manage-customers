using System;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;

namespace EScooter.Customer.ManageCustomers
{
    public static class DTUtils
    {
        public static async Task AddDigitalTwin(Guid id, DigitalTwinsClient digitalTwinsClient)
        {
            var twinData = new BasicDigitalTwin();
            twinData.Id = id.ToString();
            twinData.Metadata.ModelId = "dtmi:com:escooter:Customer;1";
            await digitalTwinsClient.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData);
        }

        public static async Task RemoveDigitalTwin(Guid id, DigitalTwinsClient digitalTwinsClient)
        {
            await digitalTwinsClient.DeleteDigitalTwinAsync(id.ToString());
        }
    }
}
