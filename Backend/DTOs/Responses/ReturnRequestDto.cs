namespace Backend.DTOs.Responses
{
    public class ReturnRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal OrderTotal { get; set; }
        public string? ProductTitle { get; set; }
    }
}
