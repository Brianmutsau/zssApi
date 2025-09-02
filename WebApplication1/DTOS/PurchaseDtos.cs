// Dtos/PurchaseDtos.cs
namespace WebApplication1.Dtos;

public record PurchaseCard(string Id, DateOnly Expiry);

public record PurchaseRequest(
    string Type,
    string ExtendedType,
    decimal Amount,
    DateTime Created,
    PurchaseCard Card,
    string Reference,
    string Narration,
    Dictionary<string, object>? AdditionalData);

public record PurchaseResponse(
    DateTime Updated,
    string ResponseCode,
    string ResponseDescription,
    string Reference,
    string DebitReference);
