using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;



namespace MauiApp1.Modeles
{
    public class ReponseEnvoyee
    {
        public int userId { get; set; }
        public int choiceId { get; set; }
        public int responseTimeMs { get; set; }
    }
}

