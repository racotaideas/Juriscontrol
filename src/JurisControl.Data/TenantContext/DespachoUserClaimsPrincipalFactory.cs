using System.Security.Claims;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JurisControl.Data.TenantContext;

/// <summary>
/// Emite el claim <c>despacho_id</c> con el DespachoId del usuario al momento del login.
/// De ahí lo levanta el <see cref="HttpTenantContext"/> en cada request.
/// </summary>
public sealed class DespachoUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>
{
    public DespachoUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(HttpTenantContext.DespachoIdClaimType, user.DespachoId.ToString()));
        return identity;
    }
}
