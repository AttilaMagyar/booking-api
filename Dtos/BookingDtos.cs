using BookingApi.Models;

namespace BookingApi.Dtos;

public record CreateBookingRequest(
    int ResourceId,
    string BookedByEmail,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes
);

public record BookingResponse(
    int Id,
    int ResourceId,
    string ResourceName,
    string BookedByEmail,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    BookingStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt
);
