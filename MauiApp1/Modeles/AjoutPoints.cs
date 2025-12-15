using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MauiApp1.Services;
using Newtonsoft.Json;

namespace MauiApp1.Modeles
{
    public class AjoutPoints
    {
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("points")]
        public int nbPoints { get; set; }
        public AjoutPoints(int id, int nbPoints)
        {
            this.id = id;
            this.nbPoints = nbPoints;
        }
    }
}
