namespace Backend.DTOs.Responses
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? ReviewerId { get; set; } // Seller Id
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
