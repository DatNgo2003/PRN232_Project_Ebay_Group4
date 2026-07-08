namespace Backend.DTOs.Responses
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public string? Content { get; set; }
        public string? Timestamp { get; set; }
        public string? SenderUsername { get; set; }
    }
}
