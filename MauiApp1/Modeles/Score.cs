using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP1.Modeles
{
    public class Score
    {
        #region propriete
        private int _id;
        private Equipe _leEquipe;
        private Competition _laCompetition;
        private Jeu _lejeu;
        private TypeJeu _leTypeJeu;

        #endregion
        #region constructeur
        #endregion
        #region getter/setter
        public int Id { get => _id; set => _id = value; }
        public Equipe LeEquipe { get => _leEquipe; set => _leEquipe = value; }
        public Competition LaCompetition { get => _laCompetition; set => _laCompetition = value; }
        public Jeu Lejeu { get => _lejeu; set => _lejeu = value; }
        public TypeJeu LeTypeJeu { get => _leTypeJeu; set => _leTypeJeu = value; }
        #endregion
        #region methode
        #endregion

    }
}
