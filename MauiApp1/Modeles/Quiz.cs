using System.Text.Json.Serialization;

namespace MauiApp1.Modeles;

public class Quiz
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("titre")]
    public string Titre { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; } = new();
}
