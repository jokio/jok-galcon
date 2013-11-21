using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jok.StarWars.GameServer.Models
{
    public class PlanetState
    {
        public Guid ID { get; set; }
        public int ShipCount { get; set; }
        public int GroupID { get; set; }
    }
}