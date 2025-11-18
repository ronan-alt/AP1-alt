using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP1.Modeles
{
    public class Professeur
    {
        #region propriete
        private int _id;
        private List<Competition> _lesCompetitions;

        #endregion
        #region constructeur

        #endregion
        #region getter/setter
        public int Id { get => _id; set => _id = value; }
        public List<Competition> LesCompetitions { get => _lesCompetitions; set => _lesCompetitions = value; }
        #endregion
        #region methode
        public void AjouterCompetition(Competition competition)
        {
            _lesCompetitions.Add(competition);
        }

        // Afficher les informations
        public override string ToString()
        {
            return $"Professeur {_id}- {_lesCompetitions.Count} compétitions";
        }
        #endregion
    }
}
