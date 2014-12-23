using ChampsRoom.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using System.Configuration;

namespace ChampsRoom
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(DataContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/account/login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, User>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            var microsoftClientId = ConfigurationManager.AppSettings["microsoft:clientid"];
            var microsoftClientSecret = ConfigurationManager.AppSettings["microsoft:clientsecret"];
            if (!string.IsNullOrWhiteSpace(microsoftClientId) && !string.IsNullOrWhiteSpace(microsoftClientSecret))
            {
                app.UseMicrosoftAccountAuthentication(clientId: microsoftClientId, clientSecret: microsoftClientSecret);
            }

            var twitterConsumerKey = ConfigurationManager.AppSettings["twitter:consumerkey"];
            var twitterConsumerSecret = ConfigurationManager.AppSettings["twitter:consumersecrect"];
            if (!string.IsNullOrWhiteSpace(twitterConsumerKey) && !string.IsNullOrWhiteSpace(twitterConsumerSecret))
            {
                app.UseTwitterAuthentication(
                    new Microsoft.Owin.Security.Twitter.TwitterAuthenticationOptions()
                    {
                        ConsumerKey = twitterConsumerKey,
                        ConsumerSecret = twitterConsumerSecret,
                        Provider = new Microsoft.Owin.Security.Twitter.TwitterAuthenticationProvider()
                        {
                            OnAuthenticated = async context =>
                            {
                                context.Identity.AddClaim(new System.Security.Claims.Claim("urn:twitter:accesstoken", context.AccessToken));
                                context.Identity.AddClaim(new System.Security.Claims.Claim("urn:twitter:accesstokensecret", context.AccessTokenSecret));
                            }
                        }
                    });
            }

            var facebookAppId = ConfigurationManager.AppSettings["facebook:appid"];
            var facebookAppSecret = ConfigurationManager.AppSettings["facebook:appsecrect"];
            if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
            {
                app.UseFacebookAuthentication(appId: facebookAppId, appSecret: facebookAppSecret);
            }

            var googleClientId = ConfigurationManager.AppSettings["google:clientid"];
            var googleClientSecret = ConfigurationManager.AppSettings["google:clientsecret"];
            if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
            {
                app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
                {
                    ClientId = googleClientId,
                    ClientSecret = googleClientSecret
                });
            }
        }
    }
}