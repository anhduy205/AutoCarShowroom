namespace Showroom.Web.Models;

public class AiChatReplyViewModel
{
    public string Reply { get; init; } = string.Empty;

    public IReadOnlyList<string> Suggestions { get; init; } = Array.Empty<string>();
}
