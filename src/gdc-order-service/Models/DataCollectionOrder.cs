public record DataCollectionOrder(
    Guid OrderId,
    string Vin,
    string DataScope,
    string Status,
    DateTime CreatedAt
);
