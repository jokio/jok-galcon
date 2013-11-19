﻿using Jok.Galcon.Common;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jok.Galcon.GameServer
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


        public static void GameEnd(ICallback to, int winnerId)
        {
            var conns = GetUsers(to);
            if (conns == null) return;

            Hub.Clients.Clients(conns).GameEnd(winnerId);
        }

        public static void RestartGame(ICallback to)
        {
            var conns = GetUsers(to);
            if (conns == null) return;

            Hub.Clients.Clients(conns).RestartGame(0);
        }

        public static void PlayerState(ICallback to, GamePlayer[] pl)
        {
            var conns = GetUsers(to);
            if (conns == null) return;

            Hub.Clients.Clients(conns).PlayerState(pl);
        }
    }
}