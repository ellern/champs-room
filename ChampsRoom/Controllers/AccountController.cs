using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using ChampsRoom.Helpers;
using ChampsRoom.Models;
using Newtonsoft.Json;
using System.Data.Entity;

namespace ChampsRoom.Controllers
{
    [Authorize]
    [RoutePrefix("account")]
    public class AccountController : Controller
    {
        private DataContext db = new DataContext();

        public AccountController()
            : this(new UserManager<User>(new UserStore<User>(new DataContext())))
        {
        }

        public AccountController(UserManager<User> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<User> UserManager { get; private set; }

        [AllowAnonymous]
        [Route("login")]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("login")]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        [Route("register")]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("register")]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!await IsNameAvailable(model.UserName))
                ModelState.AddModelError(String.Empty, "Name is not available");

            if (ModelState.IsValid)
            {
                var user = new User() { UserName = model.UserName, Url = model.UserName.ToFriendlyUrl() };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Route("edit")]
        public async Task<ActionResult> Edit()
        {
            var userid = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(q => q.Id == userid);

            if (user == null)
                return HttpNotFound();

            var viewmodel = new UserEditViewModel()
            {
                ImageUrl = user.ImageUrl,
                UserName = user.UserName
            };

            return View(viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit")]
        public async Task<ActionResult> Edit(UserEditViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(q => q.Id == userid);

            if (user == null)
                return HttpNotFound();

            if (!await IsNameAvailable(model.UserName, user.UserName))
                ModelState.AddModelError(String.Empty, "Name is not available");

            if (ModelState.IsValid)
            {
                user.ImageUrl = model.ImageUrl;
                user.UserName = model.UserName;
                user.Url = model.UserName.ToFriendlyUrl();

                db.Entry(user).State = EntityState.Modified;

                await db.SaveChangesAsync();
                await SignInAsync(user, isPersistent: false);

                return RedirectToAction("Manage");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            //var userId = User.Identity.GetUserId();
            //ViewBag.Leagues = db.Leagues.Where(q => q.Users.Any(u => u.Id == userId)).ToList();

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await OnExternalLoginAuth(user);
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(loginInfo.DefaultUserName))
                {
                    user = new User() { UserName = loginInfo.DefaultUserName, Url = loginInfo.DefaultUserName.ToFriendlyUrl() };
                    var result = await UserManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                        if (result.Succeeded)
                        {
                            await OnExternalLoginAuth(user);
                            await SignInAsync(user, isPersistent: false);

                            return RedirectToLocal(returnUrl);
                        }
                    }
                }

                // If the user does not have an account, then prompt the user to create an account - and if the automatically create failed
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User() { UserName = model.UserName, Url = model.UserName.ToFriendlyUrl() };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await OnExternalLoginAuth(user);
                        await SignInAsync(user, isPersistent: false);

                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(User user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

            identity.AddClaim(new Claim("ImageUrl", user.GetImageUrl()));

            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        private async Task<bool> IsNameAvailable(string name, string currentName = "")
        {
            var notAllowed = new string[] {
                                 "account",
                                 "result",
                                 "results",
                                 "league",
                                 "leagues",
                                 "team",
                                 "teams",
                                 "user",
                                 "users",
                                 "player",
                                 "players",
                                 "admin",
                                 "index",
                                 "create",
                                 "edit",
                                 "details",
                                 "delete",
                                 "put",
                                 "post",
                                 "home"
                             };

            var friendlyName = name.ToFriendlyUrl();

            if (friendlyName.Equals(currentName.ToFriendlyUrl()))
                return true;

            if (notAllowed.Count(q => q.Equals(name, StringComparison.InvariantCultureIgnoreCase)) > 0)
                return false;

            var count = await db.Users.CountAsync(q => q.Url.Equals(name, StringComparison.InvariantCultureIgnoreCase));


            return count == 0;
        }

        private async Task SetTwitterProfileImage(string userId, IEnumerable<Claim> claims)
        {
            // Retrieve the twitter access token and claim
            var accessTokenClaim = claims.FirstOrDefault(x => x.Type == "urn:twitter:accesstoken");
            var accessTokenSecretClaim = claims.FirstOrDefault(x => x.Type == "urn:twitter:accesstokensecret");

            if (accessTokenClaim != null && accessTokenSecretClaim != null)
            {
                var service = new TweetSharp.TwitterService("ztVUp8CwG0jyYZZoDKGXg", "jyFWNjzApKtogHMnRVvdvBqWJF2gPHNldjvopHZSoE", accessTokenClaim.Value, accessTokenSecretClaim.Value);
                var profile = service.GetUserProfile(new TweetSharp.GetUserProfileOptions());

                if (profile != null && !String.IsNullOrWhiteSpace(profile.ProfileImageUrl))
                {
                    var user = db.Users.Find(userId);

                    user.ImageUrl = profile.ProfileImageUrl.Replace("_normal", "_bigger");

                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    await db.SaveChangesAsync();
                }
            }
        }

        private async Task OnExternalLoginAuth(User user)
        {
            var externalIdentity = await HttpContext.GetOwinContext().Authentication.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);

            if (externalIdentity != null)
            {
                foreach (var item in externalIdentity.Claims)
                {
                    if (item.Type == ClaimTypes.NameIdentifier)
                        continue;

                    await UserManager.RemoveClaimAsync(user.Id, item);
                    await UserManager.AddClaimAsync(user.Id, item);
                }

                await SetTwitterProfileImage(user.Id, externalIdentity.Claims);
            }

        }
    }
}