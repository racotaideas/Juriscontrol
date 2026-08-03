# JurisControl v2

Plataforma multi-tenant para el control de asuntos jurídicos. Reescritura del
sistema clásico de 1995 (ISAC Ingeniería · cobranza judicial bancaria) como
SaaS moderno, con soporte para cualquier tipo de despacho y modo cobranza
opcional.

**Documento de especificación**: <https://claude.ai/code/artifact/1b4058a7-b823-4ea4-a5ab-38c4fe05def7>

## Stack

- **.NET 8** (LTS) · ASP.NET Core Razor Pages + Web API
- **Entity Framework Core 8** con SQL Server 2022
- **ASP.NET Core Identity** con `DespachoId` en `ApplicationUser`
- **Multi-tenant**: EF Core Global Query Filters + SQL Server Row Level Security
- **Hangfire** (SQL Server) para jobs en background
- **Hosting**: SmartASP.NET (`site4now.net`), sitio `juriscontrol`
- **CI/CD**: GitHub Actions + Web Deploy

## Estructura

```
src/
├── JurisControl.Domain/        Entidades y enums. Sin dependencias.
├── JurisControl.Application/   Casos de uso, DTOs, servicios de dominio.
├── JurisControl.Data/          DbContext, Migrations, TenantContext, Configurations, Seeding.
├── JurisControl.Web/           Razor Pages + login. Punto de entrada web.
├── JurisControl.Api/           Web API para clientes externos.
└── JurisControl.Jobs/          Hangfire workers.
tests/
├── JurisControl.Domain.Tests/
└── JurisControl.Application.Tests/
```

## Cómo correr localmente

1. Instalar .NET 8 SDK.
2. Instalar SQL Server LocalDB (viene con Visual Studio o SSDT).
3. Clonar y correr:

   ```powershell
   git clone https://github.com/racotaideas/Juriscontrol.git
   cd Juriscontrol
   dotnet build
   dotnet run --project src/JurisControl.Web
   ```

4. Abrir <https://localhost:5001>. El despacho piloto se siembra en el primer arranque.

**Credenciales del despacho piloto** (definidas en `DbSeeder.cs`):
- Correo: `rafael.corona.tavarez@gmail.com`
- Contraseña inicial: `Cambiar1!Yaesto` — **rotarla en el primer login**.

## Multi-tenant en detalle

Ningún endpoint recibe `DespachoId` desde el request. Siempre se toma de la
claim `despacho_id` que se emite al login (ver `DespachoUserClaimsPrincipalFactory`)
y viaja en la cookie de autenticación. De ahí se propaga:

1. **EF Core Global Query Filters**: todas las entidades `ITenantEntity` se
   filtran automáticamente por `DespachoId` en cada consulta LINQ.
2. **SQL Server Row Level Security**: una política de seguridad instalada por
   `RowLevelSecurityInstaller` aplica `FILTER PREDICATE` y `BLOCK PREDICATE`
   sobre las tablas de negocio. El `TenantId` viaja como `SESSION_CONTEXT`
   fijado por `TenantSessionInterceptor` antes de cada comando SQL.

El único escape legítimo es `ITenantContext.EnterPlatformScope()` (IDisposable) —
reservado para el `DbSeeder`, migraciones y jobs de Hangfire de nivel plataforma.

## Despliegue

Push a `main` → GitHub Actions build + test + publish + Web Deploy a SmartASP.NET.
La cadena de conexión y los secretos viven en Environment Variables del panel
del sitio, no en `appsettings.json`.

## Licencia

Propietario de Racota Ideas. Todos los derechos reservados.
