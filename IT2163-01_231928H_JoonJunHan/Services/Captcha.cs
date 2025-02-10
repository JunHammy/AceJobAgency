using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace IT2163_01_231928H_JoonJunHan.Services
{
    public class Captcha
    {
        private readonly HttpClient _httpClient;
        private readonly string _recaptchaSecretKey;

        public Captcha(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _recaptchaSecretKey = configuration["Captcha:reCaptchaKey"];
        }

        public async Task<bool> ValidateCaptchaAsync(string recaptchaResponse)
        {
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("Request data: " + recaptchaResponse);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _recaptchaSecretKey),
                new KeyValuePair<string, string>("response", recaptchaResponse)
            });

            try
            {
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                Console.WriteLine("Response: " + response);

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response String: " + responseString);

                // Extract the success value directly from the response
                var jsonDocument = JsonDocument.Parse(responseString);
                var successValue = jsonDocument.RootElement.GetProperty("success").GetBoolean();

                Console.WriteLine("Success = " + successValue);

                return successValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }
    }
}
