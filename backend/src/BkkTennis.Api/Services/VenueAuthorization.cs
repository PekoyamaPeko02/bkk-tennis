using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Services;

/// <summary>
/// The caller must be a member of the venue named by the <c>{venueId}</c> route value (and, for <see cref="OwnerOnly"/>, its owner).
/// Membership rows in <c>venue_members</c> are the source of truth — the role claim only gates the portal as a whole.
/// </summary>
public sealed class VenueMemberRequirement(bool ownerOnly) : IAuthorizationRequirement
{
    public bool OwnerOnly { get; } = ownerOnly;
}

public sealed class VenueMemberHandler(AppDbContext db) : AuthorizationHandler<VenueMemberRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VenueMemberRequirement requirement)
    {
        // Never throws: this handler also runs for anonymous callers, who must end up as 401, not 500.
        if (context.Resource is not HttpContext http || context.User.Identity?.IsAuthenticated != true) return;
        if (!Guid.TryParse(http.Request.RouteValues["venueId"]?.ToString(), out var venueId)) return;
        if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                out var userId)) return;

        var isMember = await db.VenueMembers.AsNoTracking().AnyAsync(m =>
            m.VenueId == venueId && m.UserId == userId && (!requirement.OwnerOnly || m.Role == "owner"), http.RequestAborted);
        if (isMember) context.Succeed(requirement);
    }
}

public static class VenueAuthorization
{
    /// <summary>Any member of the venue in the route (staff or owner).</summary>
    public const string Member = "VenueMember";
    /// <summary>Only the venue's owner (e.g. price changes).</summary>
    public const string Owner = "VenueOwner";

    public static IServiceCollection AddVenueAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, VenueMemberHandler>();
        services.AddAuthorization(o =>
        {
            o.AddPolicy(Member, p => p.RequireAuthenticatedUser()
                .RequireRole(Roles.PartnerStaff, Roles.PartnerOwner)
                .AddRequirements(new VenueMemberRequirement(ownerOnly: false)));
            o.AddPolicy(Owner, p => p.RequireAuthenticatedUser()
                .RequireRole(Roles.PartnerStaff, Roles.PartnerOwner)
                .AddRequirements(new VenueMemberRequirement(ownerOnly: true)));
        });
        return services;
    }
}
