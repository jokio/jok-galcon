using Jok.StarWars.GameServer.Models;
using Jok.GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Jok.StarWars.GameServer
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
        [DataMember]
        public TableStatus Status { get; set; }
        [DataMember]
        public List<Planet> Planets { get; set; }

        private IJokTimer<object> Timer = JokTimer<object>.Create();

        private const int NumberOfPlanets = 20;

        #endregion

        private double GetDistance(int x1, int y1, int x2, int y2)
        {
            var dx = x1 - x2;
            var dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);            
        }

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
                case TableStatus.New :
                    if (!Players.Contains(player))
                    {
                        Players.Add(player);
                    }
                    
                    if (Players.Count() == 2)
                    {
                        Init();
                        Status = TableStatus.Started;
                        var opponent = GetNextPlayer(player);
                        GameCallback.TableState(player, this);
                        GameCallback.TableState(opponent, this);
                    }
                    else
                    {
                        GameCallback.TableState(player, this);
                    }
                    break;
                case TableStatus.Started :
                    GameCallback.TableState(this, this);
                    break;

            }
            
        }

        protected override void OnLeave(GamePlayer player)
        {
            switch (Status)
            {
                
            }
        }

        protected void OnMove(GamePlayer player, Guid from, Guid to, int count)
        {
            var startPlanet = Planets.FirstOrDefault(c => c.ID == from);
            var targetPlanet = Planets.FirstOrDefault(c => c.ID == to);
            if (startPlanet == null || targetPlanet == null)
            {
                return;
            }
            if (startPlanet.GroupID != player.GroupID)
            {
                return;
            }
            if (startPlanet.ShipCount < count)
            {
                count = startPlanet.ShipCount;
            }
            startPlanet.ShipCount -= count;
            if (startPlanet.GroupID == targetPlanet.GroupID)
            {
                targetPlanet.ShipCount += count;
            }
            else
            {
                targetPlanet.ShipCount -= count;
                if (targetPlanet.ShipCount < 0)
                {
                    targetPlanet.ShipCount = Math.Abs(targetPlanet.ShipCount);
                    targetPlanet.GroupID = player.GroupID;
                }
            }
            GameCallback.PlayerMove(Table, player.UserID, from, to, count, 0 /* მანძილიდან გამომდინარე */);
            var gameIsOver = CheckFinishGame();
            if (gameIsOver)
            {
                FinishGame();
            }
        }

        protected void OnPlayAgain(GamePlayer player)
        {
            Status = TableStatus.Started;
            Init();
            var opponent = GetNextPlayer(player);
            GameCallback.TableState(player, this);
            GameCallback.TableState(opponent, this);

        }

        private bool CheckFinishGame()
        {
            if (Planets.Count(c => c.GroupID == 1) * Planets.Count(c=>c.GroupID == 2) == 0)
            {
                return true;
            }
            return false;
        }

        void Init()
        {
            Status = TableStatus.New;
            Planets = new List<Planet>();
            var randomGenerator = new Random();

            for (var i = 0; i < NumberOfPlanets; i++)
            {
                var intercepts = true;
                while (intercepts)
                {
                    intercepts = false;
                    var x = randomGenerator.Next(50, 750);
                    var y = randomGenerator.Next(50, 500);
                    var r = randomGenerator.Next(20, 50);
                    var s = randomGenerator.Next(30, 100);
                    for (var j = 0; j < i; j++)
                    {
                        var distance = GetDistance(x, y, Planets[j].X, Planets[j].Y);
                        if (Math.Abs(distance - r) < 60)
                        {
                            intercepts = true;
                            break;
                        }
                    }
                    if (!intercepts)
                    {
                        Planets.Add(new Planet
                        {
                            X = x,
                            Y = y,
                            ID = Guid.NewGuid(),
                            GroupID = 0,
                            ShipCount = s,
                            Radius = r
                        });
                    }
                }
            }
            var firstPlayer = randomGenerator.Next(0, NumberOfPlanets);
            var secondPlayer = randomGenerator.Next(0, NumberOfPlanets);
            while (secondPlayer == firstPlayer)
            {
                secondPlayer = randomGenerator.Next(0, NumberOfPlanets);
            }
            Planets[firstPlayer].GroupID = 1;
            Planets[secondPlayer].GroupID = 2;
            Timer.SetInterval(AddShips, null, 2000);
            Players.ForEach(p => p.Init());
        }

        private void AddShips(object _object)
        {
            Planets.ForEach(c =>
            {
                if (c.GroupID != 0)
                { 
                    c.ShipCount+= c.Radius / 10;
                }
            });

        }

        private void FinishGame()
        {
            Timer.Stop();
            Status = TableStatus.Finished;
            var winnerGroup = Planets.FirstOrDefault(c=>c.GroupID != 0 && c.ShipCount > 0);
            if (winnerGroup != null)
            {
                Players.ForEach(c =>
                {
                    if (c.GroupID == winnerGroup.GroupID)
                    {
                        c.WinCount++;
                    }
                });
            }
            GameCallback.TableState(this, this);
        }

        public enum TableStatus
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
        [DataMember]
        public int GroupID { get; set; }
        [DataMember]
        public int WinCount { get; set; }

        public void Init()
        {

        }
    }
}