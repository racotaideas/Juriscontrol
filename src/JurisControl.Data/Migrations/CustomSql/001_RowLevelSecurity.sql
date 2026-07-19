-- ============================================================================
-- Row Level Security de SQL Server para JurisControl v2 · Fase 0
-- Segundo candado del multi-tenant (el primero son los Global Query Filters
-- de EF Core). Aunque una consulta LINQ escapara del filtro con
-- IgnoreQueryFilters(), esta política del motor rechaza filas de otros tenants.
--
-- El TenantId se transporta como SESSION_CONTEXT, seteado por un DbCommandInterceptor
-- de EF Core antes de cada query. Si SESSION_CONTEXT no está fijado, el predicado
-- devuelve 0 filas — falla segura.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'security')
    EXEC('CREATE SCHEMA security');
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

-- Aplicar el predicado como política sobre todas las tablas con columna DespachoId.
-- Al añadir tablas nuevas hay que extender esta política.
IF NOT EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'TenantIsolationPolicy')
BEGIN
    CREATE SECURITY POLICY security.TenantIsolationPolicy
        ADD FILTER PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.Clientes,
        ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.Clientes AFTER INSERT,
        ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.Clientes AFTER UPDATE,
        ADD FILTER PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.AspNetUsers,
        ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.AspNetUsers AFTER INSERT,
        ADD BLOCK  PREDICATE security.fn_TenantAccessPredicate(DespachoId) ON dbo.AspNetUsers AFTER UPDATE
    WITH (STATE = ON);
END
GO
