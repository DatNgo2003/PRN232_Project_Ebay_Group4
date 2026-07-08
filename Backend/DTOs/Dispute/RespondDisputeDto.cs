namespace Backend.DTOs.Dispute
{
    public class RespondDisputeDto
    {
        public int Id { get; set; }
        public string Resolution { get; set; }
        public string Comment { get; set; }
        public string status { get; set; }
        public DateTime? SolvedDate { get; set; }
        public int SellerId { get; set; }
    }
}
