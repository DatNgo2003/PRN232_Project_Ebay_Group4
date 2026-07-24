namespace Backend.DTOs;

public sealed record ShippingTaskMessage(
    int OrderId,
    int AddressId,
    int UserId,
    int ProductId,
    string TransactionId,
    DateTime EstimatedArrival
);
