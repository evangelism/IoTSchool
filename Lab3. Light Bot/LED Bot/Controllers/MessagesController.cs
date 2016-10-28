using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using System.Text;

namespace LED_Bot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                var txt = activity.Text.ToLower();
                var res = "Oops!";
                if (txt.StartsWith("hostname="))
                {
                    Store.RegisterClient(activity.From.Id, txt);
                    res = "Hub registered!";
                }
                else if (txt == "on")
                {
                    res = await Process(activity.From.Id, true);
                }
                else if (txt == "off")
                {
                    res = await Process(activity.From.Id, false);
                }
                else res = "I do not understand you.";

                Activity reply = activity.CreateReply(res);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<string> Process(string id, bool v)
        {
            var hcs = Store.GetConnString(id);
            if (hcs == null) return "Hub not configured, please send me connection string";
            var hub = ServiceClient.CreateFromConnectionString(hcs);
            var state = v ? "on" : "off";
            await hub.SendAsync("RPi", new Message(Encoding.UTF8.GetBytes(state)));
            return state;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}