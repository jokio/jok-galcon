using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Jok.StarWars.GameServer.Models
{
    public class Planet
    {
        public Guid ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }
        [IgnoreDataMember]
        public int ShipCount { get; set; }
        public int GroupID { get; set; }
    }
}