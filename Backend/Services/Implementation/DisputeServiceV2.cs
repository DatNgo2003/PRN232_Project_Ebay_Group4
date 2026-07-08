using Backend.DTOs.Requests;
using Backend.Models;
using Backend.Repositories.Implementation;
using Backend.Repositories.Interface;
using Backend.Services.Interface;

namespace Backend.Services.Implementation
{
    public class DisputeServiceV2 : IDisputeServiceV2
    {
        private readonly IDisputeRepositoryV2 _disputeRepo;

        public DisputeServiceV2(IDisputeRepositoryV2 disputeRepo)
        {
            _disputeRepo = disputeRepo;
        }

        public async Task<IEnumerable<DisputableOrderDto>> GetDisputableOrdersAsync(int buyerId)
        {
            var orders = await _disputeRepo.GetDisputableOrdersAsync(buyerId);

            return orders.Select(o =>
            {
                var firstItem = o.OrderItems.FirstOrDefault();
                var product = firstItem?.Product;
                var seller = product?.Seller;
                var store = seller?.Stores.FirstOrDefault();

                return new DisputableOrderDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.Status,
                    PrimaryProductName = product?.Title ?? "Không có tên",
                    SellerName = store?.StoreName ?? seller?.Username ?? "Không rõ"
                };
            });
        }

        public async Task<Dispute> CreateDisputeAsync(DisputeCreateDto dto, int currentUserId)
        {
            var order = await _disputeRepo.GetOrderByIdAndBuyerAsync(dto.OrderId, currentUserId);
            if (order == null)
            {
                throw new Exception("Đơn hàng không tồn tại hoặc bạn không sở hữu đơn hàng này.");
            }

            if (await _disputeRepo.HasPendingDisputeAsync(dto.OrderId))
            {
                throw new Exception("Đơn hàng này đã có một khiếu nại đang chờ xử lý.");
            }

            var newDispute = new Dispute
            {
                OrderId = dto.OrderId,
                Description = dto.Description,
                RaisedBy = currentUserId,
                Status = "Pending", // Status là nvarchar(20)
                SubmittedDate = DateTime.UtcNow
            };

            return await _disputeRepo.AddDisputeAsync(newDispute);
        }

        public Task<Dispute> GetDisputeForOrderAsync(int orderId, int currentUserId)
        {
            throw new NotImplementedException();
        }
    }
    }

