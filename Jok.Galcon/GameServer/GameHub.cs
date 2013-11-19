using Jok.Galcon.Common;
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

    }
}