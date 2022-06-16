using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UAF_Frontend_Registration.Models;
using UAF_Frontend_Registration.Services;
using UAF_Frontend_Registration.Settings;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using UAF_Frontend_Registration.Helpers;

namespace UAF_Frontend_Registration.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICallbackSettings _callbackurl;
        private readonly ILDAPSettings _ldapConfig;
        private readonly IEndpointSettings _oidcConfig;


        public HomeController(ILoggerFactory loggerFactory, ICallbackSettings callbackurl, ILDAPSettings ldapConfig, IEndpointSettings oidcConfig)
        {
            _callbackurl = callbackurl;
            _ldapConfig = ldapConfig;
            _oidcConfig = oidcConfig;

            loggerFactory =
              LoggerFactory.Create(builder =>
                  builder.AddSimpleConsole(options =>
                  {
                      options.IncludeScopes = true;
                      options.SingleLine = true;
                      options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]: ";
                  }));
            _loggerFactory = loggerFactory;
        }

        public IActionResult Index()
        {
            var session = checkSession();

            if (session)
            {
                return RedirectToAction("Register");
            }
            else
            {
                return RedirectToAction("Login");

            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Register()
        {
            var session = checkSession();

            if (session)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult PleaseWait()
        {
            var session = checkSession();

            var _logger = _loggerFactory.CreateLogger<HomeController>();

            if (session)
            {
                _logger.LogInformation("Fido Registration Success");
                return RedirectToAction("Success");
            }
            else
            {
                _logger.LogInformation("Fido Registration Failed");
                return RedirectToAction("Error");
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult Login()
        {
            var session = checkSession();

            if (session)
            {
                return RedirectToAction("Register");
            }
            else
            {
                return View(new User { login_status = null });
            }
        }

        [HttpPost]
        public IActionResult Register(string email)
        {
            var session = checkSession();

            if (session)
            {
                var _logger = _loggerFactory.CreateLogger<HomeController>();

                try
                {
                    _logger.LogInformation("-----Start Fido Registration-----");
                    _logger.LogInformation("Username: " + email);

                    var name = email.Split("@")[0];
                    _logger.LogInformation("Name: " + name);

                    var qrcode_resp = requestQrCode(email, name, _logger);

                    if (qrcode_resp == null)
                    {
                        return View("Error");
                    }
                    else
                    {
                        _logger.LogInformation("Get Qr Code Success");
                        return View("QRCode", qrcode_resp);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    ex.StackTrace.ToString();
                    return View("Error");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string submit)
        {
            var _logger = _loggerFactory.CreateLogger<HomeController>();
            string init_auth_req = "";

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("-----Start Login by D Dopa-----");

                    Client auth = new Client();
                    init_auth_req = auth.GetAuthnReqUrl(_oidcConfig);

                    _logger.LogInformation("Get Authorized to Dopa: " + init_auth_req);

                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Error" + ex);
                    return View(new User { login_status = "Apologize - The system is under maintenance. Please try again later" });
                }
            }
            return Redirect(init_auth_req);
        }

        public IActionResult Logout()
        {
            var _logger = _loggerFactory.CreateLogger<HomeController>();

            _logger.LogInformation("Logout: " + HttpContext.Session.GetString("en_fullname"));

            HttpContext.Session.Clear();

            foreach (string cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            return RedirectToAction("Login");

        }

        [HttpGet]
        public IActionResult Discard()
        {
            var session = checkSession();

            if (session)
            {
                return RedirectToAction("Register");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            var _logger = _loggerFactory.CreateLogger<HomeController>();
            var session = checkSession();

            if (session)
            {
                clearSession();

                _logger.LogInformation("Logout Success");

                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult QRCodeError()
        {
            var session = checkSession();

            if (session)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        private QRCodeResp requestQrCode(string identity, string name, ILogger<HomeController> _logger)
        {
            try
            {

                _logger.LogInformation("----- Start Request QR Code -----");
                QRCodeRequestService qr_req = new QRCodeRequestService(_loggerFactory);
                return qr_req.qrCodeAsync(identity, name, _callbackurl.Register_request, _callbackurl.Qrcode_request, _callbackurl.Credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ex.StackTrace.ToString();
                return null;
            }
        }

        private bool checkSession()
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Cookies["username"]))
            {
                return false;
            }
            else if (string.IsNullOrEmpty(HttpContext.Session.GetString("en_fullname")))
            {
                return false;
            }

            else if (string.IsNullOrEmpty(HttpContext.Request.Cookies[".dopaAuthen.Session"]))
            {
                return false;
            }
            else if (!HttpContext.Request.Cookies["username"].Equals(HttpContext.Session.GetString("en_fullname")))
            {
                return false;
            }
            return true;
        }

        private void clearSession()
        {
            HttpContext.Session.Clear();

            foreach (string cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
        }
    }
}