using AutoCarShowroom.Models;
using AutoCarShowroom.Services.Chatbot;
using Microsoft.AspNetCore.Mvc;

namespace AutoCarShowroom.Controllers
{
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly ChatbotOrchestrator _chatbotOrchestrator;

        public ChatbotController(ChatbotOrchestrator chatbotOrchestrator)
        {
            _chatbotOrchestrator = chatbotOrchestrator;
        }

        [HttpPost("Ask")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Ask([FromBody] ChatbotRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatbotReply
                {
                    Message = "Anh/chị hãy nhập câu hỏi để em tư vấn mẫu xe phù hợp nhé."
                });
            }

            try
            {
                return Ok(await _chatbotOrchestrator.AskAsync(request));
            }
            catch (Exception)
            {
                return Ok(new ChatbotReply
                {
                    Message = "AI tư vấn đang bận đồng bộ dữ liệu showroom. Anh/chị thử lại sau ít phút giúp em nhé."
                });
            }
        }
    }
}
