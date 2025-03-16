using RestSharp;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;


namespace MGOBankApp.BLL.Services
{
    public class OpenRouterService
    {
        private const string apiKey = "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"; // Replace with your key

        public OpenRouterService()
        {
        }

        public async Task<string> GetAnalysisAsync(string scanResults)
        {
            var client = new RestClient("https://openrouter.ai/api/v1/chat/completions");
            var request = new RestRequest("", Method.Post); // Empty resource since base URL is set in RestClient
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                model = "openai/gpt-4-turbo",
                messages = new[]
                {
            new { role = "system", content = "You are a cybersecurity expert. Analyze the following scan results and provide a security report with recommendations. Why that vulnerabilities occur and how to disable them" },
            new { role = "user", content = scanResults }
        }
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
                return jsonResponse.choices[0].message.content;
            }
            else
            {
                Console.WriteLine($"API Error: {response.Content}"); // Log error for debugging
                return $"Error: Unable to process the request. Details: {response.Content}";
            }
        }
    }
}
