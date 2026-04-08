using AutoCarShowroom.Models;
using AutoCarShowroom.Services;
using AutoCarShowroom.Services.Chatbot;
using Microsoft.AspNetCore.Mvc;

namespace AutoCarShowroom.Controllers
{
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly ChatbotOrchestrator _chatbotOrchestrator;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            ChatbotOrchestrator chatbotOrchestrator,
            ILogger<ChatbotController> logger)
        {
            _chatbotOrchestrator = chatbotOrchestrator;
            _logger = logger;
        }

        [HttpPost("Ask")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Ask([FromBody] ChatbotRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ChatbotTextEncodingHelper.NormalizeReply(new ChatbotReply
                {
                    Message = "Anh/chị hãy nhập câu hỏi để em tư vấn mẫu xe phù hợp nhé."
                }));
            }

            try
            {
                return Ok(ChatbotTextEncodingHelper.NormalizeReply(await _chatbotOrchestrator.AskAsync(request)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chatbot request failed.");

                return Ok(ChatbotTextEncodingHelper.NormalizeReply(new ChatbotReply
                {
                    Message = DatabaseIssueHelper.IsDatabaseConnectivityIssue(ex)
                        ? "AI tư vấn hiện chưa truy cập được dữ liệu showroom vì kết nối cơ sở dữ liệu đang lỗi. Anh/chị vui lòng kiểm tra SQL Server rồi thử lại giúp em nhé."
                        : "AI tư vấn đang bận đồng bộ dữ liệu showroom. Anh/chị thử lại sau ít phút giúp em nhé."
                }));
            }
        }
    }
}
