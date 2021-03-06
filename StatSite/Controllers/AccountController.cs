﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using LOLSA.Models;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using LolApiDriver;
using LolApiDriver.Utility;


namespace LOLSA.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Register()
        {
            Register register = new Register();
            register.CreateSummonerInfos(1);
            return View(register);
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public JsonResult checkUserName(string Username)
        {
            var user = Membership.GetUser(Username);
            return Json(user == null);
        }

        [HttpPost]
        public JsonResult checkEmail(string Email)
        {
            var user = Membership.GetUserNameByEmail(Email);
            return Json(user == null);
        }

        [HttpPost]
        public ActionResult ValidateUser(Login login)
        {
            if (ModelState.IsValid)
            {
                LoginStatus status = new LoginStatus();
                if (Membership.ValidateUser(login.Username, login.Password))
                {
                    FormsAuthentication.SetAuthCookie(login.Username, login.RememberMe);
                    status.Success = true;
                    status.TargetURL = FormsAuthentication.
                                       GetRedirectUrl(login.Username, login.RememberMe);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }


        [HttpPost]
        //public JsonResult RegisterUser(string Username, string SummonerName, string Email, string Password, string ConfirmPassword, string Question, string QuestionAnswer)
        public ActionResult RegisterUser(Register registerInfo)
        {
            if (ModelState.IsValid)
            {
                MembershipCreateStatus status = new MembershipCreateStatus();
                MembershipUser newUser = Membership.CreateUser(registerInfo.Username, registerInfo.Password, registerInfo.Email, null, null, true, out status);
                switch (status)
                {
                    case MembershipCreateStatus.Success:
                        string[] summonerNames = new string[5];
                        string[] serverNames = new string[5];
                        //summonerNames[0] = registerInfo.Summoner1Name;
                        serverNames[0] = "na";
                        insertSummonerData(registerInfo.Summoners, newUser.ProviderUserKey.ToString());
                        return View("login");
                    case MembershipCreateStatus.DuplicateEmail:
                        ModelState.AddModelError("Email", "Email already exists");
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        ModelState.AddModelError("Username", "Username is taken");
                        break;
                    default:
                        break;
                }
            }
            return View("register", registerInfo);
        }


        public void insertSummonerData(ICollection<SummonerInfo> summoners, string userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeagueStatServer"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                int i = 1;
                conn.Open();
                foreach (SummonerInfo summoner in summoners)
                {
                    if (summoner.DeleteSummoner != true)
                    {
                        SqlCommand command = new SqlCommand("InsertSummoner", conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Userid", userId);
                        command.Parameters.AddWithValue("@SummonerName", summoner.SummonerName);
                        command.Parameters.AddWithValue("@SummonerId", summoner.SummonerId);
                        command.Parameters.AddWithValue("@SummonerServer", summoner.Server);
                        command.Parameters.AddWithValue("@SummonerNumber", i);
                        command.Parameters.AddWithValue("@RevisionDate", summoner.RevisionDate);
                        command.Parameters.AddWithValue("@ProfileIconId", summoner.ProfileIconId);
                        command.ExecuteNonQuery();
                        i++;
                    }
                }
            }
        }
    }



    public class LoginStatus
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TargetURL { get; set; }
    }
}