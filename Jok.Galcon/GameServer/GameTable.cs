using Jok.GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Jok.Galcon.GameServer
{
    public class GameTable : GameTableBase<GamePlayer>
    {
        #region Properties
        public override bool IsStarted
        {
            get { return Status == TableStatus.Started; }
        }

        public override bool IsFinished
        {
            get { return Status == TableStatus.Finished; }
        }

        private TableStatus Status { get; set; }
        #endregion


        public void Move(int userid, Guid from, Guid to, int percent)
        {
            var player = GetPlayer(userid);
            if (player == null) return;

            lock (SyncObject)
            {
                OnMove(player, from, to, percent);
            }
        }

        public void PlayAgain(int userid)
        {
            var player = GetPlayer(userid);
            if (player == null) return;

            lock (SyncObject)
            {
                OnPlayAgain(player);
            }
        }


        protected override void OnJoin(GamePlayer player, object state)
        {
            switch (Status)
            {

            }
        }

        protected override void OnLeave(GamePlayer player)
        {
            switch (Status)
            {

            }
        }

        protected void OnMove(GamePlayer player, Guid from, Guid to, int percent)
        {
            GameCallback.PlayerMove(Table, player.UserID, from, to, percent * 37, 1000 /* მანძილიდან გამომდინარე */);
        }

        protected void OnPlayAgain(GamePlayer player)
        {

        }


        void Init()
        {
            Status = TableStatus.New;
            Players.ForEach(p => p.Init());
        }


        enum TableStatus
        {
            New,
            Started,
            Stopped,
            Finished
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


        public void Init()
        {

        }
    }
}