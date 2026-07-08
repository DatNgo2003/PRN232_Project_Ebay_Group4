using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Backend.Models;


	[Table("Message")]
	public partial class Message
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int SenderId { get; set; }

		[Required]
		public int ReceiverId { get; set; }

		[Required]
		[MaxLength(2000)]
		public string Content { get; set; } = string.Empty;

		[Required]
		[Column("Timestamp", TypeName = "datetime2")]
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	}

