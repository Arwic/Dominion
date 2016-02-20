// Dominion - Copyright (C) Timothy Ings
// NetHelper.cs
// This file contains helper classes for networks

using ArwicEngine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ArwicEngine.Net
{
    public static class NetHelper
    {
        /// <summary>
        /// Gets the public address of this computer
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetPublicIPAsync()
        {
            string url = "http://checkip.dyndns.org";
            WebRequest req = WebRequest.Create(url);
            WebResponse resp = await req.GetResponseAsync();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }

        /// <summary>
        /// Returns the ip address of the given host name
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static string DnsResolve(string host)
        {
            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(host).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
            if (ipv4Addresses.Length > 0)
                return ipv4Addresses[0].ToString();
            return "";
        }
    }
}
