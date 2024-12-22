namespace eTickets.Models.ViewModel
{
    public class MovieSeatsVM
    {
        public Movie Movie { get; set; }
        public List<seat> Seats { get; set; }
        public int SelectedSeatId { get; set; }
    }
}
