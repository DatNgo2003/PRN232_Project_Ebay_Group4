namespace Backend.DTOs.Requests
{
    public class CreateAddressDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}