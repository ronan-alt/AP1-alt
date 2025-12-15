using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Modeles
{
    public class LieuQR
    {
        public string Indice { get; set; }        // Ex: "Cherche près de la machine à café"
        public string CodeSecret { get; set; }    // Ce qui est écrit dans le QR Code (Ex: "CAFE_01")
        public int DureeSecondes { get; set; }    // Temps pour le trouver (Ex: 60 secondes)
    }
}
