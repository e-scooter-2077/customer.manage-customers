using System;
using System.Net.Http;
using Azure;
using Azure.Core.Pipeline;
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
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _digitalTwinUrl = "https://" + Environment.GetEnvironmentVariable("AzureDTHostname");

        private static DigitalTwinsClient InstantiateDtClient()
        {
            var credential = new DefaultAzureCredential();
            return new DigitalTwinsClient(
                        new Uri(_digitalTwinUrl),
                        credential,
                        new DigitalTwinsClientOptions { Transport = new HttpClientTransport(_httpClient) });
        }

        [Function("add-customer")]
        public static async void AddCustomer([ServiceBusTrigger("%TopicName%", "%AddSubscription%", Connection = "ServiceBusConnectionString")] string mySbMsg, FunctionContext context)
        {
            var logger = context.GetLogger("add-customer");
            var digitalTwinsClient = InstantiateDtClient();

            var message = JsonConvert.DeserializeObject<CustomerCreated>(mySbMsg);
            try
            {
                await DTUtils.AddDigitalTwin(message.Id, digitalTwinsClient);
                logger.LogInformation($"Add customer: {mySbMsg}");
            }
            catch (RequestFailedException e)
            {
                logger.LogInformation($"Function failed to add customer {e.ErrorCode}");
            }
        }

        [Function("remove-customer")]
        public static async void RemoveCustomer([ServiceBusTrigger("%TopicName%", "%RemoveSubscription%", Connection = "ServiceBusConnectionString")] string mySbMsg, FunctionContext context)
        {
            var logger = context.GetLogger("remove-customer");
            var digitalTwinsClient = InstantiateDtClient();

            var message = JsonConvert.DeserializeObject<CustomerDeleted>(mySbMsg);

            try
            {
                await DTUtils.RemoveDigitalTwin(message.Id, digitalTwinsClient);
                logger.LogInformation($"Removed customer: {mySbMsg}");
            }
            catch (RequestFailedException e)
            {
                logger.LogInformation($"Function failed to remove customer {e.ErrorCode}");
            }
        }
    }
}
