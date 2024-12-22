using eTickets.Models.ViewModel;
using GraphQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace eTickets.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly AppDbContext _context;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
			_roleManager = roleManager;
		}

		public async Task<IActionResult> Users()
		{
			var users = await _context.Users.ToListAsync();
			return View(users);
		}

		public IActionResult Login() => View(new LoginVM());

		[HttpPost]
		public async Task<IActionResult> Login(LoginVM loginVM)
		{
			if (!ModelState.IsValid) return View(loginVM);

			var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
			if (user != null)
			{
				var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
				if (passwordCheck)
				{
					var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
					if (result.Succeeded)
					{
						return RedirectToAction("welcome","Movies");
					}
				}
			}

			TempData["Error"] = "Wrong credentials. Please, try again!";
			return View(loginVM);
		}

		public async Task<IActionResult> Register(RegisterVM registerVM, bool isAdmin)
		{
			if (!ModelState.IsValid)
				return View(registerVM);

			var userName = GenerateUserName(registerVM.FullName);

			var newUser = new ApplicationUser()
			{
				UserName = userName,
				Email = registerVM.EmailAddress,
				FullName = registerVM.FullName
			};

			IdentityResult createResult = await _userManager.CreateAsync(newUser, registerVM.Password);

			if (createResult.Succeeded)
			{
				// تحديد الدور بناءً على شرط
				string role = isAdmin ? "Admin" : "User";
				IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, role);

				if (roleResult.Succeeded)
				{
					await _signInManager.SignInAsync(newUser, isPersistent: false);
					return RedirectToAction("Index", "Movies");
				}
				else
				{
					foreach (var error in roleResult.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			else
			{
				foreach (var error in createResult.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}

			return View(registerVM);
		}
		public IActionResult RegisterUser()
		{
			return View(new RegisterVM());
		}

		[HttpPost]
		public async Task<IActionResult> RegisterUser(RegisterVM registerVM)
		{
			if (!ModelState.IsValid)
				return View(registerVM);

			var userName = GenerateUserName(registerVM.FullName);

			var newUser = new ApplicationUser()
			{
				UserName = userName,
				Email = registerVM.EmailAddress,
				FullName = registerVM.FullName
			};

			IdentityResult createResult = await _userManager.CreateAsync(newUser, registerVM.Password);

			if (createResult.Succeeded)
			{
				// إضافة الدور "User"
				IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, "User");

				if (roleResult.Succeeded)
				{
					await _signInManager.SignInAsync(newUser, isPersistent: false);
					return RedirectToAction("Index", "Movies");
				}
				else
				{
					foreach (var error in roleResult.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			else
			{
				foreach (var error in createResult.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}

			return View(registerVM);
		}
		public IActionResult RegisterAdmin()
		{
			return View(new RegisterVM());
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> RegisterAdmin(RegisterVM registerVM)
		{
			if (!ModelState.IsValid)
				return View(registerVM);

			var userName = GenerateUserName(registerVM.FullName);

			var newUser = new ApplicationUser()
			{
				UserName = userName,
				Email = registerVM.EmailAddress,
				FullName = registerVM.FullName
			};

			IdentityResult createResult = await _userManager.CreateAsync(newUser, registerVM.Password);

			if (createResult.Succeeded)
			{
				// إضافة الدور "Admin"
				IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser , "Admin");

				if (roleResult.Succeeded)
				{
					await _signInManager.SignInAsync(newUser, isPersistent: false);
					return RedirectToAction("Index", "Movies");
				}
				else
				{
					foreach (var error in roleResult.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			else
			{
				foreach (var error in createResult.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}

			return View(registerVM);
		}

		public async Task<IActionResult> Signout()
		{
			await _signInManager.SignOutAsync();
			return View("Login");
		}
		private string GenerateUserName(string fullName)
		{
			return fullName.Replace(" ", "").ToLower();
		}
		[HttpGet]
		public IActionResult ChangePassword(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return BadRequest("Email is required."); // يعيد خطأ إذا كانت القيمة فارغة
			}

			var model = new ChangePasswordViewModel { Email = email };
			return View(model);
		}


		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				ModelState.AddModelError("", "User not found.");
				return View(model);
			}

			var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
			if (result.Succeeded)
			{
				TempData["Success"] = "Password changed successfully!";
				return RedirectToAction("Users"); // أو أي صفحة تريد إعادة توجيه المستخدم إليها
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error.Description);
			}

			return View(model);
		}




		public IActionResult AccessDenied(string returnUrl)
		{
			return View();
		}
        private async Task EnsureAdminRoleExists()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }

        public async Task<IActionResult> CreateAdminAccount()
        {
            await EnsureAdminRoleExists(); // تأكد من وجود دور "Admin"

            var newUser = new ApplicationUser
            {
                UserName = "mahmoud@gmail.com",  // اسم المستخدم هو البريد الإلكتروني
                Email = "mahmoud@gmail.com",
                FullName = "Mahmoud"
            };

            var password = "Admin@123"; // اختر كلمة مرور قوية
            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                // إضافة المستخدم إلى دور "Admin"
                var roleResult = await _userManager.AddToRoleAsync(newUser, "Admin");
                if (roleResult.Succeeded)
                {
                    return Content("Admin user created and assigned successfully!");
                }
                else
                {
                    return Content("Failed to assign admin role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                return Content("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }



    }
}
