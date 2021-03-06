﻿using Jok.GameEngine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Jok.StarWars.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Play");
        }

        public ActionResult Play(string id, string sid)
        {
            var loginUrl = ConfigurationManager.AppSettings["jok:SiteUrl"] + "/joinus?returnUrl=" + Request.Url;
            var exitUrl = ConfigurationManager.AppSettings["jok:SiteUrl"] + "/lobby/starwars";

            if (!String.IsNullOrEmpty(sid))
            {
                var cookie = Request.Cookies["sid"];
                if (cookie == null)
                    cookie = new HttpCookie("sid", sid);

                cookie.Value = sid;
                cookie.Expires = DateTime.UtcNow.AddYears(30);

                if (Request.Url.Host.Contains('.'))
                    cookie.Domain = Request.Url.Host.Substring(Request.Url.Host.IndexOf('.'));

                Response.Cookies.Add(cookie);
            }
            else
            {
                sid = Request.Cookies["sid"] == null ? "" : Request.Cookies["sid"].Value;
            }

            var userInfo = JokAPI.GetUser(sid, Request.UserHostAddress);
            if (userInfo == null || !userInfo.IsSuccess)
                return Redirect(loginUrl);

            //if (!String.IsNullOrEmpty(userInfo.CultureName))
            //{
            //    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(userInfo.CultureName);
            //    ViewBag.Language = userInfo.CultureName.Replace('-', '_');
            //}

            ViewBag.SID = sid;
            ViewBag.Channel = id;
            ViewBag.AuthorizationUrl = loginUrl;
            ViewBag.ExitUrl = exitUrl;

            return View();
        }
    }
}