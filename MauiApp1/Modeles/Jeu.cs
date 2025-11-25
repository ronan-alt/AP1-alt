using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP1.Modeles
{
    public class Jeu
    {
        #region propriete
        private int _id;
        private string _nom;
        private TypeJeu _leTypeJeu;
        private Statut _leStatut;
        private DateTime _dateDebut;
        private int _duree;

        #endregion
        #region constructeur
        public Jeu()
        {
        }
        public Jeu(int id,string nom,DateTime datedebut,int duree)
        {
            _id = id;
            _nom = nom;
            _dateDebut = datedebut;
            _duree = duree;
        }
        #endregion
        #region getter/setter
        [JsonProperty("id")]
        public int Id { get => _id; set => _id = value; }
        [JsonProperty("nom")]
        public string Nom { get => _nom; set => _nom = value; }
        public TypeJeu LeTypeJeu { get => _leTypeJeu; set => _leTypeJeu = value; }
        public Statut LeStatut { get => _leStatut; set => _leStatut = value; }
        [JsonProperty("dateDebut")]
        public DateTime DateDebut { get => _dateDebut; set => _dateDebut = value; }
        [JsonProperty("dureeMax")]
        public int Duree { get => _duree; set => _duree = value; }
        #endregion
        #region methode
        #endregion
    }
}
