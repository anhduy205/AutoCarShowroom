using Showroom.Web.Models;

namespace Showroom.Web.Services;

public interface IShowroomAssistantService
{
    Task<AiChatReplyViewModel> GetReplyAsync(string? message, CancellationToken cancellationToken = default);
}
