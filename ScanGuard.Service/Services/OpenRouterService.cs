using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MGOBankApp.BLL.Services
{
    public class OpenRouterService
    {
        private const string apiKey = "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"; // Replace with your real key

        public OpenRouterService()
        {
        }

        public async Task<string> GetAnalysisAsync(string scanResults)
        {
            try
            {
                var client = new RestClient("https://openrouter.ai/api/v1/chat/completions");
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Authorization", $"Bearer {apiKey}");
                request.AddHeader("Content-Type", "application/json");

                var body = new
                {
                    model = "openai/gpt-3.5-turbo", // Use a cheaper model
                    messages = new[]
                    {
        new { role = "system", content = "You are a cybersecurity expert. Provide a short security analysis." },
        new { role = "user", content = scanResults.Substring(0, Math.Min(scanResults.Length, 1000)) } // Limit input length
    },
                    max_tokens = 500 // Reduce response size
                };

                request.AddJsonBody(JsonConvert.SerializeObject(body));

                var response = await client.ExecuteAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                {
                    return $"API Error: {response.StatusCode} - {response.Content}";
                }

                // Debug: Log full API response
                Console.WriteLine("API Response: " + response.Content);

                // Parse JSON response safely
                var jsonResponse = JObject.Parse(response.Content);
                var messageContent = jsonResponse["choices"]?.First?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(messageContent))
                {
                    return $"API Response Error: {response.Content}";
                }

                return messageContent;
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }


    }
}
