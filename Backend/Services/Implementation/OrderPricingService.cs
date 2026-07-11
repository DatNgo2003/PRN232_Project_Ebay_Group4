using System;
using System.Threading.Tasks;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Exceptions;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class OrderPricingService : IOrderPricingService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly CloneEbayDbContext _context;
        private readonly IShippingFeeCalculator _shippingFeeCalculator;  

        public OrderPricingService(
            IProductRepository productRepository,
            ICouponRepository couponRepository,
            CloneEbayDbContext context,
            IShippingFeeCalculator shippingFeeCalculator)  
        {
            _productRepository = productRepository;
            _couponRepository = couponRepository;
            _context = context;
            _shippingFeeCalculator = shippingFeeCalculator;
        }

        public async Task<OrderPricingResultDto> CalculateAsync(CalculateOrderDto request)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new BusinessException("The order must contain at least one product.");

            decimal subTotal = 0;
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    throw new BusinessException($"Product quantity {item.ProductId} invalid");

                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    throw new BusinessException($"Product not found id={item.ProductId}");

                if (product.Price == null)
                    throw new BusinessException($"Product id={item.ProductId} don't not have yet available price");

                subTotal += product.Price.Value * item.Quantity;
            }

            decimal discount = 0;
            Coupon? appliedCoupon = null;

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                appliedCoupon = await _couponRepository.GetByCodeAsync(request.CouponCode);
                ValidateCoupon(appliedCoupon, request);
                discount = Math.Round(subTotal * (appliedCoupon!.DiscountPercent!.Value / 100m), 2);
            }

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId);
            if (address == null)
                throw new BusinessException($"Address not found id={request.AddressId}");

            decimal shippingFee = _shippingFeeCalculator.Calculate(address);

            decimal total = Math.Round(subTotal - discount + shippingFee, 2);
            if (total < 0) total = 0;

            return new OrderPricingResultDto
            {
                SubTotal = Math.Round(subTotal, 2),
                DiscountAmount = discount,
                ShippingFee = shippingFee,
                Total = total,
                AppliedCoupon = appliedCoupon?.Code,
                CouponId = appliedCoupon?.Id
            };
        }

        private void ValidateCoupon(Coupon? coupon, CalculateOrderDto request)
        {
            if (coupon == null)
                throw new BusinessException("The discount code does not exist.");

            var now = DateTime.UtcNow;
            if (coupon.StartDate.HasValue && coupon.StartDate > now)
                throw new BusinessException("The discount code is not yet valid.");
            if (coupon.EndDate.HasValue && coupon.EndDate < now)
                throw new BusinessException("The discount code has expired.");
            if (coupon.MaxUsage.HasValue && (coupon.UsedCount ?? 0) >= coupon.MaxUsage.Value)
                throw new BusinessException("The discount code has reached its usage limit.");
            if (coupon.DiscountPercent == null || coupon.DiscountPercent <= 0)
                throw new BusinessException("Invalid discount code");

            if (coupon.ProductId.HasValue)
            {
                bool hasProduct = request.Items.Exists(i => i.ProductId == coupon.ProductId.Value);
                if (!hasProduct)
                    throw new BusinessException("The discount code does not apply to the products in this order.");
            }
        }

    }
}