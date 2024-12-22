namespace eTickets.Models
{
    public class seat
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; }
        public bool IsAvailable { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
