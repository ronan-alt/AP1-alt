
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP1.Modeles
{
    public class Equipe
    {
        #region propriete
        private int _id;
        private int _nbplaces;
        private string _nomEquipe;
        private List<User> _lesUsers;
        private User _leCapitaine;
        private Score _leScore;

        #endregion
        #region constructeur
        public Equipe(int id,string nomEquipe)
        {
           _id = id;
              _nomEquipe = nomEquipe;
              _lesUsers = new List<User>();
        }
        public Equipe()
        {
        }
        #endregion
        #region getter/setter
        public int Id { get => _id; set => _id = value; }
        public int Nbplaces { get => _nbplaces; set => _nbplaces = value; }
        public string NomEquipe { get => _nomEquipe; set => _nomEquipe = value; }
        public List<User> LesUsers { get => _lesUsers; set => _lesUsers = value; }
        public User LeCapitaine { get => _leCapitaine; set => _leCapitaine = value; }
        public Score LeScore { get => _leScore; set => _leScore = value; }
        #endregion
        #region methode
        #endregion

    }
}
