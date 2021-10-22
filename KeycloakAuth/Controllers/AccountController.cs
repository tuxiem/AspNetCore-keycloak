using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeycloakAuth.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string userName)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                Response.Cookies.Append("cloudpense.login.username", userName);
            }

            return RedirectToAction("ExternalRedirect");
        }

        [HttpGet]
        [Authorize]
        public IActionResult LogOut()
        {
            return new SignOutResult(
                new[]
                {
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme,
                },
                new AuthenticationProperties { RedirectUri = "/" });
        }

        [HttpGet]
        [Authorize]
        public IActionResult ExternalRedirect()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
