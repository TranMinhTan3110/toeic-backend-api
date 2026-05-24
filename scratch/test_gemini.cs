using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = "YOUR_API_KEY_HERE"; // User should replace this or I should use it from secrets
        var url = "https://generativelanguage.googleapis.com/v1beta/models?key=" + apiKey;
        
        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine(content);
    }
}
