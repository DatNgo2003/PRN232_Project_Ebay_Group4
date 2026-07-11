namespace Backend.Services.Implementation
{
    public static class RegionHelper
    {
        // Dùng Contains vì API trả "Thành phố Hà Nội", "Thành phố Hồ Chí Minh"
        public static bool IsInnerCity(string? city)
        {
            if (string.IsNullOrWhiteSpace(city)) return false;
            return city.Contains("Hà Nội", System.StringComparison.OrdinalIgnoreCase)
                || city.Contains("Hồ Chí Minh", System.StringComparison.OrdinalIgnoreCase)
                || city.Contains("Ha Noi", System.StringComparison.OrdinalIgnoreCase)
                || city.Contains("Ho Chi Minh", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}