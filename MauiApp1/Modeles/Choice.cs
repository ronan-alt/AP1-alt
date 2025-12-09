using System.Text.Json.Serialization;

namespace MauiApp1.Modeles;

public class Choice
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("texte")]
    public string Texte { get; set; }

    [JsonPropertyName("isCorrect")]
    public bool IsCorrect { get; set; }
}
