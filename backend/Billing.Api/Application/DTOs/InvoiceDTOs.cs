namespace Billing.Api.Application.DTOs;

public record AddItemRequest(Guid ProductId, string ProductDescription, int Quantity);

public record UpdateItemRequest(int Quantity);

public record InvoiceItemResponse(Guid Id, Guid ProductId, string ProductDescription, int Quantity);

public record InvoiceResponse(
    Guid Id,
    int Number,
    string Status,
    DateTime CreatedDate,
    DateTime? PrintedDate,
    List<InvoiceItemResponse> Items);

public record PrintLogResponse(
    Guid Id,
    Guid InvoiceId,
    int InvoiceNumber,
    DateTime PrintedDate);
