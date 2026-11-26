using BookingApi.Data;
using BookingApi.Dtos;
using BookingApi.Models;
using BookingApi.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Tests;

public class ScheduleServiceTests
{
    private static BookingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BookingDbContext(options);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldRejectOverlappingConfirmedBooking()
    {
        await using var db = CreateDbContext();
        var service = new ScheduleService(db);

        var resource = await service.CreateResourceAsync(
            new CreateResourceRequest("Meeting Room A", "Room", 8));

        var start1 = DateTimeOffset.Parse("2026-12-01T10:00:00Z");
        var end1 = DateTimeOffset.Parse("2026-12-01T11:30:00Z");

        await service.CreateBookingAsync(
            new CreateBookingRequest(resource.Id, "alice@example.com", start1, end1, "Sprint planning"));

        // Átfedő idősáv: 11:00 - 12:00
        var overlappingStart = DateTimeOffset.Parse("2026-12-01T11:00:00Z");
        var overlappingEnd = DateTimeOffset.Parse("2026-12-01T12:00:00Z");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(
                new CreateBookingRequest(resource.Id, "bob@example.com", overlappingStart, overlappingEnd, "Conflict")));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldAllowBackToBackBookingAndAfterCancellation()
    {
        await using var db = CreateDbContext();
        var service = new ScheduleService(db);

        var resource = await service.CreateResourceAsync(
            new CreateResourceRequest("Focus Pod", "Room", 2));

        var t1 = DateTimeOffset.Parse("2026-12-02T09:00:00Z");
        var t2 = DateTimeOffset.Parse("2026-12-02T10:00:00Z");
        var t3 = DateTimeOffset.Parse("2026-12-02T11:00:00Z");

        var first = await service.CreateBookingAsync(
            new CreateBookingRequest(resource.Id, "user1@example.com", t1, t2, null));

        // Pontosan t2-kor kezdődik (10:00 -> 11:00): nem ütközik
        var backToBack = await service.CreateBookingAsync(
            new CreateBookingRequest(resource.Id, "user2@example.com", t2, t3, null));

        Assert.Equal(BookingStatus.Confirmed, backToBack.Status);

        // Ha az elsőt lemondjuk, az eredeti [09:00, 10:00) sáv újra foglalható
        await service.CancelBookingAsync(first.Id);

        var rebooked = await service.CreateBookingAsync(
            new CreateBookingRequest(resource.Id, "user3@example.com", t1, t2, "Rebooked after cancel"));

        Assert.Equal(BookingStatus.Confirmed, rebooked.Status);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldRejectWhenEndAtIsNotAfterStartAt()
    {
        await using var db = CreateDbContext();
        var service = new ScheduleService(db);

        var resource = await service.CreateResourceAsync(
            new CreateResourceRequest("Projector", "Equipment", 1));

        var sameTime = DateTimeOffset.Parse("2026-12-03T14:00:00Z");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateBookingAsync(
                new CreateBookingRequest(resource.Id, "user@example.com", sameTime, sameTime, null)));
    }
}
