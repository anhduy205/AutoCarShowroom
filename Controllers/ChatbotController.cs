using AutoCarShowroom.Models;
using AutoCarShowroom.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoCarShowroom.Controllers
{
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly ShowroomChatbotService _chatbotService;

        public ChatbotController(ShowroomChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("Ask")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Ask([FromBody] ChatbotRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatbotReply
                {
                    Message = "Bạn hãy nhập câu hỏi để mình tư vấn mẫu xe phù hợp."
                });
            }

            try
            {
                return Ok(await _chatbotService.AskAsync(request.Message));
            }
            catch (Exception)
            {
                return Ok(new ChatbotReply
                {
                    Message = "AI tư vấn đang bận đồng bộ dữ liệu showroom. Bạn thử hỏi lại sau ít phút nhé."
                });
            }
        }
    }
}
