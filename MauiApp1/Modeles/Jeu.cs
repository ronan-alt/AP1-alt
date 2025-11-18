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

        #endregion
        #region constructeur
        #endregion
        #region getter/setter
        public int Id { get => _id; set => _id = value; }
        public string Nom { get => _nom; set => _nom = value; }
        public TypeJeu LeTypeJeu { get => _leTypeJeu; set => _leTypeJeu = value; }
        public Statut LeStatut { get => _leStatut; set => _leStatut = value; }
        #endregion
        #region methode
        #endregion
    }
}
