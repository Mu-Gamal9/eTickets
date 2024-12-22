using Microsoft.EntityFrameworkCore;

namespace eTickets.Models
{
    public class DbInitializer
    {
        private readonly AppDbContext _context;

        public DbInitializer(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddSeatsForExistingMovies()
        {
            var movies = await _context.Movies.ToListAsync();

            foreach (var movie in movies)
            {
                // التأكد أن الفيلم لا يحتوي بالفعل على كراسي
                if (movie.Seats == null || !movie.Seats.Any())
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        _context.Seats.Add(new seat
                        {
                            MovieId = movie.Id,
                            SeatNumber = i.ToString(),  // تحويل الرقم إلى نص (string)
                            IsAvailable = true // الكراسي متاحة في البداية
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }


}
