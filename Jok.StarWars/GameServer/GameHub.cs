using Jok.GameEngine;
using Jok.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jok.StarWars.GameServer
{
    public class GameHub : GameHubBase<GameTable>
    {
        public void Move(Guid from, Guid to, int count)
        {
            var user = GetCurrentUser();
            if (user == null) return;

            user.Table.Move(user.UserID, from, to, count);
        }

        public void PlayAgain()
        {
            var user = GetCurrentUser();
            if (user == null) return;

            user.Table.PlayAgain(user.UserID);
        }
    }
}