using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Bookings;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.Payments;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPayments(this RouteGroupBuilder api, IWebHostEnvironment env)
    {
        // Gateway → us. The provider validates the signature; status only ever changes here.
        api.MapPost("/payments/webhook/{provider}", async (
            string provider, HttpRequest request, IPaymentProvider payments, BookingService bookings, CancellationToken ct) =>
        {
            if (!string.Equals(provider, payments.Name, StringComparison.OrdinalIgnoreCase)) return Results.NotFound();

            using var reader = new StreamReader(request.Body);
            var raw = await reader.ReadToEndAsync(ct);
            var result = await payments.ParseWebhookAsync(request, raw, ct);
            if (result is null) return Results.Unauthorized();

            await bookings.ConfirmPaymentAsync(payments.Name, result.ProviderRef, result.Succeeded, raw, ct);
            return Results.Ok(new { received = true });
        }).AllowAnonymous().WithTags("Payments");

        if (env.IsDevelopment())
        {
            // Dev helper: pretend the customer scanned the QR and paid.
            api.MapPost("/dev/bookings/{id:guid}/simulate-payment", async (
                Guid id, AppDbContext db, IPaymentProvider payments, BookingService bookings, CancellationToken ct) =>
            {
                var pay = await db.Payments.Where(p => p.BookingId == id && p.Status == "pending")
                    .OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync(ct);
                if (pay is null) return Results.NotFound();
                await bookings.ConfirmPaymentAsync(pay.Provider, pay.ProviderRef, true, """{"simulated":true}""", ct);
                return Results.Ok(await BookingEndpoints.LoadDtoAsync(db, id, ct));
            }).WithTags("Dev");
        }

        return api;
    }
}
