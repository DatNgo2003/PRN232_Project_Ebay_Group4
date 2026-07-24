using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CloneEbayDbContext _context;

    public CartRepository(CloneEbayDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p!.Seller)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
    }

    public async Task<CartItem> AddCartItemAsync(int cartId, int productId, int quantity)
    {
        var existingItem = await GetCartItemAsync(cartId, productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            existingItem.AddedAt = DateTime.UtcNow;
        }
        else
        {
            existingItem = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                AddedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(existingItem);
        }

        await _context.SaveChangesAsync();
        return existingItem;
    }

    public async Task<CartItem> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
    {
        var cartItem = await _context.CartItems.FindAsync(cartItemId);
        if (cartItem == null)
            throw new KeyNotFoundException("Cart item not found");

        cartItem.Quantity = quantity;
        await _context.SaveChangesAsync();
        return cartItem;
    }

    public async Task<bool> RemoveCartItemAsync(int cartItemId)
    {
        var cartItem = await _context.CartItems.FindAsync(cartItemId);
        if (cartItem == null) return false;

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int cartId)
    {
        var cartItems = await _context.CartItems
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCartItemCountAsync(int cartId)
    {
        return await _context.CartItems
            .Where(ci => ci.CartId == cartId)
            .SumAsync(ci => ci.Quantity);
    }
}
