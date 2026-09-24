namespace BkkTennis.Api.Domain;

// Entities map 1:1 to db/init/001_schema.sql (snake_case via EFCore.NamingConventions).
// Generated columns (venues.location, *.time_range) are not mapped — they are used from raw SQL only.

public static class Roles
{
    public const string Player = "player";
    public const string PartnerStaff = "partner_staff";
    public const string PartnerOwner = "partner_owner";
    public const string PlatformAdmin = "platform_admin";
}

public static class BookingStatus
{
    public const string Held = "held";
    public const string Confirmed = "confirmed";
    public const string CheckedIn = "checked_in";
    public const string Completed = "completed";
    public const string Expired = "expired";
    public const string Cancelled = "cancelled";
    public const string NoShow = "no_show";

    public static readonly string[] Active = [Held, Confirmed, CheckedIn];
}

public class User
{
    public Guid Id { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string Locale { get; set; } = "th";
    public string Role { get; set; } = Roles.Player;
    /// <summary>bcrypt; only partner staff have one (players use OTP). Never serialized.</summary>
    public string? PasswordHash { get; set; }
    public int FailedLogins { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OtpCode
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = "";
    public string CodeHash { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Partner
{
    public Guid Id { get; set; }
    public string LegalName { get; set; } = "";
    public string? TaxId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public decimal CommissionRate { get; set; } = 0.10m;
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}

public class Venue
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public string Slug { get; set; } = "";
    public string NameTh { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string District { get; set; } = "";
    public string Address { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Timezone { get; set; } = "Asia/Bangkok";
    /// <summary>Raw jsonb array, e.g. ["floodlit","parking"]. Use <see cref="AmenityList"/> to read.</summary>
    public string Amenities { get; set; } = "[]";
    public string[] AmenityList() =>
        System.Text.Json.JsonSerializer.Deserialize<string[]>(Amenities) ?? [];
    public string? CoverImageUrl { get; set; }
    public string? TransitNote { get; set; }
    public decimal RatingAvg { get; set; }
    public int RatingCount { get; set; }
    public int CancelFreeHours { get; set; } = 4;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }

    public List<Court> Courts { get; set; } = [];
    public List<OperatingHours> OperatingHours { get; set; } = [];
    public List<PriceRule> PriceRules { get; set; } = [];
    public List<Addon> Addons { get; set; } = [];
}

public class Court
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string Name { get; set; } = "";
    public string Surface { get; set; } = "hard";
    public bool Indoor { get; set; }
    public bool AirCon { get; set; }
    public bool Floodlit { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class OperatingHours
{
    public Guid VenueId { get; set; }
    public short DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public short SlotMinutes { get; set; } = 60;
}

public class PriceRule
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public Guid? CourtId { get; set; }
    public string DayType { get; set; } = "any";
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal PricePerHour { get; set; }
    public string? Label { get; set; }
    public int Priority { get; set; }
}

public class Addon
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal Price { get; set; }
    public string Unit { get; set; } = "per_booking";
    public bool IsActive { get; set; } = true;
}

public class Booking
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public Guid? UserId { get; set; }
    public Guid VenueId { get; set; }
    public Venue? Venue { get; set; }
    public string Status { get; set; } = BookingStatus.Held;
    public string Source { get; set; } = "app";
    public string? CustomerName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal AddonTotal { get; set; }
    public decimal Total { get; set; }
    public decimal Commission { get; set; }
    /// <summary>The payout this completed booking was settled into (null = not yet paid out).</summary>
    public Guid? PayoutId { get; set; }
    public DateTime? HoldExpiresAt { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<BookingSlot> Slots { get; set; } = [];
    public List<BookingAddon> Addons { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}

public class BookingSlot
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CourtId { get; set; }
    public Court? Court { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal Price { get; set; }
    public string? Label { get; set; }
    public string Status { get; set; } = BookingStatus.Held;
}

public class BookingAddon
{
    public Guid BookingId { get; set; }
    public Guid AddonId { get; set; }
    public Addon? Addon { get; set; }
    public int Qty { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class CourtBlock
{
    public Guid Id { get; set; }
    public Guid CourtId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Reason { get; set; } = "maintenance";
    public string? Note { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string Provider { get; set; } = "";
    public string ProviderRef { get; set; } = "";
    public string Method { get; set; } = "promptpay";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public string? QrPayload { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? RawWebhook { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Refund
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}

public static class PayoutStatus
{
    public const string Pending = "pending";
    public const string Paid = "paid";
    public const string Failed = "failed";
}

public class Payout
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    /// <summary>Nominal week (Friday … Thursday, Bangkok dates). Completed bookings older than the week roll in too.</summary>
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal Gross { get; set; }
    public decimal Commission { get; set; }
    public decimal Net { get; set; }
    public string Status { get; set; } = PayoutStatus.Pending;
    public string? TransferRef { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VenueMember
{
    public Guid VenueId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "staff";
}
