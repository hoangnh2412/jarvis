using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis.Core.Extensions
{
    public static class NetworkExtension
    {
        public static async Task<string> GetLocalIpAddressAsync()
        {
            var host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static IPAddress GetLocalIpAddress(HttpRequest request)
        {
            return request.HttpContext.Connection.LocalIpAddress;
        }

        public static async Task<string> GetRemoteIpAddressAsync()
        {
            if (NetworkInterface.GetIsNetworkAvailable() == false)
                return null;

            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetStringAsync("http://checkip.dyndns.org");
                return response.Split(':')[1].Substring(1).Split('<')[0];
            }
            catch (Exception)
            {
                //Nếu ko connect dc mạng hoặc web check IP chết
                return null;
            }
        }

        public static IPAddress GetRemoteIpAddress(HttpRequest request)
        {
            return request.HttpContext.Connection.RemoteIpAddress;
        }
        
        public static string GetUserAgent(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("User-Agent"))
                return null;

            string userAgent = "";
            var headers = request.Headers["User-Agent"];

            StringBuilder sb = new StringBuilder();

            foreach (var header in headers)
            {
                sb.Append(header);

                // Re-add spaces stripped when user agent string was split up.
                sb.Append(" ");
            }

            userAgent = sb.ToString().Trim();
            return userAgent;
        }
    }
}
