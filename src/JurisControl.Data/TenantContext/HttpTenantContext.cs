using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace JurisControl.Data.TenantContext;

/// <summary>
/// Implementación de <see cref="ITenantContext"/> para el HTTP pipeline de ASP.NET Core.
/// Lee el claim <c>despacho_id</c> del <see cref="ClaimsPrincipal"/> del usuario
/// autenticado. Ese claim se emite al iniciar sesión (ver JurisControlUserClaimsPrincipalFactory).
/// </summary>
public sealed class HttpTenantContext : ITenantContext
{
    public const string DespachoIdClaimType = "despacho_id";
    public const string PlatformScopeClaimType = "platform_scope";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? DespachoId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.User?.FindFirst(DespachoIdClaimType)?.Value;
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsPlatformScope =>
        _httpContextAccessor.HttpContext?.User?.HasClaim(PlatformScopeClaimType, "true") == true;
}

/// <summary>
/// Contexto de tenant para operaciones fuera del pipeline HTTP (jobs de Hangfire, seed,
/// migraciones). Se instancia por el proceso que sabe qué tenant está procesando.
/// </summary>
public sealed class BackgroundTenantContext : ITenantContext
{
    public BackgroundTenantContext(Guid? despachoId = null, bool platformScope = false)
    {
        DespachoId = despachoId;
        IsPlatformScope = platformScope;
    }

    public Guid? DespachoId { get; }
    public bool IsPlatformScope { get; }
}
