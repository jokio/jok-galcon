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
        [DataMember]
        public int LastWinner { get; set; }

        private IJokTimer<object> ShipsAddTimer = JokTimer<object>.Create();

        private const int NumberOfPlanets = 20;

        private Random randomGenerator = new Random();
        #endregion


        public void Move(int userid, List<Guid> from, Guid to, int percent)
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
                        AddPlayer(player);
                        //Players.Add(player);
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

        protected void OnMove(GamePlayer player, List<Guid> from, Guid to, int percent)
        {
            var count = 0;
            var startPlanets = Planets.Where(c => from.Contains(c.ID) && c.GroupID  == player.GroupID && c.ShipCount > 0).ToList();
            var startPlanetIDs = startPlanets.Select(c=>c.ID).ToList();
            var targetPlanet = Planets.FirstOrDefault(c => c.ID == to);

            if (!startPlanets.Any() || targetPlanet == null) return;
            count = startPlanets.Sum(c => c.ShipCount) * percent / 100;
            Planets.ForEach(c =>
            {
                if (startPlanetIDs.Contains(c.ID))
                {
                    c.ShipCount -= c.ShipCount * percent / 100;
                }
            });

            if (player.GroupID == targetPlanet.GroupID)
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
            UpdatePlanetsState();

            if (CheckFinishGame())
            {
                FinishGame();
            }
        }

        protected void OnPlayAgain(GamePlayer player)
        {
            Init();
            Status = TableStatus.Started;
            var opponent = GetNextPlayer(player);
            GameCallback.TableState(player, this);
            GameCallback.TableState(opponent, this);
        }


        void Init()
        {
            Status = TableStatus.New;
            Planets = new List<Planet>();

            for (var i = 0; i < NumberOfPlanets; i++)
            {
                var intercepts = true;
                while (intercepts)
                {
                    intercepts = false;
                    var x = randomGenerator.Next(50, 750);
                    var y = randomGenerator.Next(50, 500);
                    var r = randomGenerator.Next(20, 50);
                    var s = randomGenerator.Next(5, 70);
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
            var firstPlayerID = GetPlanetNearBorder(4);
            var secondPlayerID = GetPlanetNearBorder(2);
            
            var firstPlayerPlanet = Planets.FirstOrDefault(c=>c.ID == firstPlayerID);
            var secondPlayerPlanet = Planets.FirstOrDefault(c => c.ID == secondPlayerID);
            firstPlayerPlanet.GroupID = 1;
            secondPlayerPlanet.GroupID = 2;

            firstPlayerPlanet.ShipCount = randomGenerator.Next(0, 10) + 50;
            secondPlayerPlanet.ShipCount = randomGenerator.Next(0, 10) + 50;
            
            Players[0].GroupID = 1;
            Players[1].GroupID = 2;
            ShipsAddTimer.SetInterval(OnShipsAddTimer, null, 2000);
            Players.ForEach(p => p.Init());
        }

        bool CheckFinishGame()
        {
            if (Planets.Count(c => c.GroupID == 1) * Planets.Count(c => c.GroupID == 2) == 0)
            {
                var winnerGroupID = Planets.Count(c => c.GroupID == 1) > 0 ? 1 : 2;
                LastWinner = Players.FirstOrDefault(c => c.GroupID == winnerGroupID).UserID;
                return true;
            }
            return false;
        }

        void FinishGame()
        {
            ShipsAddTimer.Stop();
            Status = TableStatus.Finished;
            var winnerGroup = Planets.FirstOrDefault(c => c.GroupID != 0 && c.ShipCount > 0);
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

        double GetDistance(int x1, int y1, int x2, int y2)
        {
            var dx = x1 - x2;
            var dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        void OnShipsAddTimer(object obj)
        {
            Planets.ForEach(c =>
            {
                if (c.GroupID != 0)
                { 
                    c.ShipCount+= c.Radius / 10;
                }
            });

            UpdatePlanetsState();
        }

        void UpdatePlanetsState()
        {
            var planets = Planets.Select(c => new PlanetState
            {
                GroupID = c.GroupID,
                ID = c.ID,
                ShipCount = c.ShipCount
            }).ToList();
            GameCallback.UpdatePlanetsState(this, planets);
        }

        protected Guid? GetPlanetNearBorder(int border) //saatis isris mimartulebit: 1 - zeda Border, 2- marjvena, 3- qveda, 4- marcxena
        {
            if (!Planets.Any())
            {
                return null;
            }
            var top = Planets.Min(c => c.Y);
            var bottom = Planets.Max(c => c.Y);
            var left = Planets.Min(c=>c.X);
            var right = Planets.Max(c => c.X);
            var compareValues = new[] {-1, top, right, bottom, left };
            if (border % 2 == 1)
            {
                var planet = Planets.FirstOrDefault(c => c.Y == compareValues[border]);
                if (planet == null)
                {
                    return null;
                }
                return planet.ID;
            }
            else
            {
                var planet = Planets.FirstOrDefault(c => c.X == compareValues[border]);
                if (planet == null)
                {
                    return null;
                }
                return planet.ID;
            }
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