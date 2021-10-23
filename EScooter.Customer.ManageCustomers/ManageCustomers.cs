using System;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EScooter.Customer.ManageCustomers
{
    public record CustomerCreated(Guid Id);

    public record CustomerDeleted(Guid Id);

    public static class ManageCustomers
    {
        [Function("add-customer")]
        public static async void AddCustomer([ServiceBusTrigger("%TopicName%", "%AddSubscription%", Connection = "ServiceBusConnectionString")] string mySbMsg, FunctionContext context)
        {
            var logger = context.GetLogger("add-customer");
            string digitalTwinUrl = "https://" + Environment.GetEnvironmentVariable("AzureDTHostname");
            var credential = new DefaultAzureCredential();
            var digitalTwinsClient = new DigitalTwinsClient(new Uri(digitalTwinUrl), credential);

            var message = JsonConvert.DeserializeObject<CustomerCreated>(mySbMsg);

            try
            {
                await DTUtils.AddDigitalTwin(message.Id, digitalTwinsClient);
                logger.LogInformation($"Add customer: {mySbMsg}");
            }
            catch (RequestFailedException e)
            {
                logger.LogError($"Create twin error: {e.Status}: {e.Message}");
            }
        }

        [Function("remove-customer")]
        public static async void RemoveCustomer([ServiceBusTrigger("%TopicName%", "%AddSubscription%", Connection = "ServiceBusConnectionString")] string mySbMsg, FunctionContext context)
        {
            var logger = context.GetLogger("remove-customer");
            string digitalTwinUrl = "https://" + Environment.GetEnvironmentVariable("AzureDTHostname");
            var credential = new DefaultAzureCredential();
            var digitalTwinsClient = new DigitalTwinsClient(new Uri(digitalTwinUrl), credential);

            var message = JsonConvert.DeserializeObject<CustomerDeleted>(mySbMsg);

            try
            {
                await DTUtils.AddDigitalTwin(message.Id, digitalTwinsClient);
                logger.LogInformation($"Removed customer: {mySbMsg}");
            }
            catch (RequestFailedException e)
            {
                logger.LogError($"Remove twin error: {e.Status}: {e.Message}");
            }
        }

        internal static class DTUtils
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
}
