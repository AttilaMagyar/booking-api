namespace BookingApi.Dtos;

public record CreateResourceRequest(
    string Name,
    string Category,
    int Capacity
);

public record ResourceResponse(
    int Id,
    string Name,
    string Category,
    int Capacity,
    bool IsActive,
    DateTimeOffset CreatedAt
);
