using Jok.Galcon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Jok.Galcon.GameServer
{
    public class GameTable : GameTableBase<GamePlayer>
    {
        public override bool IsStarted
        {
            get { return false; }
        }

        public override bool IsFinished
        {
            get { return false; }
        }


        protected override void OnJoin(GamePlayer player, object state)
        {
        }

        protected override void OnLeave(GamePlayer player)
        {
        }
    }


    [DataContract]
    public class GamePlayer : IGamePlayer
    {
        [DataMember]
        public string IPAddress { get; set; }
        [DataMember]
        public bool IsVIP { get; set; }
        [DataMember]
        public bool IsOnline { get; set; }
        [IgnoreDataMember]
        public List<string> ConnectionIDs { get; set; }
        [DataMember]
        public int UserID { get; set; }
    }
}