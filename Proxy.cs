using System;
using System.IO;
using System.Linq;
using RegistracjaTEST;
namespace RegistracjaTEST
{
    public class Proxy
    {
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RequiresAuthentication { get; set; }

        public static Proxy GetRandomProxy()
        {
            var proxies = File.ReadAllLines("proxies.txt")
                              .Where(line => !string.IsNullOrWhiteSpace(line))
                              .ToList();

            if (proxies.Count == 0)
            {
                throw new InvalidOperationException("Nie znaleziono proxy w pliku proxies.txt.");
            }

            var random = new Random();
            var proxyLine = proxies[random.Next(proxies.Count)];

            var parts = proxyLine.Split(':');
            if (parts.Length == 2)
            {
                return new Proxy
                {
                    Address = $"{parts[0]}:{parts[1]}",
                    RequiresAuthentication = false
                };
            }
            else if (parts.Length == 4)
            {
                return new Proxy
                {
                    Address = $"{parts[0]}:{parts[1]}",
                    Username = parts[2],
                    Password = parts[3],
                    RequiresAuthentication = true
                };
            }
            else
            {
                throw new FormatException("Nieprawidłowy format proxy w pliku proxies.txt.");
            }
        }
    }
}