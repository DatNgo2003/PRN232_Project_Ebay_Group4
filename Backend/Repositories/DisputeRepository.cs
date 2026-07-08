using Backend.DTOs.Dispute;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class DisputeRepository : IDisputeRepository
    {
        private readonly CloneEbayDbContext _context;
        private readonly ILogger<DisputeRepository> _logger;
        public DisputeRepository(CloneEbayDbContext context, ILogger<DisputeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<DisputeListItemDto>> GetDisputesByBuyerAsync(int buyerId)
        {
            // 1. Lấy các dispute của buyer, include đủ nav để dùng
            var disputes = await _context.Disputes
                .Where(d => d.RaisedBy == buyerId)
                .Include(d => d.Order)
                    .ThenInclude(o => o.Buyer)                   // User buyer
                .Include(d => d.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Seller)          // User seller
                .ToListAsync();
            

            // 2. Map sang DTO hoàn toàn bằng C#
            var result = disputes.Select(d =>
            {
                var order = d.Order;
                var parsed = DisputeDescriptionParser.Parse(d.Description);
                var products = order?.OrderItems?
                    .Select(oi => new DisputeProductDto
                    {
                        ProductId = (int)oi.ProductId,
                        ProductTitle = oi.Product.Title,
                        SellerName = oi.Product.Seller.Username
                    })
                    .ToList() ?? new List<DisputeProductDto>();

                return new DisputeListItemDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId ?? 0,
                    BuyerName = order?.Buyer?.Username,
                    SellerName = products.Select(p => p.SellerName).Distinct().FirstOrDefault(),
                    Description = d.Description,
                    ResolutionRequest = parsed.ResolutionRequest,
                    MainReason = parsed.MainReason,
                    DetailReason = parsed.DetailReason,
                    UserContent = parsed.UserContent,
                    Status = d.Status,
                    SubmittedDate = d.SubmittedDate,
                    Products = products,
                    Resolution = d.Resolution,
                    Comment = d.Comment,
                    SolvedDate = d.SolvedDate,
                };
            })
                // ❶ Sắp xếp: status 1 & 3 lên trước, còn lại xuống dưới
                .OrderBy(x => (x.Status.Equals("1") || x.Status.Equals("3")) ? 0 : 1)
                // ❷ Trong mỗi nhóm vẫn sort theo ngày gửi (mới nhất trước)
                .ThenByDescending(x => x.SubmittedDate ?? DateTime.MinValue)
                .ToList();


            return result;
        }

        public async Task<List<DisputeListItemDto>> GetDisputesBySellerAsync(int sellerId)
        {
            // 1. Lấy dispute mà trong order có ít nhất 1 product thuộc seller đó
            var disputes = await _context.Disputes
                .Include(d => d.Order)
                    .ThenInclude(o => o.Buyer)
                .Include(d => d.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Seller)
                .Where(d => d.Order.OrderItems
                    .Any(oi => oi.Product.SellerId == sellerId) && (d.Status.Equals("1")||d.Status.Equals("2")))
                .ToListAsync();

            // 2. Map sang DTO
            var result = disputes.Select(d =>
            {
                var order = d.Order;
                var parsed = DisputeDescriptionParser.Parse(d.Description);
                var products = order?.OrderItems?
                    .Where(oi => oi.Product.SellerId == sellerId)
                    .Select(oi => new DisputeProductDto
                    {
                        ProductId = (int)oi.ProductId,
                        ProductTitle = oi.Product.Title,
                        SellerName = oi.Product.Seller.Username
                    })
                    .ToList() ?? new List<DisputeProductDto>();

                return new DisputeListItemDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId ?? 0,
                    BuyerName = order?.Buyer?.Username,
                    SellerName = products.Select(p => p.SellerName).Distinct().FirstOrDefault(),
                    Description = d.Description,
                    ResolutionRequest = parsed.ResolutionRequest,
                    MainReason = parsed.MainReason,
                    DetailReason = parsed.DetailReason,
                    UserContent = parsed.UserContent,
                    Status = d.Status,
                    SubmittedDate = d.SubmittedDate,
                    Products = products,
                    Resolution = d.Resolution,
                    Comment = d.Comment,
                    SolvedDate = d.SolvedDate,
                };
            })
                .OrderBy(x => (x.Status.Equals("1")) ? 0 : 1)
                .OrderByDescending(x => x.SubmittedDate)
                .ToList();


            return result;
        }

        public async Task<List<DisputeListItemDto>> GetDisputesForSupporterAsync()
        {
            // 1. Lấy dispute mà trong order có ít nhất 1 product thuộc seller đó
            var disputes = await _context.Disputes
                .Include(d => d.Order)
                    .ThenInclude(o => o.Buyer)
                .Include(d => d.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Seller)
                .Where(d =>  (d.Status.Equals("3") || d.Status.Equals("4")))
                .ToListAsync();

            // 2. Map sang DTO
            var result = disputes.Select(d =>
            {
                var order = d.Order;
                var parsed = DisputeDescriptionParser.Parse(d.Description);
                var products = order?.OrderItems?
                    .Select(oi => new DisputeProductDto
                    {
                        ProductId = (int)oi.ProductId,
                        ProductTitle = oi.Product.Title,
                        SellerName = oi.Product.Seller.Username
                    })
                    .ToList() ?? new List<DisputeProductDto>();

                return new DisputeListItemDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId ?? 0,
                    BuyerName = order?.Buyer?.Username,
                    SellerName = products.Select(p => p.SellerName).Distinct().FirstOrDefault(),
                    Description = d.Description,
                    ResolutionRequest = parsed.ResolutionRequest,
                    MainReason = parsed.MainReason,
                    DetailReason = parsed.DetailReason,
                    UserContent = parsed.UserContent,
                    Status = d.Status,
                    SubmittedDate = d.SubmittedDate,
                    Products = products,
                    Resolution = d.Resolution,
                    Comment = d.Comment,
                    SolvedDate = d.SolvedDate,
                };
            })
                .OrderBy(x => (x.Status.Equals("3")) ? 0 : 1)
                .OrderByDescending(x => x.SubmittedDate)
                .ToList();


            return result;
        }

        public void RespondDispute(RespondDisputeDto respond)
        {
            var dispute = _context.Disputes.Where(x => x.Id == respond.Id).FirstOrDefault();

            if (dispute == null)
                throw new Exception("Không tìm thấy khiếu nại");

            _logger.LogInformation("Trước update: {@Before}", dispute);
            _logger.LogInformation("DTO nhận vào: {@Dto}", respond);

            dispute.Resolution = respond.Resolution;
            dispute.Comment = respond.Comment;
            dispute.Status = respond.status;        // OK vì DB string
            dispute.SolvedDate = DateTime.Now;

            // KHÔNG gọi Update() - đã tracking rồi
            var affected =  _context.SaveChangesAsync();

            _logger.LogInformation("SaveChangesAsync affected rows = {Count}", affected);
        }

        public async Task AutoEscalateDisputesAsync(int daysToEscalate)
        {
            var now = DateTime.UtcNow;

            var overdueDisputes = await _context.Disputes
                .Where(d => d.Status == "1"
                            && d.SubmittedDate != null
                            && EF.Functions.DateDiffDay(d.SubmittedDate.Value, now) >= daysToEscalate)
                .ToListAsync();

            if (overdueDisputes.Count == 0)
                return;

            foreach (var dispute in overdueDisputes)
            {
                dispute.Status = "3";
            }

            await _context.SaveChangesAsync();
        }

    }

}

