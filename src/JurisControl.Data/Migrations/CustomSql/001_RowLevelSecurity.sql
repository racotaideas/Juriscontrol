-- ============================================================================
-- Row Level Security de SQL Server para JurisControl v2 · Fase 0
-- Segundo candado del multi-tenant (el primero son los Global Query Filters
-- de EF Core). Aunque una consulta LINQ escapara del filtro con
-- IgnoreQueryFilters(), esta política del motor rechaza filas de otros tenants.
--
-- El TenantId se transporta como SESSION_CONTEXT, seteado por un DbCommandInterceptor
-- de EF Core antes de cada query. Si SESSION_CONTEXT no está fijado, el predicado
-- devuelve 0 filas — falla segura.
--
-- IMPORTANTE: este script corre en CADA arranque. Debe ser idempotente:
-- - Si la policy ya existe, hay que deshabilitarla temporalmente porque
--   SQL Server no deja ALTER de la función mientras esté referenciada.
-- - Después se re-crean función y policy, y se deja la policy ON.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'security')
    EXEC('CREATE SCHEMA security');
GO

-- Si la policy ya existe, apagarla y quitar los predicados que referencian a la función.
-- Sin esto, CREATE OR ALTER FUNCTION falla con error 3729.
IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'TenantIsolationPolicy' AND schema_id = SCHEMA_ID('security'))
BEGIN
    ALTER SECURITY POLICY security.TenantIsolationPolicy WITH (STATE = OFF);
    DROP SECURITY POLICY security.TenantIsolationPolicy;
END
GO

-- Predicado que compara el DespachoId de la fila contra el SESSION_CONTEXT('TenantId').
-- Devuelve 1 si coinciden o si SESSION_CONTEXT('PlatformScope') está en 1 (jobs/seed).
CREATE OR ALTER FUNCTION security.fn_TenantAccessPredicate(@DespachoId uniqueidentifier)
    RETURNS TABLE
    WITH SCHEMABINDING
AS
RETURN SELECT 1 AS AccessOk
    WHERE
        CAST(SESSION_CONTEXT(N'PlatformScope') AS BIT) = 1
        OR @DespachoId = CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier);
GO

-- Recrear la policy. Como la borramos arriba, aquí siempre entra a CREATE.
CREATE SECURITY POLICY security.TenantIsolationPolicy
    ADD FILTER PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.Clientes,
    ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.Clientes AFTER INSERT,
    ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.Clientes AFTER UPDATE,
    ADD FILTER PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.AspNetUsers,
    ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.AspNetUsers AFTER INSERT,
    ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.AspNetUsers AFTER UPDATE
WITH (STATE = ON);
GO
