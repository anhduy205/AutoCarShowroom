namespace Showroom.Web.Configuration;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = "gemini-2.5-flash";

    public string Endpoint { get; init; } = "https://generativelanguage.googleapis.com";
}
