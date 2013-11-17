﻿using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Collections.Generic;

namespace ChampsRoom
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/account/login/")
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            app.UseTwitterAuthentication(
                new Microsoft.Owin.Security.Twitter.TwitterAuthenticationOptions()
                {
                    ConsumerKey = "ztVUp8CwG0jyYZZoDKGXg",
                    ConsumerSecret = "jyFWNjzApKtogHMnRVvdvBqWJF2gPHNldjvopHZSoE",
                    Provider = new Microsoft.Owin.Security.Twitter.TwitterAuthenticationProvider()
                    {
                        OnAuthenticated = async context =>
                        {
                            context.Identity.AddClaim(new System.Security.Claims.Claim("urn:twitter:accesstoken", context.AccessToken));
                            context.Identity.AddClaim(new System.Security.Claims.Claim("urn:twitter:accesstokensecret", context.AccessTokenSecret));
                        }
                    }
                }
                );

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            app.UseGoogleAuthentication();
        }
    }
}