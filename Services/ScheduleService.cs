using BookingApi.Data;
using BookingApi.Dtos;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Services;

public class ScheduleService(BookingDbContext db)
{
    public async Task<ResourceResponse> CreateResourceAsync(CreateResourceRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Resource name is required.");
        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Resource category is required.");
        if (request.Capacity <= 0)
            throw new ArgumentException("Resource capacity must be greater than zero.");

        var normalizedName = request.Name.Trim();
        var exists = await db.Resources.AnyAsync(r => r.Name.ToLower() == normalizedName.ToLower(), ct);
        if (exists)
            throw new InvalidOperationException($"Resource with name '{normalizedName}' already exists.");

        var resource = new Resource
        {
            Name = normalizedName,
            Category = request.Category.Trim(),
            Capacity = request.Capacity,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Resources.Add(resource);
        await db.SaveChangesAsync(ct);

        return ToResourceResponse(resource);
    }

    public async Task<List<ResourceResponse>> GetResourcesAsync(CancellationToken ct = default)
    {
        var resources = await db.Resources
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        return resources.Select(ToResourceResponse).ToList();
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.BookedByEmail) || !request.BookedByEmail.Contains('@'))
            throw new ArgumentException("Valid BookedByEmail is required.");
        if (request.EndAt <= request.StartAt)
            throw new ArgumentException("EndAt must be later than StartAt.");

        var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId, ct);
        if (resource is null)
            throw new KeyNotFoundException($"Resource with id {request.ResourceId} was not found.");
        if (!resource.IsActive)
            throw new InvalidOperationException("Cannot book an inactive resource.");

        // Időintervallum-ütközés ellenőrzése a meglévő aktív (Confirmed) foglalásokkal
        var hasConflict = await db.Bookings
            .AsNoTracking()
            .AnyAsync(b =>
                b.ResourceId == request.ResourceId &&
                b.Status == BookingStatus.Confirmed &&
                b.StartAt < request.EndAt &&
                b.EndAt > request.StartAt,
                ct);

        if (hasConflict)
            throw new InvalidOperationException("Requested time slot overlaps with an existing confirmed booking.");

        var booking = new Booking
        {
            ResourceId = resource.Id,
            Resource = resource,
            BookedByEmail = request.BookedByEmail.Trim().ToLowerInvariant(),
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Status = BookingStatus.Confirmed,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        return ToBookingResponse(booking, resource.Name);
    }

    public async Task<List<BookingResponse>> GetBookingsAsync(int? resourceId, CancellationToken ct = default)
    {
        var query = db.Bookings
            .AsNoTracking()
            .Include(b => b.Resource)
            .AsQueryable();

        if (resourceId.HasValue)
            query = query.Where(b => b.ResourceId == resourceId.Value);

        var bookings = await query
            .OrderBy(b => b.StartAt)
            .ToListAsync(ct);

        return bookings
            .Select(b => ToBookingResponse(b, b.Resource?.Name ?? string.Empty))
            .ToList();
    }

    public async Task<BookingResponse> CancelBookingAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.Resource)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");

        if (booking.Status == BookingStatus.Cancelled)
            return ToBookingResponse(booking, booking.Resource?.Name ?? string.Empty);

        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        return ToBookingResponse(booking, booking.Resource?.Name ?? string.Empty);
    }

    private static ResourceResponse ToResourceResponse(Resource r) =>
        new(r.Id, r.Name, r.Category, r.Capacity, r.IsActive, r.CreatedAt);

    private static BookingResponse ToBookingResponse(Booking b, string resourceName) =>
        new(b.Id, b.ResourceId, resourceName, b.BookedByEmail, b.StartAt, b.EndAt, b.Status, b.Notes, b.CreatedAt);
}
