using Microsoft.Azure.Devices;
using Microsoft.Bot.Connector;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace LED_Bot
{
    public static class Common
    {
        public static string HubConnString = "HostName=samplehub13.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=rEIMLHddWvwHD3HFpad/5vswJSJaDmuspE8kJO8OfOQ=";
        public static string DeviceId = "RPi";
        public static Activity activity = null;
        public static ConnectorClient connector = null;

        public static async Task Send(string txt)
        {
            if (activity != null && connector != null)
            {
                var repl = activity.CreateReply(txt);
                await connector.Conversations.ReplyToActivityAsync(repl);
            }
        }

        public static async Task Monitor()
        {
            EventHubClient cli = EventHubClient.CreateFromConnectionString(HubConnString,"messages/events");
            var runtimeInfo = await cli.GetRuntimeInformationAsync();
            foreach (var p in runtimeInfo.PartitionIds)
            {
                var rec = await cli.GetDefaultConsumerGroup().CreateReceiverAsync(p);
                Func<Task> f = async () =>
                {
                    while (true)
                    {
                        var x = await rec.ReceiveAsync();
                        var s = Encoding.UTF8.GetString(x.GetBytes());
                        await Send(s);
                    }
                };
                f();
            }
        }

    }
}