using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProniaNew.Models;
using ProniaNew.ViewModels;

namespace ProniaNew.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _usermanager;
        private readonly SignInManager<AppUser> _signinManager;

        public AccountController(UserManager<AppUser> manager, SignInManager<AppUser> signIn)
        {
            _usermanager = manager;
            _signinManager = signIn;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View();
            registerVM.Name = registerVM.Name.Trim();
            registerVM.Surname = registerVM.Surname.Trim();

            string name = Char.ToUpper(registerVM.Name[0]) + registerVM.Name.Substring(1).ToLower();
            string surname = Char.ToUpper(registerVM.Surname[0]) + registerVM.Surname.Substring(1).ToLower();

            AppUser user = new()
            {
                Name = name,
                Surname = surname,
                Email = registerVM.Email,
                UserName = registerVM.username,
                Gender = registerVM.Gender
            };
            var result = await _usermanager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }
                return View();
            }
            await _signinManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await _signinManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}

