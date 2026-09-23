using BkkTennis.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Data;

/// <summary>
/// Schema is owned by SQL scripts in /db (not EF migrations) because it relies on
/// PostGIS, generated columns and an exclusion constraint. Keep this mapping in sync.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<VenueMember> VenueMembers => Set<VenueMember>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<OperatingHours> OperatingHours => Set<OperatingHours>();
    public DbSet<PriceRule> PriceRules => Set<PriceRule>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSlot> BookingSlots => Set<BookingSlot>();
    public DbSet<BookingAddon> BookingAddons => Set<BookingAddon>();
    public DbSet<CourtBlock> CourtBlocks => Set<CourtBlock>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Refund> Refunds => Set<Refund>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });

        b.Entity<OtpCode>(e =>
        {
            e.ToTable("otp_codes");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });

        b.Entity<Partner>(e =>
        {
            e.ToTable("partners");
            e.Property(x => x.CommissionRate).HasPrecision(5, 4);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });

        b.Entity<Venue>(e =>
        {
            e.ToTable("venues");
            e.Property(x => x.Amenities).HasColumnType("jsonb");
            e.Property(x => x.RatingAvg).HasPrecision(2, 1);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.HasMany(x => x.Courts).WithOne().HasForeignKey(x => x.VenueId);
            e.HasMany(x => x.OperatingHours).WithOne().HasForeignKey(x => x.VenueId);
            e.HasMany(x => x.PriceRules).WithOne().HasForeignKey(x => x.VenueId);
            e.HasMany(x => x.Addons).WithOne().HasForeignKey(x => x.VenueId);
        });

        b.Entity<VenueMember>(e =>
        {
            e.ToTable("venue_members");
            e.HasKey(x => new { x.VenueId, x.UserId });
        });

        b.Entity<Court>().ToTable("courts");

        b.Entity<OperatingHours>(e =>
        {
            e.ToTable("operating_hours");
            e.HasKey(x => new { x.VenueId, x.DayOfWeek });
        });

        b.Entity<PriceRule>(e =>
        {
            e.ToTable("price_rules");
            e.Property(x => x.PricePerHour).HasPrecision(10, 2);
        });

        b.Entity<Addon>(e =>
        {
            e.ToTable("addons");
            e.Property(x => x.Price).HasPrecision(10, 2);
        });

        b.Entity<Booking>(e =>
        {
            e.ToTable("bookings");
            // "" is the CLR default: tell EF it means "not set" so the DB default generates the code
            // (otherwise the first booking is stored with code '' and the second violates the unique key).
            e.Property(x => x.Code).HasDefaultValueSql("('BT' || nextval('booking_code_seq'))").HasSentinel("").ValueGeneratedOnAdd();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            e.HasMany(x => x.Slots).WithOne().HasForeignKey(x => x.BookingId);
            e.HasMany(x => x.Addons).WithOne().HasForeignKey(x => x.BookingId);
            e.HasMany(x => x.Payments).WithOne().HasForeignKey(x => x.BookingId);
        });

        b.Entity<BookingSlot>().ToTable("booking_slots");

        b.Entity<BookingAddon>(e =>
        {
            e.ToTable("booking_addons");
            e.HasKey(x => new { x.BookingId, x.AddonId });
        });

        b.Entity<CourtBlock>().ToTable("court_blocks");

        b.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.Property(x => x.RawWebhook).HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });

        b.Entity<Refund>(e =>
        {
            e.ToTable("refunds");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}
