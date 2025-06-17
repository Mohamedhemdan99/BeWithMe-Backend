using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
namespace BeWithMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SyncController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SyncController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] SyncGenerateRequest request)
        {
            var client = _httpClientFactory.CreateClient();

            var syncApiKey = _configuration["SyncSo:ApiKey"];
            if (string.IsNullOrEmpty(syncApiKey))
            {
                return StatusCode(500, new { error = "Missing Sync.so API key" });
            }

            var apiUrl = "https://api.sync.so/v2/generate";

            var jsonContent = JsonSerializer.Serialize(request);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = httpContent
            };

            httpRequest.Headers.Add("x-api-key", syncApiKey);
            if (request.Input == null || request.Input.Count < 2)
            {
                return BadRequest("Input array must contain both video and audio sources");
            }
            try
            {
                var response = await client.SendAsync(httpRequest);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        error = $"Sync.so API returned status code {(int)response.StatusCode}",
                        details = responseBody
                    });
                }

                var result = JsonSerializer.Deserialize<SyncGenerateResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to call Sync.so API", details = ex.Message });
            }
        }

        [HttpGet("generate/{id}")]
        public async Task<IActionResult> CheckStatus(string id)
        {
            var client = _httpClientFactory.CreateClient();
            var syncApiKey = _configuration["SyncSo:ApiKey"];

            if (string.IsNullOrEmpty(syncApiKey))
            {
                return StatusCode(500, new { error = "Missing Sync.so API key" });
            }

            var apiUrl = $"https://api.sync.so/v2/generate/{id}";

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                httpRequest.Headers.Add("x-api-key", syncApiKey);

                var response = await client.SendAsync(httpRequest);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        error = $"Sync.so API returned status code {(int)response.StatusCode}",
                        details = responseBody
                    });
                }

                var result = JsonSerializer.Deserialize<SyncGenerateResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to check status", details = ex.Message });
            }
        }

    }

    public class SyncInput
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }


    public class SyncOptions
    {
        [JsonPropertyName("pads")]
        public int[] Pads { get; set; } = [0, 5, 0, 0];

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.5;

        [JsonPropertyName("output_resolution")]
        public int[] OutputResolution { get; set; } = [1280, 720];

        [JsonPropertyName("output_format")]
        public string OutputFormat { get; set; } = "mp4";

        [JsonPropertyName("sync_mode")]
        public string SyncMode { get; set; } = "loop"; // Changed from "cut_off" to match your code
    }


    public class SyncGenerateRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "lipsync-1.7.1";

        [JsonPropertyName("input")]
        public List<SyncInput> Input { get; set; }

        [JsonPropertyName("options")]
        public SyncOptions Options { get; set; } = new SyncOptions();
    }


    public class SyncGenerateResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string OutputUrl { get; set; }
        public string Error { get; set; }
    }
}
