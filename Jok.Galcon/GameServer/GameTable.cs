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
            get { throw new NotImplementedException(); }
        }

        public override bool IsFinished
        {
            get { throw new NotImplementedException(); }
        }


        protected override void OnJoin(GamePlayer player, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnLeave(GamePlayer player)
        {
            throw new NotImplementedException();
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