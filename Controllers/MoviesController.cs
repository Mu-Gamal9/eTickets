using eTickets.Models;
using eTickets.Models.ViewModel;
using eTickets.Repository;
using eTickets.Services;
using eTickets.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eTickets.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IOtherService _otherService;

        // دمج جميع الخدمات في مُنشئ واحد
        public MoviesController(IMovieRepository movieRepository, IWebHostEnvironment env, IOtherService otherService)
        {
            _movieRepository = movieRepository;
            _env = env;
            _otherService = otherService;
        }

        [AllowAnonymous]
        public IActionResult Welcome()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var allMovies = await _movieRepository.GetAllAsync(n => n.Cinema);
            return View(allMovies);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Filter(string searchString)
        {
            var allMovies = await _movieRepository.GetAllAsync(n => n.Cinema);

            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResultNew = allMovies.Where(n => string.Equals(n.Name, searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();
                return View("Index", filteredResultNew);
            }

            return View("Index", allMovies);
        }
        [AllowAnonymous]
        public async Task<IActionResult> search(string searchString1)
        {
            var allMovies = await _movieRepository.GetAllAsync(n => n.Cinema);

            if (!string.IsNullOrEmpty(searchString1))
            {
                // استخدام المقارنة غير الحساسة للأحرف
                var filteredResultNew = allMovies.Where(n => n.Name.Contains(searchString1, StringComparison.OrdinalIgnoreCase));

                // التحقق إذا كان يوجد نتائج
                if (filteredResultNew.Any())
                {
                    return View("SearchResults", filteredResultNew);  // عرض النتائج
                }
                else
                {
                    return View("SearchResults", Enumerable.Empty<Movie>());  // لا توجد نتائج تطابق
                }
            }

            return View("SearchResults", allMovies);
        }


        //GET: Movies/Details/1
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var movieDetail = await _movieRepository.GetMovieByIdAsync(id);
            return View(movieDetail);
        }

        //GET: Movies/Create
        public async Task<IActionResult> Create()
        {
            var movieDropdownsData = await _movieRepository.GetNewMovieDropdownsValues();

            ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
            ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
            ViewBag.Heroes = new SelectList(movieDropdownsData.Heroes, "Id", "FullName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewMovieVM movie)
        {
            if (!ModelState.IsValid)
            {
                var movieDropdownsData = await _movieRepository.GetNewMovieDropdownsValues();

                ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
                ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
                ViewBag.Heroes = new SelectList(movieDropdownsData.Heroes, "Id", "FullName");

                return View(movie);
            }

            await _movieRepository.AddNewMovieAsync(movie);
            return RedirectToAction(nameof(Index));
        }

        //GET: Movies/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var movieDetails = await _movieRepository.GetMovieByIdAsync(id);
            if (movieDetails == null) return View("NotFound");

            var response = new NewMovieVM()
            {
                Id = movieDetails.Id,
                Name = movieDetails.Name,
                Description = movieDetails.Description,
                Price = movieDetails.Price,
                StartDate = movieDetails.StartDate,
                EndDate = movieDetails.EndDate,
                ImageURL = movieDetails.ImageURL,
                MovieCategory = movieDetails.MovieCategory,
                CinemaId = movieDetails.CinemaId,
                ProducerId = movieDetails.ProducerId,
                HeroIds = movieDetails.Heroes_Movies.Select(n => n.HeroId).ToList(),
            };

            var movieDropdownsData = await _movieRepository.GetNewMovieDropdownsValues();
            ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
            ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
            ViewBag.Heroes = new SelectList(movieDropdownsData.Heroes, "Id", "FullName");

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, NewMovieVM movie)
        {
            if (id != movie.Id) return View("NotFound");

            if (!ModelState.IsValid)
            {
                var movieDropdownsData = await _movieRepository.GetNewMovieDropdownsValues();

                ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
                ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
                ViewBag.Heroes = new SelectList(movieDropdownsData.Heroes, "Id", "FullName");

                return View(movie);
            }

            await _movieRepository.UpdateMovieAsync(movie);
            return RedirectToAction(nameof(Index));
        }

        //GET: Movies/AboutUs
        [AllowAnonymous]
        public IActionResult AboutUs()
        {
            var model = new AboutUs
            {
                Title = "About Us",
                Description = "Welcome to eTickets, your premier destination for hassle-free movie ticket booking!",
                Mission = "Our mission is to make movie ticket booking easy, convenient, and enjoyable for everyone.",
                Vision = "Our vision is to become the go-to platform for all movie enthusiasts, providing the best viewing experiences.",
                CompanyAddress = "123 Movie Street, Assuit",
                ContactEmail = "movieseticket@gmail.com"
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var movieDetail = await _movieRepository.GetMovieByIdAsync(id);
            if (movieDetail == null) return View("NotFound");

            var cinemaId = movieDetail.CinemaId;
            var producerId = movieDetail.ProducerId;

            var viewModel = new NewMovieVM
            {
                Id = movieDetail.Id,
                Name = movieDetail.Name,
                Description = movieDetail.Description,
                Price = movieDetail.Price,
                ImageURL = movieDetail.ImageURL,
                StartDate = movieDetail.StartDate,
                EndDate = movieDetail.EndDate,
                MovieCategory = movieDetail.MovieCategory,
                CinemaId = cinemaId,
                ProducerId = producerId,
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movieDetails = await _movieRepository.GetByIdAsync(id);
            if (movieDetails == null) return View("NotFound");

            await _movieRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> PrivacyPolicy()
        {
            var privacyPolicyContent = await _movieRepository.GetPrivacyPolicyContentAsync();
            var privacyPolicy = new PrivacyPolicy
            {
                Content = privacyPolicyContent
            };
            return View(privacyPolicy);
        }

        // Action show seat free
        public async Task<IActionResult> SelectSeats(int movieId)
        {
            var movie = await _movieRepository.Movies.Include(m => m.Seats)
                                                     .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
            {
                return NotFound();
            }

            var viewModel = new MovieSeatsVM
            {
                Movie = movie,
                Seats = movie.Seats.Where(s => s.IsAvailable).ToList() // اختيار الكراسي المتاحة فقط
            };

            return View(viewModel);
        }

        // Action book seat
        [HttpPost]
        public async Task<IActionResult> BookSeat(int seatId, int movieId)
        {
            // جلب الكرسي للتحقق
            var seat = await _movieRepository.GetSeatByIdAsync(seatId, movieId);

            // التحقق من وجود الكرسي وحالته
            if (seat == null || !seat.IsAvailable)
            {
                return RedirectToAction("Error", "Home"); // عرض رسالة خطأ للمستخدم
            }

            // تحديث حالة الكرسي
            await _movieRepository.UpdateSeatAvailabilityAsync(seatId, false);

            // التوجه إلى صفحة الدفع
            return RedirectToAction("Payment", "Orders");
        }
    }
}
