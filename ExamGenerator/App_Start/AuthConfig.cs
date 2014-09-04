using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using ExamGenerator.Models;

namespace ExamGenerator
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // Aby umożliwić użytkownikom tej witryny logowanie się za pomocą kont w innych witrynach, takich jak Microsoft, Facebook, Twitter,
            // musisz zaktualizować tę witrynę. Aby uzyskać więcej informacji, odwiedź stronę http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
