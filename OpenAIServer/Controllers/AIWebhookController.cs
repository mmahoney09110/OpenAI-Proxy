using Microsoft.AspNetCore.Mvc;
using OpenAI.Examples;
using System.Threading.Tasks;

namespace MaidenServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class WebhookController : ControllerBase
    {
        private readonly AiServiceVectorStore _AIService;
        private readonly ILogger _logger;

        public WebhookController(AiServiceVectorStore AIService, ILogger<WebhookController> logger)
        {
            _AIService = AIService;
            _logger = logger;
        }

        // Endpoint to handle incoming SMS messages
        [HttpPost("response")]
        public async Task<IActionResult> IncomingRequest([FromForm] string Body)
        {
            _logger.LogInformation("Incoming request received with Body: {Body}", Body);

            try
            {
                var response = await _AIService.GenerateResponseAsync(Body);
                _logger.LogInformation("AI response: {Response}", response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating response");
                return BadRequest(ex.Message);
            }
        }

        // Endpoint to handle incoming SMS messages
        [HttpPost("OpenRouter")]
        public async Task<IActionResult> IncomingRequestOR([FromForm] string Body, [FromForm] string Character = "0")
        {
            _logger.LogInformation("Incoming request received with Body: {Body}", Body);

            try
            {
                var response = await _AIService.GetORChatResponseAsync(Body, Character);
                _logger.LogInformation("AI response: {Response}", response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating response");
                return BadRequest(ex.Message);
            }
        }

        // Test endpoint callable from browser
        [HttpGet("OpenRouter/test")]
        public async Task<IActionResult> TestOpenRouter([FromQuery] string query, [FromQuery] string character = "0")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required (e.g., ?query=your+question)");
            }

            _logger.LogInformation("Test request received - Query: {Query}, Character: {Character}", query, character);

            try
            {
                var response = await _AIService.GetORChatResponseAsync(query, character);
                _logger.LogInformation("AI test response: {Response}", response);

                // Return plain text for easy browser viewing
                return Content(response, "text/plain");
                // Alternative: return Ok(new { query, character, aiResponse = response }); // JSON for structured output
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // Test endpoint callable from browser
        [HttpGet("test")]
        public async Task<IActionResult> keepAlive()
        {
            try
            {
                return Ok("Server is alive");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
