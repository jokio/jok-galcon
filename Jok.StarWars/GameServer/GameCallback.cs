using Jok.GameEngine;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jok.StarWars.GameServer.Models;

namespace Jok.StarWars.GameServer
{
    public class GameCallback
    {
        #region Cool Stuff (dont open :P)
        static IHubContext Hub = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

        public string ConnectionID { get; set; }

        public dynamic Callback
        {
            get { return Hub.Clients.Client(ConnectionID); }
        }

        static IList<string> GetUsers(ICallback to, params ICallback[] exclude)
        {
            if (to == null) return null;

            var result = new List<string>();
            var ignoreList = new List<string>();

            exclude.ToList().ForEach(i1 => i1.ConnectionIDs.ForEach(ignoreList.Add));

            foreach (var item in to.ConnectionIDs)
            {
                if (!ignoreList.Contains(item))
                    result.Add(item);
            }

            if (result.Count == 0)
                return null;

            return result;
        }
        #endregion



        public static void PlayerMove(ICallback to, int userid, List<Guid> fromObject, Guid toObject, int shipsCount, int animationDuration)
        {
            var conns = GetUsers(to);
            if (conns == null) return;

            Hub.Clients.Clients(conns).PlayerMove(userid, fromObject, toObject, shipsCount, animationDuration);
        }

        public static void TableState(ICallback to, GameTable table)
        {
            var conns = GetUsers(to);
            if (conns == null) return;
            Hub.Clients.Clients(conns).TableState(table);
        }

        public static void UpdatePlanetsState(ICallback to, List<PlanetState> planets)
        {
            var conns = GetUsers(to);
            if (conns == null) return;
            Hub.Clients.Clients(conns).UpdatePlanetsState(planets);
        }
    }
}