using eTickets.Models;
using eTickets.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace eTickets.Controllers
{

	[Authorize(Roles ="Admin")]
	
		public class LogoController : Controller
		{
			private readonly ILogoRepository _logoRepository;

			public LogoController(ILogoRepository logoRepository)
			{
				_logoRepository = logoRepository;
			}

			public IActionResult Index()
			{
				var logo = _logoRepository.GetById();
				ViewData["LogoImage"] = logo?.LogoImage ?? "/images/123.jpg"; 

				return View();
			}
		

		[HttpPost]
		public async Task< IActionResult> UploadLogo(IFormFile logoImage)
		{
			if (logoImage != null && logoImage.Length > 0)
			{
				var logoPath = Path.Combine("wwwroot/images", logoImage.FileName);

				using (var stream = new FileStream(logoPath, FileMode.Create))
				{
					logoImage.CopyTo(stream);
				}

				var logo = _logoRepository.GetById() ?? new Logo();
				logo.LogoImage = "/images/" + logoImage.FileName;

				if (logo.Id == 0)
				{

					_logoRepository.Add(logo);

				}
				else
				{
					_logoRepository.UpdateLogo(logo);
				}

				_logoRepository.Save();
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult DeleteLogo()
		{
			var logo = _logoRepository.GetById();
			if (logo != null)
			{
				if (logo.LogoImage.EndsWith("123.jpg"))
				{
				
					return RedirectToAction("Index");
				}

				var logoPath = Path.Combine("wwwroot", logo.LogoImage.TrimStart('/'));
				if (System.IO.File.Exists(logoPath))
				{
					System.IO.File.Delete(logoPath);
				}

				_logoRepository.Delete(logo.Id);
				_logoRepository.Save();
			}

			return RedirectToAction("Index");
		}
	}
}