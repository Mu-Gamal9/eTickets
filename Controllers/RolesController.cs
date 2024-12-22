using eTickets.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eTickets.Controllers
{
	[Authorize(Roles = "Admin")]
	public class RolesController : Controller
	{
		
		private readonly RoleManager<IdentityRole> roleManager;

		public RolesController(RoleManager<IdentityRole> roleManager)
		{
			this.roleManager = roleManager;
		}
		public IActionResult AddRole()
		{
			return View("AddRole");
		}
		public async Task<IActionResult> SaveRole(RoleViewModel roleViewModel)
		{
			IdentityRole identityRole = new IdentityRole();

			if (roleViewModel != null)
			{
				identityRole.Name = roleViewModel.RoleName;
				IdentityResult result = await roleManager.CreateAsync(identityRole);
				if (result.Succeeded==true)
				{
					return View("sucsess");
				}
				foreach (var item in result.Errors)
				{
					ModelState.AddModelError("", item.Description);
				}

			}
			return View("AddRole", roleViewModel);

		}
	}
}
