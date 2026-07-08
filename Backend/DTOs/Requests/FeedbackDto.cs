using Backend.Models;

namespace Backend.DTOs.Requests
{
	public class FeedbackDto
	{
		public int OrderTableId { get; set; }
		public int PositiveRate { get; set; }
		public int DeliveryOnTime { get; set; }
		public int ExactSame { get; set; }
		public int Communication { get; set; }
		public string Comment { get; set; }
		public int ReceiverId { get; set; }
	}
}
