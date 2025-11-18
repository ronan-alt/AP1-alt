using AP1.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AP1.Modeles
{
    public class User
    {
        #region propriete (champs privés d'origine)
        private int _id;
        private string _nom;
        private string _prenom;
        private string _password;
        private string _email;
        private bool _statut;
        private Equipe _laEquipe;
        private Apis _apis;
        // Ajouts discrets pour refléter le JSON
        private string _userIdentifier;
        private List<string> _roles;
        #endregion

        #region constructeurs d'origine


        public User(string email, string password, string nom, string prenom) // register
        {
            _email = email;
            _password = password;
            _nom = nom;
            _prenom = prenom;
            _laEquipe = new Equipe();
            _apis = new Apis();
            _roles = new List<string>();
            _userIdentifier = string.Empty;
        }
        public User(string email, string password) //login
        {
            _email = email;
            _password = password;
            Equipe equipe = new Equipe();

        }
        #endregion

        #region constructeur JSON (nouveau)
        /// <summary>
        /// Constructeur utilisé par Newtonsoft.Json pour désérialiser le JSON fourni.
        /// Les noms de paramètres DOIVENT correspondre aux clés du JSON (minuscules).
        /// Les paramètres non présents dans le JSON sont optionnels.
        /// </summary>
        [JsonConstructor]
        public User(
            [JsonProperty("id")] int id,
            [JsonProperty("email")] string email,
            [JsonProperty("userIdentifier")] string userIdentifier = null,
            [JsonProperty("roles")] List<string> roles = null,
            [JsonProperty("password")] string password = null,
            [JsonProperty("nom")] string nom = null,
            [JsonProperty("prenom")] string prenom = null,
            [JsonProperty("statut")] bool statut = false,
            [JsonProperty("latEquipe")] Equipe latEquipe = null
        )
        {
            _id = id;
            _email = email;
            _userIdentifier = userIdentifier ?? string.Empty;
            _roles = roles ?? new List<string>();
            _password = password ?? string.Empty;
            _nom = nom ?? string.Empty;
            _prenom = prenom ?? string.Empty;
            _statut = statut;

            // Conserver l'esprit de la classe d'origine
            _laEquipe = new Equipe();
            _apis = new Apis();
        }
        #endregion

        #region getter/setter (avec mapping JSON précis)

        [JsonProperty("id")]
        public int Id { get => _id; set => _id = value; }

        [JsonProperty("statut")]
        public bool Statut { get => _statut; set => _statut = value; }

        public Equipe LaEquipe { get => _laEquipe; set => _laEquipe = value; }
        public Apis Apis { get => _apis; set => _apis = value; }

        // IMPORTANT : les noms doivent matcher le JSON en minuscules
        [JsonProperty("Email")]
        public string Email { get => _email; set => _email = value; }

        [JsonProperty("Password")]
        public string Password { get => _password; set => _password = value; }

        [JsonProperty("Nom")]
        public string Nom { get => _nom; set => _nom = value; }

        [JsonProperty("Prenom")]
        public string Prenom { get => _prenom; set => _prenom = value; }

        [JsonProperty("userIdentifier")]
        public string UserIdentifier
        {
            get => _userIdentifier;
            set => _userIdentifier = value;
        }

        [JsonProperty("roles")]
        public List<string> Roles
        {
            get => _roles ??= new List<string>();
            set => _roles = value ?? new List<string>();
        }

        #endregion

        #region methode
        // Tes méthodes métier ici (inchangé)
        #endregion
    }
}
