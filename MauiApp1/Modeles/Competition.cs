using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AP1.Modeles
{
    public class Competition
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nom")]
        public string Nom { get; set; } = string.Empty;

        // Pièges pour capturer le nom sous d'autres formes
        [JsonProperty("libelle")]
        public string Libelle { set { if (!string.IsNullOrWhiteSpace(value)) Nom = value; } }

        [JsonProperty("name")]
        public string Name { set { if (!string.IsNullOrWhiteSpace(value)) Nom = value; } }

        [JsonProperty("label")]
        public string Label { set { if (!string.IsNullOrWhiteSpace(value)) Nom = value; } }

        [JsonProperty("dateDebut")]
        public DateTime DateDeb { get; set; }

        [JsonProperty("dateFin")]
        public DateTime DateFin { get; set; }

        [JsonProperty("teamsCount")]
        public int TeamsCount { get; set; }

        [JsonProperty("teams")]
        public List<Equipe> Teams { get; set; } = new();

        [JsonIgnore]
        public List<Score> LesScores { get; set; } = new();

        [JsonIgnore]
        public Professeur LeProfesseur { get; set; } = new();

        [JsonExtensionData]
        public Dictionary<string, object> ExtraData { get; set; } = new();

        public void FixNameFromExtraData()
        {
            if (!string.IsNullOrWhiteSpace(Nom)) return;
            
            // On cherche d'abord dans ExtraData si disponible
            if (ExtraData != null && ExtraData.Count > 0)
            {
                string[] candidates = { "libelle", "name", "title", "label", "nomCompetition", "nom_competition" };
                foreach (var key in candidates)
                {
                    var match = ExtraData.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
                    if (match != null && ExtraData[match] != null)
                    {
                        Nom = ExtraData[match].ToString() ?? "";
                        return;
                    }
                }
                // Si on a des données mais pas de nom connu, on affiche les clés
                Nom = "KEYS: " + string.Join(", ", ExtraData.Keys);
            }
            else
            {
                // Si vraiment rien, on met un placeholder
                Nom = $"Compétition #{Id} (Nom manquant)";
            }
        }
    }

    public class CompetitionListResponse
    {
        [JsonProperty("count")]
        public int TotalCount { get; set; }

        [JsonProperty("activeCount")]
        public int InProgressCount { get; set; }

        [JsonProperty("upcomingCount")]
        public int UpcomingCount { get; set; }

        [JsonProperty("competitions")]
        public List<Competition> Competitions { get; set; } = new();
    }

    public class CompetitionUpsertRequest
    {
        [JsonProperty("nom")]
        public string Nom { get; set; } = string.Empty;

        [JsonProperty("dateDebut")]
        public DateTime DateDeb { get; set; }

        [JsonProperty("dateFin")]
        public DateTime DateFin { get; set; }

        [JsonProperty("libelle")]
        public string Libelle => Nom;
    }

    public class TeamLinkRequest
    {
        [JsonProperty("teamId")]
        public int TeamId { get; set; }
    }

    public class CompetitionTeamsResponse
    {
        [JsonProperty("competition")]
        public Competition? Competition { get; set; }

        [JsonProperty("teams")]
        public List<Equipe> Teams { get; set; } = new();

        [JsonProperty("teamCount")]
        public int TeamCount { get; set; }
    }

    public class CompetitionCreationResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("competition")]
        public Competition? Competition { get; set; }
    }
}