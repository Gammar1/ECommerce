using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Account.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;
        private readonly CartService _cartService;

        public LoginModel(
           AuthService authService,
            CartService cartService)
        {
            _authService = authService;
            _cartService = cartService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; }

        [TempData]
        public string LoginMessage { get; set; }

        public async Task OnGetAsync()
        {
            //await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = Url.Content("~/") });

            //if(response.IsCompleted == null)
            //{
            //    return Task.FromResult(new HttpResponseMessage() { });
            //}

            //return null;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var authResult = await _authService.Login(Input);

                switch (authResult.StatusCode)
                {
                    case SD.StatusCode.OK:
                        if (authResult.ApplicationUser.LockoutEnd != null)
                        {
                            LoginMessage = "Your account has been suspended due to possible violation of E-Mall rules and regulations. Please contact E-Mall for more information.";
                            return RedirectToPage();
                        }

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, authResult.ApplicationUser.Id),
                            new Claim(JwtClaimTypes.GivenName, authResult.ApplicationUser.Name),
                            new Claim(ClaimTypes.Email, authResult.ApplicationUser.Email),
                            new Claim(ClaimTypes.Role, authResult.ApplicationUser.Role),
                            new Claim(ClaimTypes.MobilePhone, authResult.ApplicationUser.PhoneNumber),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "Password");
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                        {
                            IsPersistent = Input.RememberMe,
                            RedirectUri = returnUrl
                        });

                        var cartItemsCount = await _cartService.GetItemsCount(authResult.ApplicationUser.Id);

                        HttpContext.Session.SetInt32(SD.CART_SESSION_KEY, cartItemsCount);

                        return LocalRedirect(returnUrl);
                    case SD.StatusCode.NOTFOUND:
                    case SD.StatusCode.UNAUTHORIZED:
                        LoginMessage = authResult.Message[0];
                        break;
                }
            }
            return RedirectToPage();
        }
    }
}
