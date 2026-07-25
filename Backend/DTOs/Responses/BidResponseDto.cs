namespace Backend.DTOs.Responses
{
    public class BidResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductTitle { get; set; }
        public int BidderId { get; set; }
        public string? BidderUsername { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidTime { get; set; }
        public bool IsWinning { get; set; }
    }

    public class AuctionListItemDto
    {
        public int ProductId { get; set; }
        public string? ProductTitle { get; set; }
        public string? ProductImage { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentHighestBid { get; set; }
        public int TotalBids { get; set; }
        public DateTime AuctionEndTime { get; set; }
        public bool IsEnded { get; set; }
        public string? HighestBidderUsername { get; set; }
    }
}
