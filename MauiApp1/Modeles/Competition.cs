using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP1.Modeles
{
    public class Competition
    {
        #region propriete
        private int _id;
        private string _nom;
        private DateTime _dateDeb;
        private DateTime _dateFin;
        private List<Score> _lesScores;
        private Professeur _leProfesseur;

        #endregion
        #region constructeur
        #endregion
        #region getter/setter
        public int Id { get => _id; set => _id = value; }
        public string Nom { get => _nom; set => _nom = value; }
        public DateTime DateDeb { get => _dateDeb; set => _dateDeb = value; }
        public DateTime DateFin { get => _dateFin; set => _dateFin = value; }
        public List<Score> LesScores { get => _lesScores; set => _lesScores = value; }
        public Professeur LeProfesseur { get => _leProfesseur; set => _leProfesseur = value; }
        #endregion
        #region methode
        #endregion
    }
}
