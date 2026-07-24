using Backend.Models;

namespace Backend.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(int userId);
    Task<Cart> GetOrCreateCartAsync(int userId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId);
    Task<CartItem> AddCartItemAsync(int cartId, int productId, int quantity);
    Task<CartItem> UpdateCartItemQuantityAsync(int cartItemId, int quantity);
    Task<bool> RemoveCartItemAsync(int cartItemId);
    Task<bool> ClearCartAsync(int cartId);
    Task<int> GetCartItemCountAsync(int cartId);
}
