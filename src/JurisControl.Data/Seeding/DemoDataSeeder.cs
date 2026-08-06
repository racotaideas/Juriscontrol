using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JurisControl.Data.Seeding;

/// <summary>
/// Siembra datos ficticios pero realistas para los 3 despachos del piloto.
/// Cada despacho recibe clientes, asuntos, juicios con etapas procesales
/// que avanzan de 2025 a 2026, gestiones, pagos, plantillas y gastos.
///
/// Además inserta en el despacho piloto los 6 CASOS FEATURED narrativos
/// de <see cref="CasosNarrativos"/> — juicios con actuaciones y promociones
/// escritas en lenguaje jurídico real, desde la demanda hasta la sentencia
/// (algunos hasta segunda instancia).
///
/// Se activa solo si <c>DemoData:Enabled</c> es true. Idempotente por despacho.
/// </summary>
public static class DemoDataSeeder
{
    private static readonly Random Rng = new(2026);

    private static readonly string[] JuzgadosCivil =
    {
        "Juzgado 1° de lo Civil, CDMX", "Juzgado 8° de lo Civil, CDMX",
        "Juzgado 15° de lo Civil, CDMX", "Juzgado 22° de lo Civil, CDMX",
        "Juzgado 32° de lo Civil, CDMX", "Juzgado 58° de lo Civil, CDMX"
    };
    private static readonly string[] JuzgadosMercantil =
    {
        "Juzgado 3° de lo Mercantil, CDMX", "Juzgado 6° de lo Mercantil, CDMX",
        "Juzgado 10° de lo Mercantil, CDMX", "Juzgado 18° de lo Mercantil, CDMX",
        "Juzgado 34° de lo Mercantil, CDMX"
    };
    private static readonly string[] JuzgadosFamiliar =
    {
        "Juzgado 4° de lo Familiar, CDMX", "Juzgado 12° de lo Familiar, CDMX",
        "Juzgado 19° de lo Familiar, CDMX"
    };
    private static readonly string[] JuzgadosLaboral =
    {
        "Junta 5 de Conciliación y Arbitraje CDMX",
        "Juzgado 2° del Trabajo, Centro Federal, CDMX"
    };
    private static readonly string[] JuzgadosAmparo =
    {
        "Juzgado 4° de Distrito en Materia Administrativa, CDMX",
        "Juzgado 9° de Distrito en Materia Civil, CDMX"
    };

    private static readonly string[] NombresHombres =
    { "Roberto", "Carlos", "José", "Luis", "Fernando", "Miguel", "Javier",
      "Ricardo", "Alejandro", "Eduardo", "Rafael", "Jorge", "Andrés", "Sergio" };
    private static readonly string[] NombresMujeres =
    { "María", "Ana", "Patricia", "Sofía", "Gabriela", "Adriana", "Laura",
      "Mónica", "Claudia", "Verónica", "Alejandra", "Lucía", "Rosa", "Isabel" };
    private static readonly string[] Apellidos =
    { "Hernández", "García", "Martínez", "López", "González", "Rodríguez",
      "Pérez", "Sánchez", "Ramírez", "Torres", "Flores", "Rivera", "Gómez" };
    private static readonly string[] Empresas =
    { "Constructora del Valle S.A. de C.V.", "Distribuidora Norte S.A. de C.V.",
      "Servicios Integrales Peninsulares S.C.", "Grupo Corporativo Alameda S.A.P.I.",
      "Textiles Industriales de México S.A.", "Alimentos Selectos del Bajío S. de R.L.",
      "Inmobiliaria Reforma 2000 S.A. de C.V.", "Transportes Especializados S.A.",
      "Metalúrgica del Pacífico S.A. de C.V.", "Farmacéutica Regional S.A." };

    private static readonly (string Tipo, string MateriaKey, string DescripcionBase)[] TiposDeAsunto =
    {
        ("Ordinario Mercantil",       Materia.Mercantil, "Reclamación por incumplimiento de contrato de prestación de servicios."),
        ("Ejecutivo Mercantil",       Materia.Mercantil, "Ejecución de pagaré vencido con intereses moratorios."),
        ("Ordinario Civil",           Materia.Civil,     "Cumplimiento forzoso de contrato de arrendamiento."),
        ("Especial de Desahucio",     Materia.Civil,     "Desocupación de inmueble por falta de pago de rentas."),
        ("Amparo Indirecto",          Materia.Amparo,    "Amparo contra acto de autoridad administrativa."),
        ("Ordinario Laboral",         Materia.Laboral,   "Reclamo por despido injustificado."),
        ("Divorcio Necesario",        Materia.Familiar,  "Divorcio con controversia."),
        ("Cobro de Crédito Bancario", Materia.Cobranza,  "Recuperación de crédito con garantía hipotecaria.")
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        if (!config.GetValue<bool>("DemoData:Enabled")) return;

        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DemoDataSeeder");

        var tenant = sp.GetRequiredService<ITenantContext>();
        using var _platform = tenant.EnterPlatformScope();

        var db = sp.GetRequiredService<JurisControlDbContext>();

        foreach (var d in DbSeeder.Despachos)
        {
            var yaHay = await db.Asuntos.IgnoreQueryFilters().CountAsync(a => a.DespachoId == d.Id);
            if (yaHay >= 20)
            {
                logger.LogInformation("DemoDataSeeder skip despacho {D}: ya hay {N} asuntos.", d.RazonSocial, yaHay);
                continue;
            }

            logger.LogInformation("DemoDataSeeder sembrando despacho '{D}'…", d.RazonSocial);
            await SembrarDespachoAsync(db, d, logger);
        }
    }

    private static async Task SembrarDespachoAsync(
        JurisControlDbContext db, DbSeeder.DespachoDemo d, ILogger logger)
    {
        // Usuarios del despacho para poder asignar responsables
        var usuarios = await db.Users.IgnoreQueryFilters()
            .Where(u => u.DespachoId == d.Id && u.Activo)
            .ToListAsync();

        // --- 1. Clientes ---
        var clientes = new List<Cliente>();
        for (int i = 0; i < 20; i++)
        {
            var esHombre = Rng.Next(2) == 0;
            var nombre = esHombre ? NombresHombres[Rng.Next(NombresHombres.Length)]
                                   : NombresMujeres[Rng.Next(NombresMujeres.Length)];
            var apP = Apellidos[Rng.Next(Apellidos.Length)];
            var apM = Apellidos[Rng.Next(Apellidos.Length)];
            clientes.Add(new Cliente
            {
                DespachoId = d.Id,
                Tipo = TipoCliente.PersonaFisica,
                Nombre = nombre, ApellidoPaterno = apP, ApellidoMaterno = apM,
                Rfc = $"{apP.Substring(0, 2).ToUpper()}{apM[0]}{nombre[0]}{Rng.Next(600000, 999999)}",
                CorreoPrincipal = $"{nombre.ToLower()}.{apP.ToLower()}@correo.mx",
                TelefonoPrincipal = $"55{Rng.Next(10000000, 99999999)}",
                Ciudad = "Ciudad de México", Estado = "CDMX",
                CodigoPostal = $"0{Rng.Next(1000, 9999)}",
                Direccion = $"Calle {Apellidos[Rng.Next(Apellidos.Length)]} #{Rng.Next(1, 500)}",
                Etiquetas = Rng.Next(4) == 0 ? "VIP" : "activo",
                Activo = true, CreatedBy = "demo",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-Rng.Next(1, 18))
            });
        }
        for (int i = 0; i < 10; i++)
        {
            var razon = Empresas[i % Empresas.Length];
            clientes.Add(new Cliente
            {
                DespachoId = d.Id,
                Tipo = TipoCliente.PersonaMoral,
                RazonSocial = razon,
                NombreComercial = razon.Split(' ')[0],
                RepresentanteLegal = $"{NombresHombres[Rng.Next(NombresHombres.Length)]} {Apellidos[Rng.Next(Apellidos.Length)]}",
                Rfc = $"{razon.Substring(0, 3).ToUpper()}{Rng.Next(600000, 999999)}",
                CorreoPrincipal = $"contacto@{razon.Split(' ')[0].ToLower()}.mx",
                TelefonoPrincipal = $"55{Rng.Next(10000000, 99999999)}",
                Ciudad = "Ciudad de México", Estado = "CDMX",
                Etiquetas = "corporativo",
                Activo = true, CreatedBy = "demo",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-Rng.Next(1, 18))
            });
        }
        db.Clientes.AddRange(clientes);
        await db.SaveChangesAsync();
        var clientesGuardados = await db.Clientes.IgnoreQueryFilters()
            .Where(c => c.DespachoId == d.Id)
            .OrderByDescending(c => c.CreatedAt).Take(clientes.Count).ToListAsync();

        // Contadores de folio
        var contador2025 = await EnsureContadorAsync(db, d.Id, 2025);
        var contador2026 = await EnsureContadorAsync(db, d.Id, 2026);

        // --- 2. CASOS FEATURED (solo en el despacho piloto principal) ---
        if (d.Id == DbSeeder.Despachos[0].Id)
        {
            await SembrarCasosFeaturedAsync(db, d, clientesGuardados, usuarios,
                contador2025, contador2026, logger);
        }

        // --- 3. Asuntos "de fondo" para dar volumen ---
        var (asuntos, juicios, actuaciones, promociones, audiencias, plazos) =
            await GenerarAsuntosDeFondoAsync(db, d, clientesGuardados, usuarios,
                contador2025, contador2026);

        // --- 4. Cobranza (solo si el despacho tiene modo cobranza) ---
        if (d.ModoCobranza)
        {
            await SembrarCobranzaAsync(db, d, asuntos, clientesGuardados);
        }

        // --- 5. Plantillas iniciales (solo en el piloto principal) ---
        if (d.Id == DbSeeder.Despachos[0].Id)
        {
            await SembrarPlantillasAsync(db, d);
        }

        // --- 6. Gastos ---
        await SembrarGastosAsync(db, d, juicios);

        await db.SaveChangesAsync();
        logger.LogInformation("Despacho '{D}' listo: {C} clientes, {A} asuntos, {J} juicios.",
            d.RazonSocial, clientes.Count, asuntos.Count, juicios.Count);
    }

    private static async Task<ContadorFolio> EnsureContadorAsync(
        JurisControlDbContext db, Guid despachoId, int anio)
    {
        var existente = await db.ContadoresFolio.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.DespachoId == despachoId && c.Anio == anio);
        if (existente is not null) return existente;
        var nuevo = new ContadorFolio
        {
            DespachoId = despachoId, Anio = anio, UltimoNumero = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.ContadoresFolio.Add(nuevo);
        await db.SaveChangesAsync();
        return nuevo;
    }

    // ============================================================
    // CASOS FEATURED — historias narrativas
    // ============================================================
    private static async Task SembrarCasosFeaturedAsync(
        JurisControlDbContext db, DbSeeder.DespachoDemo d,
        List<Cliente> clientes, List<ApplicationUser> usuarios,
        ContadorFolio contador2025, ContadorFolio contador2026,
        ILogger logger)
    {
        var fechaHoy = DateTime.UtcNow.Date;

        foreach (var caso in CasosNarrativos.Casos)
        {
            var fechaInicio = fechaHoy.AddDays(-caso.DiasInicioAtras);
            var anioFolio = fechaInicio.Year;
            var cliente = clientes[Rng.Next(clientes.Count)];
            var responsable = usuarios.Count > 0 ? usuarios[Rng.Next(usuarios.Count)] : null;

            int folioNumero;
            if (anioFolio == 2025) { contador2025.UltimoNumero++; folioNumero = contador2025.UltimoNumero; }
            else { contador2026.UltimoNumero++; folioNumero = contador2026.UltimoNumero; }

            var asunto = new Asunto
            {
                DespachoId = d.Id,
                Folio = $"JC-{anioFolio}-{folioNumero:D4}",
                Titulo = "★ " + caso.Titulo,
                MateriaKey = caso.Materia,
                ClienteId = cliente.Id,
                ResponsableId = responsable?.Id,
                Estado = caso.EstadoAsuntoFinal,
                FechaRecepcion = new DateTimeOffset(fechaInicio, TimeSpan.Zero),
                Descripcion = caso.Descripcion,
                Cuantia = caso.Cuantia > 0 ? caso.Cuantia : null,
                Prioridad = 1,
                Etiquetas = "featured,demo",
                EsCobranza = caso.Materia == Materia.Cobranza,
                CreatedBy = "demo-narrativo",
                CreatedAt = new DateTimeOffset(fechaInicio, TimeSpan.Zero)
            };
            db.Asuntos.Add(asunto);
            await db.SaveChangesAsync();

            var juicioFechaInicio = fechaInicio.AddDays(3);
            var juicioAnio = juicioFechaInicio.Year;
            var numExp = Rng.Next(100, 999);

            var juicio = new Juicio
            {
                DespachoId = d.Id,
                AsuntoId = asunto.Id,
                NumeroExpediente = $"{numExp}/{juicioAnio}",
                Juzgado = caso.Juzgado,
                TipoJuicio = caso.TipoJuicio,
                MateriaKey = caso.Materia,
                Estado = caso.EstadoJuicioFinal,
                FechaInicio = DateOnly.FromDateTime(juicioFechaInicio),
                FechaConclusion = caso.EstadoJuicioFinal == EstadoJuicio.Concluido
                    ? DateOnly.FromDateTime(juicioFechaInicio.AddDays(caso.Actuaciones.Last().OffsetDias))
                    : null,
                Cuantia = caso.Cuantia > 0 ? caso.Cuantia : null,
                Descripcion = caso.Descripcion,
                CreatedBy = "demo-narrativo",
                CreatedAt = new DateTimeOffset(juicioFechaInicio, TimeSpan.Zero)
            };
            db.Juicios.Add(juicio);
            await db.SaveChangesAsync();

            // Partes: Actor (nuestro cliente) + Demandado (el mencionado en el caso)
            db.PartesJuicio.Add(new ParteJuicio
            {
                DespachoId = d.Id, JuicioId = juicio.Id, Rol = RolProcesal.Actor,
                ClienteId = cliente.Id, NombreLibre = caso.NombreActor,
                CreatedBy = "demo-narrativo", CreatedAt = juicio.CreatedAt
            });
            db.PartesJuicio.Add(new ParteJuicio
            {
                DespachoId = d.Id, JuicioId = juicio.Id, Rol = RolProcesal.Demandado,
                NombreLibre = caso.NombreDemandado,
                Representante = $"Lic. {Apellidos[Rng.Next(Apellidos.Length)]}",
                CreatedBy = "demo-narrativo", CreatedAt = juicio.CreatedAt
            });

            // Actuaciones con texto rico
            foreach (var a in caso.Actuaciones)
            {
                var fecha = juicioFechaInicio.AddDays(a.OffsetDias);
                if (fecha > fechaHoy) continue;
                db.Actuaciones.Add(new Actuacion
                {
                    DespachoId = d.Id, JuicioId = juicio.Id,
                    Tipo = a.Tipo, Fecha = DateOnly.FromDateTime(fecha),
                    Resumen = a.Resumen, Detalle = a.Detalle,
                    CreatedBy = "demo-narrativo",
                    CreatedAt = new DateTimeOffset(fecha, TimeSpan.Zero)
                });
            }

            // Promociones
            foreach (var p in caso.Promociones)
            {
                var fecha = juicioFechaInicio.AddDays(p.OffsetDias);
                if (fecha > fechaHoy) continue;
                db.Promociones.Add(new Promocion
                {
                    DespachoId = d.Id, JuicioId = juicio.Id,
                    Tipo = p.Tipo, FechaPresentacion = DateOnly.FromDateTime(fecha),
                    Titulo = p.Titulo, Contenido = p.Contenido,
                    FirmanteId = responsable?.Id,
                    NumeroAcuse = $"AC-{Rng.Next(100000, 999999)}",
                    CreatedBy = "demo-narrativo",
                    CreatedAt = new DateTimeOffset(fecha, TimeSpan.Zero)
                });
            }
            await db.SaveChangesAsync();
        }

        logger.LogInformation("{N} casos featured sembrados en '{D}'.",
            CasosNarrativos.Casos.Length, d.RazonSocial);
    }

    // ============================================================
    // Asuntos de fondo (volumen aleatorio)
    // ============================================================
    private static async Task<(List<Asunto>, List<Juicio>, List<Actuacion>, List<Promocion>, List<Audiencia>, List<Plazo>)>
        GenerarAsuntosDeFondoAsync(
            JurisControlDbContext db, DbSeeder.DespachoDemo d,
            List<Cliente> clientes, List<ApplicationUser> usuarios,
            ContadorFolio contador2025, ContadorFolio contador2026)
    {
        var estadosAsuntoDist = new[]
        {
            EstadoAsunto.Activo, EstadoAsunto.Activo, EstadoAsunto.Activo,
            EstadoAsunto.Asignado, EstadoAsunto.Recibido,
            EstadoAsunto.EnEspera, EstadoAsunto.Cerrado, EstadoAsunto.Cerrado
        };
        var estadosJuicioDist = new[]
        {
            EstadoJuicio.Iniciado, EstadoJuicio.EnPruebas, EstadoJuicio.EnPruebas,
            EstadoJuicio.Alegatos, EstadoJuicio.Sentencia,
            EstadoJuicio.Apelacion, EstadoJuicio.Ejecucion,
            EstadoJuicio.Concluido, EstadoJuicio.Concluido
        };

        var asuntos = new List<Asunto>();
        var juicios = new List<Juicio>();
        var actuaciones = new List<Actuacion>();
        var promociones = new List<Promocion>();
        var audiencias = new List<Audiencia>();
        var plazos = new List<Plazo>();

        var fechaHoy = DateTime.UtcNow.Date;
        var totalAsuntos = 30;

        for (int i = 0; i < totalAsuntos; i++)
        {
            var cliente = clientes[Rng.Next(clientes.Count)];
            var tipo = TiposDeAsunto[Rng.Next(TiposDeAsunto.Length)];
            var estadoAsunto = estadosAsuntoDist[i % estadosAsuntoDist.Length];

            var mesesAtras = Rng.Next(1, 18);
            var fechaRecepcion = fechaHoy.AddMonths(-mesesAtras).AddDays(-Rng.Next(0, 28));
            var anioFolio = fechaRecepcion.Year;

            int folioNumero;
            if (anioFolio == 2025) { contador2025.UltimoNumero++; folioNumero = contador2025.UltimoNumero; }
            else { contador2026.UltimoNumero++; folioNumero = contador2026.UltimoNumero; }

            var responsable = usuarios.Count > 0 && Rng.Next(3) > 0
                ? usuarios[Rng.Next(usuarios.Count)] : null;

            var asunto = new Asunto
            {
                DespachoId = d.Id,
                Folio = $"JC-{anioFolio}-{folioNumero:D4}",
                Titulo = $"{tipo.Tipo} · {cliente.DisplayName}",
                MateriaKey = tipo.MateriaKey,
                ClienteId = cliente.Id,
                ResponsableId = responsable?.Id,
                Estado = estadoAsunto,
                FechaRecepcion = new DateTimeOffset(fechaRecepcion, TimeSpan.Zero),
                Descripcion = tipo.DescripcionBase,
                Cuantia = Rng.Next(50, 3500) * 1000,
                Prioridad = Rng.Next(1, 6),
                Etiquetas = Rng.Next(4) == 0 ? "urgente" : "",
                EsCobranza = tipo.MateriaKey == Materia.Cobranza || (tipo.MateriaKey == Materia.Mercantil && Rng.Next(3) == 0),
                FechaCierre = estadoAsunto == EstadoAsunto.Cerrado
                    ? new DateTimeOffset(fechaRecepcion.AddDays(Rng.Next(180, 500)), TimeSpan.Zero) : null,
                CreatedBy = "demo",
                CreatedAt = new DateTimeOffset(fechaRecepcion, TimeSpan.Zero)
            };
            asuntos.Add(asunto);
        }
        db.Asuntos.AddRange(asuntos);
        await db.SaveChangesAsync();

        foreach (var asunto in asuntos)
        {
            if (asunto.Estado == EstadoAsunto.Recibido || asunto.Estado == EstadoAsunto.Cancelado) continue;
            if (Rng.Next(10) > 7) continue;

            var estadoJuicio = asunto.Estado == EstadoAsunto.Cerrado
                ? EstadoJuicio.Concluido
                : estadosJuicioDist[Rng.Next(estadosJuicioDist.Length)];

            var juzgados = asunto.MateriaKey switch
            {
                Materia.Mercantil or Materia.Cobranza => JuzgadosMercantil,
                Materia.Civil => JuzgadosCivil,
                Materia.Familiar => JuzgadosFamiliar,
                Materia.Laboral => JuzgadosLaboral,
                Materia.Amparo => JuzgadosAmparo,
                _ => JuzgadosCivil
            };

            var fechaInicio = asunto.FechaRecepcion.AddDays(Rng.Next(5, 30)).Date;

            var juicio = new Juicio
            {
                DespachoId = d.Id,
                AsuntoId = asunto.Id,
                NumeroExpediente = $"{Rng.Next(100, 900)}/{fechaInicio.Year}",
                Juzgado = juzgados[Rng.Next(juzgados.Length)],
                TipoJuicio = "Ordinario",
                MateriaKey = asunto.MateriaKey,
                Estado = estadoJuicio,
                FechaInicio = DateOnly.FromDateTime(fechaInicio),
                FechaConclusion = estadoJuicio == EstadoJuicio.Concluido
                    ? DateOnly.FromDateTime(fechaInicio.AddDays(Rng.Next(200, 400))) : null,
                Cuantia = asunto.Cuantia,
                CreatedBy = "demo",
                CreatedAt = new DateTimeOffset(fechaInicio, TimeSpan.Zero)
            };
            juicios.Add(juicio);
        }
        db.Juicios.AddRange(juicios);
        await db.SaveChangesAsync();

        foreach (var j in juicios)
        {
            var etapas = new[]
            {
                (7, TipoActuacion.Acuerdo, "Auto que admite la demanda."),
                (21, TipoActuacion.Diligencia, "Emplazamiento a la parte demandada."),
                (55, TipoActuacion.Acuerdo, "Contestación tenida por presentada."),
                (90, TipoActuacion.Acuerdo, "Auto que admite pruebas."),
                (150, TipoActuacion.Audiencia, "Audiencia de desahogo celebrada."),
                (210, TipoActuacion.Sentencia, "Sentencia definitiva.")
            };
            foreach (var (dias, tipo, resumen) in etapas)
            {
                var fecha = j.FechaInicio.AddDays(dias);
                if (fecha.ToDateTime(TimeOnly.MinValue) > fechaHoy) break;
                actuaciones.Add(new Actuacion
                {
                    DespachoId = d.Id, JuicioId = j.Id, Tipo = tipo,
                    Fecha = fecha, Resumen = resumen,
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fecha, TimeOnly.MinValue, TimeSpan.Zero)
                });
            }

            // Audiencias
            var numAud = Rng.Next(1, 3);
            for (int a = 0; a < numAud; a++)
            {
                var offset = 60 + a * 60 + Rng.Next(-15, 30);
                var fechaHora = j.FechaInicio.ToDateTime(TimeOnly.MinValue).AddDays(offset).AddHours(9 + Rng.Next(0, 8));
                var estadoAud = fechaHora < fechaHoy ? EstadoAudiencia.Celebrada : EstadoAudiencia.Programada;
                audiencias.Add(new Audiencia
                {
                    DespachoId = d.Id, JuicioId = j.Id,
                    FechaHora = fechaHora,
                    Tipo = new[] { "Audiencia inicial", "Audiencia de pruebas", "Audiencia de conciliación" }[Rng.Next(3)],
                    Lugar = $"Sala {Rng.Next(1, 8)}",
                    Estado = estadoAud,
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(j.FechaInicio.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                });
            }

            // Plazos
            var numPl = Rng.Next(1, 3);
            for (int p = 0; p < numPl; p++)
            {
                var dias = new[] { 3, 5, 8, 9, 10, 15 }[Rng.Next(6)];
                var fechaInicioP = j.FechaInicio.ToDateTime(TimeOnly.MinValue).AddDays(30 + p * 50);
                var fechaVenc = fechaInicioP.AddDays(dias);
                var estadoP = fechaVenc < fechaHoy
                    ? (Rng.Next(3) == 0 ? EstadoPlazo.Vencido : EstadoPlazo.Cumplido)
                    : EstadoPlazo.Abierto;
                plazos.Add(new Plazo
                {
                    DespachoId = d.Id, JuicioId = j.Id,
                    Descripcion = new[] { "Contestar demanda", "Ofrecer pruebas", "Interponer recurso", "Alegatos" }[Rng.Next(4)],
                    FechaInicio = DateOnly.FromDateTime(fechaInicioP),
                    FechaVencimiento = DateOnly.FromDateTime(fechaVenc),
                    DiasOriginales = dias, DiasHabiles = true, Estado = estadoP,
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fechaInicioP, TimeSpan.Zero)
                });
            }
        }
        db.Actuaciones.AddRange(actuaciones);
        db.Promociones.AddRange(promociones);
        db.Audiencias.AddRange(audiencias);
        db.Plazos.AddRange(plazos);
        await db.SaveChangesAsync();

        return (asuntos, juicios, actuaciones, promociones, audiencias, plazos);
    }

    private static async Task SembrarCobranzaAsync(
        JurisControlDbContext db, DbSeeder.DespachoDemo d,
        List<Asunto> asuntos, List<Cliente> clientes)
    {
        var fechaHoy = DateTime.UtcNow.Date;
        var asuntosCob = asuntos.Where(a => a.EsCobranza).Take(5).ToList();
        foreach (var a in asuntosCob)
        {
            var monto = (decimal)Rng.Next(100, 3000) * 1000;
            var saldo = a.Estado == EstadoAsunto.Cerrado ? 0 : monto * (decimal)(0.3 + Rng.NextDouble() * 0.7);
            var cliente = clientes.First(c => c.Id == a.ClienteId);

            var credito = new Credito
            {
                DespachoId = d.Id, AsuntoId = a.Id,
                DeudorClienteId = Rng.Next(2) == 0 ? cliente.Id : null,
                NombreDeudor = cliente.DisplayName,
                NumeroCredito = $"CR-{Rng.Next(100000, 999999)}",
                Acreedor = new[] { "Banco del Norte S.A.", "HSBC México", "BBVA Bancomer" }[Rng.Next(3)],
                Tipo = (TipoCredito)Rng.Next(0, 8),
                Estado = saldo == 0 ? EstadoCredito.Recuperado : EstadoCredito.Judicial,
                MontoOriginal = monto,
                SaldoActual = Math.Round(saldo, 2),
                TasaInteres = (decimal)(0.10 + Rng.NextDouble() * 0.20),
                FechaOrigen = DateOnly.FromDateTime(a.FechaRecepcion.AddMonths(-Rng.Next(6, 24)).Date),
                DiasMora = Rng.Next(90, 900),
                Garantia = "Inmueble ubicado en la CDMX, valor de avalúo aproximado MXN 3,500,000.",
                CreatedBy = "demo", CreatedAt = a.CreatedAt
            };
            db.Creditos.Add(credito);
            await db.SaveChangesAsync();

            // Gestiones y pagos
            for (int i = 0; i < Rng.Next(1, 4); i++)
            {
                var fechaG = fechaHoy.AddDays(-Rng.Next(1, 300));
                db.GestionesCobranza.Add(new GestionCobranza
                {
                    DespachoId = d.Id, CreditoId = credito.Id,
                    Fecha = fechaG,
                    Canal = new[] { "telefono", "visita", "whatsapp", "correo" }[Rng.Next(4)],
                    Resultado = (EstadoGestion)Rng.Next(0, 6),
                    Descripcion = "Se contactó al deudor. Manifestó dificultades económicas.",
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fechaG, TimeSpan.Zero)
                });
            }
            for (int i = 0; i < Rng.Next(0, 3); i++)
            {
                var monto2 = Math.Round((decimal)Rng.Next(5, 30) * 1000, 2);
                var fechaP = fechaHoy.AddDays(-Rng.Next(30, 400));
                db.PagosCobranza.Add(new PagoCobranza
                {
                    DespachoId = d.Id, CreditoId = credito.Id,
                    Fecha = DateOnly.FromDateTime(fechaP),
                    Monto = monto2,
                    AplicadoCapital = monto2 * 0.7m,
                    AplicadoInteres = monto2 * 0.25m,
                    AplicadoGastos = monto2 * 0.05m,
                    MedioPago = "Transferencia SPEI",
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fechaP, TimeSpan.Zero)
                });
            }
            await db.SaveChangesAsync();
        }
    }

    private static async Task SembrarPlantillasAsync(JurisControlDbContext db, DbSeeder.DespachoDemo d)
    {
        var plantillas = new[]
        {
            new Plantilla
            {
                DespachoId = d.Id, Clave = "CARTA-COBRO-1",
                Nombre = "Carta de cobro extrajudicial",
                Categoria = "carta",
                Descripcion = "Requerimiento previo a demanda formal.",
                Cuerpo = @"{{CIUDAD_ACTUAL}}, a {{FECHA_ACTUAL_LETRA}}.

{{NOMBRE_DEL_DEMANDADO}}
{{DIRECCION_CLIENTE}}

Por medio de la presente, y en representación de {{NOMBRE_CLIENTE}}, le requerimos
formalmente el pago de {{CUANTIA_LETRAS}} correspondiente al asunto {{FOLIO_ASUNTO}}
del expediente {{NUMERO_EXPEDIENTE}} radicado en el {{JUZGADO}}.

Le concedemos DIEZ DÍAS HÁBILES para cubrir el adeudo. Vencido dicho plazo sin
respuesta favorable, procederemos con las acciones legales correspondientes.

Atentamente,
{{NOMBRE_ABOGADO}}
{{NOMBRE_DESPACHO}}",
                Activa = true, CreatedBy = "demo", CreatedAt = DateTimeOffset.UtcNow
            },
            new Plantilla
            {
                DespachoId = d.Id, Clave = "INFORME-1",
                Nombre = "Informe mensual al cliente",
                Categoria = "informe",
                Descripcion = "Reporte de estado del juicio para el cliente.",
                Cuerpo = @"INFORME DE ESTADO PROCESAL
Fecha: {{FECHA_ACTUAL}}
Cliente: {{NOMBRE_CLIENTE}}
Asunto: {{TITULO_ASUNTO}}
Folio: {{FOLIO_ASUNTO}}
Materia: {{MATERIA}}
Cuantía: {{CUANTIA}}

EXPEDIENTE JUDICIAL
Número: {{NUMERO_EXPEDIENTE}}
Juzgado: {{JUZGADO}}
Tipo: {{TIPO_JUICIO}}
Inicio: {{FECHA_INICIO_JUICIO}}

PARTES
Actor: {{NOMBRE_DEL_ACTOR}}
Demandado(s): {{NOMBRES_DEMANDADOS}}

Atentamente,
{{NOMBRE_ABOGADO}}",
                Activa = true, CreatedBy = "demo", CreatedAt = DateTimeOffset.UtcNow
            },
            new Plantilla
            {
                DespachoId = d.Id, Clave = "ESCRITO-PROMOCION",
                Nombre = "Encabezado de escrito o promoción",
                Categoria = "escrito",
                Descripcion = "Encabezado para presentar cualquier promoción ante el juzgado.",
                Cuerpo = @"C. JUEZ {{JUZGADO}}
PRESENTE.

{{NOMBRE_DEL_ACTOR}}, por mi propio derecho, promoviendo dentro de los autos del
juicio {{TIPO_JUICIO}} radicado bajo el expediente {{NUMERO_EXPEDIENTE}}, ante
Usía con el debido respeto comparezco y expongo:

[Contenido de la promoción]

Por lo anteriormente expuesto, a Usía atentamente pido:
ÚNICO.- Se sirva proveer conforme a derecho corresponda.

PROTESTO LO NECESARIO.
{{CIUDAD_ACTUAL}}, a {{FECHA_ACTUAL_LETRA}}.
{{NOMBRE_ABOGADO}}",
                Activa = true, CreatedBy = "demo", CreatedAt = DateTimeOffset.UtcNow
            }
        };
        db.Plantillas.AddRange(plantillas);
        await db.SaveChangesAsync();
    }

    private static async Task SembrarGastosAsync(
        JurisControlDbContext db, DbSeeder.DespachoDemo d, List<Juicio> juicios)
    {
        var fechaHoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var conceptos = new (string Cat, string Concepto, decimal MinM, decimal MaxM)[]
        {
            ("copias",     "Copias certificadas del expediente", 150, 800),
            ("viáticos",   "Traslado a diligencia foránea",       350, 1500),
            ("perito",     "Honorarios perito valuador",         5000, 12000),
            ("judiciales", "Depósito para embargo",              1000, 4500),
            ("notariales", "Certificación notarial de documentos", 800, 2500),
            ("honorarios", "Anticipo de honorarios convenio",    5000, 25000)
        };

        foreach (var j in juicios.Take(10))
        {
            var num = Rng.Next(1, 3);
            for (int i = 0; i < num; i++)
            {
                var c = conceptos[Rng.Next(conceptos.Length)];
                var monto = (decimal)Rng.Next((int)c.MinM, (int)c.MaxM);
                var fecha = j.FechaInicio.AddDays(Rng.Next(15, 300));
                if (fecha > fechaHoy) fecha = fechaHoy.AddDays(-Rng.Next(1, 30));
                db.Gastos.Add(new Gasto
                {
                    DespachoId = d.Id, JuicioId = j.Id, AsuntoId = j.AsuntoId,
                    Fecha = fecha, Categoria = c.Cat, Concepto = c.Concepto,
                    Monto = monto, Reembolsable = c.Cat != "honorarios",
                    Estado = Rng.Next(3) == 0 ? "reembolsado" : "pendiente",
                    Comprobante = $"FAC-{Rng.Next(10000, 99999)}",
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fecha.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
