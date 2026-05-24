using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Scratch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var apiKey = "YOUR_API_KEY_HERE"; // Need to get this from secrets or env
            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine(content);
        }
    }
}
