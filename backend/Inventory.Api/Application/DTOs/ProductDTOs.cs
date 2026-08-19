namespace Inventory.Api.Application.DTOs;

public record CreateProductRequest(string Code, string Description, int Balance);

public record DecreaseBalanceRequest(int Quantity, string IdempotencyKey);

public record ReplenishBalanceRequest(int Quantity);

public record ProductResponse(Guid Id, string Code, string Description, int Balance);
