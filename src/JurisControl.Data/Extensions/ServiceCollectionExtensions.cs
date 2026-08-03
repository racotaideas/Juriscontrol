using JurisControl.Data.Services;
using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JurisControl.Data.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra el DbContext de JurisControl, ASP.NET Core Identity y el ITenantContext
    /// que resuelve el DespachoId del usuario autenticado desde el HTTP pipeline.
    /// </summary>
    public static IServiceCollection AddJurisControlData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddScoped<TenantSessionInterceptor>();

        services.AddDbContext<JurisControlDbContext>((sp, options) =>
            options
                .UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(JurisControlDbContext).Assembly.GetName().Name);
                    sql.EnableRetryOnFailure(maxRetryCount: 3);
                })
                .AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>()));

        services.AddScoped<IFolioService, FolioService>();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false; // fase 0 — luego a true con correo
            })
            .AddEntityFrameworkStores<JurisControlDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
