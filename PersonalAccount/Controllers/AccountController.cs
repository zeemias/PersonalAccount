﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PersonalAccount.Models;
using System.Net;
using DaData.Client;
using System.Collections.Generic;
using System.Web.Security;
using System.Net.Mail;

namespace PersonalAccount.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LogRegViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Logins.Email, model.Logins.Password, model.Logins.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("About", "Home");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = "#", RememberMe = model.Logins.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(string type)
        {
            if (ModelState.IsValid)
            {
                switch (Request["Type"])
                {
                    case "Questions":
                        if (Request["ActivitiesYes"] == "false" && Request["ActivitiesNo"] == "false" ||
                            Request["InsuranceYes"] == "false" && Request["InsuranceNo"] == "false" ||
                            Request["CompanyYes"] == "false" && Request["CompanyNo"] == "false" ||
                            Request["EmployeeYes"] == "false" && Request["EmployeeNo"] == "false" ||
                            Request["ExperienceYes"] == "false" && Request["ExperienceNo"] == "false" ||
                            Request["BaseYes"] == "false" && Request["BaseNo"] == "false")
                        {
                            ModelState.AddModelError("", "Не все поля заполнены");
                            return View();
                        }
                        if (Request["ActivitiesYes"] == "true,false" && Request["ActivitiesNo"] == "true,false" ||
                            Request["InsuranceYes"] == "true,false" && Request["InsuranceNo"] == "true,false" ||
                            Request["CompanyYes"] == "true,false" && Request["CompanyNo"] == "true,false" ||
                            Request["EmployeeYes"] == "true,false" && Request["EmployeeNo"] == "true,false" ||
                            Request["ExperienceYes"] == "true,false" && Request["ExperienceNo"] == "true,false" ||
                            Request["BaseYes"] == "true,false" && Request["BaseNo"] == "true,false")
                        {
                            ModelState.AddModelError("", "Нельзя выбирать оба поля в одном вопросе");
                            return View();
                        }
                        if (Request["ActivitiesYes"] == "true,false" && Request["InsuranceNo"] == "true,false" && Request["CompanyNo"] == "true,false" &&
                            Request["EmployeeNo"] == "true,false" && Request["ExperienceYes"] == "true,false" && Request["BaseYes"] == "true,false")
                        {
                            ViewBag.Type = "RegisterOGRN";
                            return View();
                        }
                        ViewBag.Type = "NoRegistration";
                        return View();
                    case "OGRN":
                        string ogrn = Request["party"];
                        var sug = DadataParty(ogrn);
                        string ogrnParty = sug.data.ogrn;
                        LogRegViewModel reg = new LogRegViewModel();
                        if (ogrn == ogrnParty)
                        {
                            using(ApplicationDbContext db = new ApplicationDbContext())
                            {
                                if(db.Users.Any(t => t.OGRN == ogrn))
                                {
                                    ViewBag.Type = "CompanyRegistered";
                                    ViewBag.OGRN = ogrn;
                                    return View();
                                }
                                else
                                {
                                    reg.Registers.OGRN = ogrn;
                                    reg.Registers.CompanyName = sug.data.name.@short;
                                    reg.Registers.FullCompanyName = sug.data.name.short_with_opf;
                                    reg.Registers.OPF = sug.data.opf.@short;
                                    var city = DadataAddress(sug.data.address.value);
                                    reg.Registers.City = city.data.city;
                                    ViewBag.Type = "RegisterUser";
                                    return View(reg);
                                }
                            }
                        }
                        else
                        {
                            return View();
                        }
                    case "User":
                        string regOGRN = Request["Registers.OGRN"];
                        var suggestion = DadataParty(regOGRN);
                        string password = Membership.GeneratePassword(12, 1);
                        var user = new ApplicationUser { UserName = Request["Registers.Email"],
                            CompanyType = "Организация",
                            Email = Request["Registers.Email"],
                            OGRN = regOGRN,
                            City = Request["Registers.City"],
                            CompanyName = Request["Registers.CompanyName"],
                            FullCompanyName = suggestion.value,
                            OPF = Request["Registers.OPF"],
                            ContactFIO = Request["Registers.ContactFIO"],
                            PhoneNumber = Request["Registers.PhoneNumber"],
                            PhoneNumberOne = Request["Registers.PhoneNumberOne"],
                            PhoneNumberTwo = Request["Registers.PhoneNumberTwo"],
                            EmailEmployee = Request["Registers.EmailEmployee"],
                            WebSite = Request["Registers.WebSite"],
                            INN = suggestion.data.inn,
                            KPP = suggestion.data.kpp,
                            DirectorFIO = suggestion.data.management.name,
                            DirectorPost = suggestion.data.management.post,
                            LawAddress = suggestion.data.address.value
                        };
                        var result = await UserManager.CreateAsync(user, password);
                        if (result.Succeeded)
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            SendEmailAsync(user.Email, "Регистрация", "Спасибо за регистрацию!<br> Ваш Логин: " + user.Email + "<br> Ваш пароль: "+ password);
                            ViewBag.Type = "RegisterSuccess";
                            return View();
                        }
                        AddErrors(result); 
                        return View();
                    case "Branch":
                        ViewBag.OGRN = Request["OGRN"];
                        ViewBag.Type = "RegisterBranch";
                        return View();
                    case "RegisterBranch":
                        string branchOGRN = Request["OGRN"];
                        var sugbranch = DadataParty(branchOGRN);
                        string passwordBranch = Membership.GeneratePassword(12, 1);
                        var userBranch = new ApplicationUser
                        {
                            UserName = Request["Registers.Email"],
                            CompanyType = Request["Registers.CompanyType"],
                            Email = Request["Registers.Email"],
                            OGRN = branchOGRN,
                            City = Request["Registers.City"],
                            CompanyName = sugbranch.data.name.@short,
                            FullCompanyName = sugbranch.data.name.short_with_opf,
                            OPF = sugbranch.data.opf.@short,
                            ContactFIO = Request["Registers.ContactFIO"],
                            PhoneNumber = Request["Registers.PhoneNumber"],
                            PhoneNumberOne = Request["Registers.PhoneNumberOne"],
                            PhoneNumberTwo = Request["Registers.PhoneNumberTwo"],
                            EmailEmployee = Request["Registers.EmailEmployee"],
                            WebSite = Request["Registers.WebSite"],
                            INN = sugbranch.data.inn,
                            KPP = sugbranch.data.kpp,
                            DirectorFIO = sugbranch.data.management.name,
                            DirectorPost = sugbranch.data.management.post,
                            LawAddress = sugbranch.data.address.value
                        };
                        var resultBranch = await UserManager.CreateAsync(userBranch, passwordBranch);
                        if (resultBranch.Succeeded)
                        {
                            await SignInManager.SignInAsync(userBranch, isPersistent: false, rememberBrowser: false);
                            SendEmailAsync(userBranch.Email, "Регистрация", "Спасибо за регистрацию!<br> Ваш Логин: " + userBranch.Email + "<br> Ваш пароль: " + passwordBranch);
                            ViewBag.Type = "RegisterSuccess";
                            return View();
                        }
                        AddErrors(resultBranch);
                        return View();
                    default:
                        return RedirectToAction("Http404", "Error");
                }   
            }

            // If we got this far, something failed, redisplay form
           return View();
        }

        public void SendEmailAsync(string GetEmail, string mailSubject, string mailBody)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("");
                mail.To.Add(GetEmail);
                mail.Subject = mailSubject;
                mail.Body = mailBody;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 25))
                {
                    smtp.Credentials = new NetworkCredential("", "");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }    
            }
            catch
            {
            }
        }

        SuggestPartyResponse.Suggestions DadataParty(string party)
        {
            var token = "";
            var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
            var api = new SuggestClient(token, url);
            var response = api.QueryParty(party);
            var suggestion = response.suggestions[0];
            return suggestion;
        }

        SuggestAddressResponse.Suggestions DadataAddress(string address)
        {
            var token = "";
            var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
            var api = new SuggestClient(token, url);
            var response = api.QueryAddress(address);
            var suggestion = response.suggestions[0];
            return suggestion;
        }

        //
        // GET: /Account/RegisterQuestion
        [AllowAnonymous]
        public ActionResult RegisterQuestion()
        {
            return View();
        }

        //
        // GET: /Account/NoRegistration
        [AllowAnonymous]
        public ActionResult NoRegistration()
        {
            return View();
        }

        //
        // GET: /Account/RegisterUser
        [AllowAnonymous]
        public ActionResult RegisterUser()
        {
            return View();
        }

        //
        // POST: /Account/RegisterQuestion
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterQuestion(string model)
        {
            if (ModelState.IsValid)
            {

            }
            return View(model);
        }

        //
        // GET: /Account/RegisterQuestion
        [AllowAnonymous]
        public ActionResult RegisterOGRN()
        {
            return View();
        }

        //
        // GET: /Account/RegisterQuestion
        [AllowAnonymous]
        public ActionResult CompanyRegistered()
        {
            return View();
        }

        //
        // GET: /Account/RegisterQuestion
        [AllowAnonymous]
        public ActionResult RegisterBranch()
        {
            return View();
        }

        //
        // GET: /Account/RegisterQuestion
        [AllowAnonymous]
        public ActionResult RegisterSuccess()
        {
            return View();
        }

        //
        // POST: /Account/RegisterQuestion
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterOGRN(string model)
        {
            if (ModelState.IsValid)
            {
            }
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpGet]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
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

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
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
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}