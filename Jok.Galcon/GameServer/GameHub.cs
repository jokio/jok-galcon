using Jok.GameEngine;
using Jok.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jok.Galcon.GameServer
{
    public class GameHub : GameHubBase<GameTable>
    {
        #region Custom Authentication
        // ტესტირებისთვის არის ეს მხოლოდ საჭირო, რეალურ სერვერზე რომ არ შეამოწმოს ინფო
        protected override JokUserInfo GetUserInfo(string token, string ipaddress)
        {
            var userInfo = new JokUserInfo
            {
                IsSuccess = true,
                UserID = Convert.ToInt32(token),
                Nick = token,
                IsVIP = false
            };

            return userInfo;
        }
        #endregion


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