using System;

namespace Backend.Models
{
    public class ProcessedWebhookEvent
    {
        public int Id { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}