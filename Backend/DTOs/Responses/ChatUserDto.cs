using System;

namespace Backend.DTOs.Responses
{
    public class ChatUserDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? LastMessage { get; set; }
        public string? LastTimestamp { get; set; }
    }
}
