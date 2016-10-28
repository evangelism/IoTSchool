using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LED_Bot
{
    public static class Store
    {
        public static Dictionary<string, string> HubConnString = new Dictionary<string, string>();
        
        public static void RegisterClient(string uid, string hcs)
        {
            HubConnString[uid] = hcs;
        }

        public static string GetConnString(string uid)
        {
            if (HubConnString.ContainsKey(uid)) return HubConnString[uid];
            else return null;
        }

    }
}