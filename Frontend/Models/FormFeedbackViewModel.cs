using Backend.Models;

namespace Frontend.Models
{
	public class FormFeedbackViewModel
	{
		public int OrderTableId { get; set; }
		public OrderTable OrderTable { get; set; }
		public int PositiveRate { get; set; }
		public int DeliveryOnTime { get; set; }
		public int ExactSame { get; set; }
		public int Communication { get; set; }
		public string Comment { get; set; }
		public int ReceiverId { get; set; }

	}
}
