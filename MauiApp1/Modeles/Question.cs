using System.Text.Json.Serialization;

namespace MauiApp1.Modeles;

public class Question
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("enonce")]
    public string Enonce { get; set; }

    [JsonPropertyName("dureeSecondes")]
    public int DureeSecondes { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();
}
