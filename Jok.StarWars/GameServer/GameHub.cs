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
        public void Move(List<Guid> from, Guid to, int percent)
        {
            var user = GetCurrentUser();
            if (user == null) return;

            user.Table.Move(user.UserID, from, to, percent);
        }

        public void PlayAgain()
        {
            var user = GetCurrentUser();
            if (user == null) return;

            user.Table.PlayAgain(user.UserID);
        }
    }
}