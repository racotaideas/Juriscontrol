using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JurisControl.Data.Seeding;

/// <summary>
/// Siembra datos ficticios pero realistas para el piloto: 40 clientes,
/// 50 asuntos con folios secuenciales, ~40 juicios con etapas procesales
/// que avanzan de enero 2025 a agosto 2026 y actuaciones/promociones/
/// audiencias/plazos con textos creíbles. También pobla el módulo de cobranza.
///
/// Se activa solo si <c>DemoData:Enabled</c> es true en appsettings o env vars.
/// Idempotente: si ya hay más de 20 asuntos, no hace nada.
/// Quitar el flag cuando el piloto termine y el sistema esté con datos reales.
/// </summary>
public static class DemoDataSeeder
{
    private static readonly Guid PilotoDespachoId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Random Rng = new(2026); // seed fijo → mismos datos en re-runs

    // --- Catálogos de datos ficticios ---

    private static readonly string[] JuzgadosCivil =
    {
        "Juzgado 1° de lo Civil, CDMX",
        "Juzgado 8° de lo Civil, CDMX",
        "Juzgado 15° de lo Civil, CDMX",
        "Juzgado 22° de lo Civil, CDMX",
        "Juzgado 32° de lo Civil, CDMX",
        "Juzgado 45° de lo Civil, CDMX",
        "Juzgado 58° de lo Civil, CDMX"
    };
    private static readonly string[] JuzgadosMercantil =
    {
        "Juzgado 3° de lo Mercantil, CDMX",
        "Juzgado 6° de lo Mercantil, CDMX",
        "Juzgado 10° de lo Mercantil, CDMX",
        "Juzgado 18° de lo Mercantil, CDMX",
        "Juzgado 25° de lo Mercantil, CDMX",
        "Juzgado 34° de lo Mercantil, CDMX"
    };
    private static readonly string[] JuzgadosFamiliar =
    {
        "Juzgado 4° de lo Familiar, CDMX",
        "Juzgado 12° de lo Familiar, CDMX",
        "Juzgado 19° de lo Familiar, CDMX",
        "Juzgado 26° de lo Familiar, CDMX"
    };
    private static readonly string[] JuzgadosLaboral =
    {
        "Junta 5 de Conciliación y Arbitraje CDMX",
        "Juzgado 2° del Trabajo, Centro Federal, CDMX",
        "Juzgado 7° del Trabajo, Centro Federal, CDMX"
    };
    private static readonly string[] JuzgadosAmparo =
    {
        "Juzgado 4° de Distrito en Materia Administrativa, CDMX",
        "Juzgado 9° de Distrito en Materia Civil, CDMX",
        "Juzgado 12° de Distrito en Materia Penal, CDMX"
    };

    private static readonly string[] NombresHombres =
    {
        "Roberto", "Carlos", "José", "Luis", "Fernando", "Miguel", "Javier",
        "Ricardo", "Alejandro", "Eduardo", "Rafael", "Jorge", "Andrés", "Sergio",
        "Óscar", "Arturo", "Guillermo", "Manuel", "Alberto", "Enrique"
    };
    private static readonly string[] NombresMujeres =
    {
        "María", "Ana", "Patricia", "Sofía", "Gabriela", "Adriana", "Laura",
        "Mónica", "Claudia", "Verónica", "Alejandra", "Lucía", "Rosa", "Isabel",
        "Carmen", "Beatriz", "Diana", "Elena", "Fernanda", "Lorena"
    };
    private static readonly string[] Apellidos =
    {
        "Hernández", "García", "Martínez", "López", "González", "Rodríguez",
        "Pérez", "Sánchez", "Ramírez", "Torres", "Flores", "Rivera", "Gómez",
        "Díaz", "Reyes", "Cruz", "Morales", "Ortiz", "Gutiérrez", "Chávez",
        "Vázquez", "Jiménez", "Mendoza", "Ruiz", "Aguilar"
    };
    private static readonly string[] Empresas =
    {
        "Constructora del Valle S.A. de C.V.",
        "Distribuidora Norte del Golfo S.A. de C.V.",
        "Servicios Integrales Peninsulares S.C.",
        "Grupo Corporativo Alameda S.A.P.I.",
        "Textiles Industriales de México S.A.",
        "Alimentos Selectos del Bajío S. de R.L.",
        "Inmobiliaria Reforma 2000 S.A. de C.V.",
        "Consultores Jurídicos y Contables Asociados S.C.",
        "Transportes Especializados Roble S.A.",
        "Metalúrgica del Pacífico S.A. de C.V.",
        "Editorial Educativa Ateneo S.A.",
        "Farmacéutica Regional del Centro S.A.",
        "Ingeniería Aplicada Xochimilco S.C.",
        "Bebidas Artesanales Cuauhtémoc S.A.",
        "Refacciones Automotrices Águila S.A. de C.V."
    };

    private static readonly (string Tipo, string MateriaKey, string DescripcionBase)[] TiposDeAsunto =
    {
        ("Ordinario Mercantil",       Materia.Mercantil, "Reclamación por incumplimiento de contrato de prestación de servicios."),
        ("Ejecutivo Mercantil",       Materia.Mercantil, "Ejecución de pagaré vencido con intereses moratorios."),
        ("Ordinario Civil",           Materia.Civil,     "Cumplimiento forzoso de contrato de arrendamiento."),
        ("Especial de Desahucio",     Materia.Civil,     "Desocupación de inmueble por falta de pago de rentas."),
        ("Rescisión de Contrato",     Materia.Civil,     "Rescisión de contrato de compraventa por vicios ocultos."),
        ("Amparo Indirecto",          Materia.Amparo,    "Amparo contra acto de autoridad administrativa."),
        ("Ordinario Laboral",         Materia.Laboral,   "Reclamo de prestaciones laborales por despido injustificado."),
        ("Divorcio Necesario",        Materia.Familiar,  "Divorcio con controversia sobre bienes y guarda de menores."),
        ("Sucesión Intestamentaria",  Materia.Familiar,  "Trámite sucesorio sin testamento previo del de cujus."),
        ("Cobro de Crédito Bancario", Materia.Cobranza,  "Recuperación de crédito bancario con garantía hipotecaria.")
    };

    // Etapas típicas por estado (nombre-de-actuación, offset-en-días desde inicio)
    private static readonly Dictionary<EstadoJuicio, (string Resumen, int Dias, TipoActuacion Tipo)[]> EtapasPorEstado = new()
    {
        [EstadoJuicio.Iniciado] = new[]
        {
            ("Auto que admite la demanda y ordena emplazar al demandado.", 7, TipoActuacion.Acuerdo),
            ("Diligencia de emplazamiento por conducto de actuario.", 21, TipoActuacion.Diligencia),
        },
        [EstadoJuicio.EnPruebas] = new[]
        {
            ("Auto que admite la demanda.", 7, TipoActuacion.Acuerdo),
            ("Emplazamiento a la parte demandada.", 21, TipoActuacion.Diligencia),
            ("Auto que tiene por contestada la demanda.", 55, TipoActuacion.Acuerdo),
            ("Auto que admite pruebas ofrecidas por ambas partes.", 90, TipoActuacion.Acuerdo),
            ("Desahogo de prueba testimonial.", 115, TipoActuacion.Diligencia),
        },
        [EstadoJuicio.Alegatos] = new[]
        {
            ("Auto admisorio de demanda.", 7, TipoActuacion.Acuerdo),
            ("Emplazamiento.", 21, TipoActuacion.Diligencia),
            ("Contestación tenida por presentada.", 55, TipoActuacion.Acuerdo),
            ("Auto que admite pruebas.", 90, TipoActuacion.Acuerdo),
            ("Audiencia de desahogo celebrada.", 130, TipoActuacion.Audiencia),
            ("Se cita a las partes a formular alegatos.", 165, TipoActuacion.Acuerdo),
        },
        [EstadoJuicio.Sentencia] = new[]
        {
            ("Auto admisorio.", 7, TipoActuacion.Acuerdo),
            ("Emplazamiento.", 21, TipoActuacion.Diligencia),
            ("Contestación.", 55, TipoActuacion.Acuerdo),
            ("Audiencia de pruebas y alegatos.", 130, TipoActuacion.Audiencia),
            ("Sentencia definitiva de primera instancia. Se condena a la parte demandada.", 210, TipoActuacion.Sentencia),
            ("Notificación de sentencia.", 215, TipoActuacion.Notificacion),
        },
        [EstadoJuicio.Apelacion] = new[]
        {
            ("Auto admisorio.", 7, TipoActuacion.Acuerdo),
            ("Emplazamiento.", 21, TipoActuacion.Diligencia),
            ("Contestación.", 55, TipoActuacion.Acuerdo),
            ("Sentencia definitiva favorable.", 210, TipoActuacion.Sentencia),
            ("Recurso de apelación admitido y remitido a la Sala.", 240, TipoActuacion.Resolucion),
            ("Se cita para audiencia de vista.", 275, TipoActuacion.Acuerdo),
        },
        [EstadoJuicio.Ejecucion] = new[]
        {
            ("Auto admisorio.", 7, TipoActuacion.Acuerdo),
            ("Emplazamiento.", 21, TipoActuacion.Diligencia),
            ("Sentencia condenatoria.", 210, TipoActuacion.Sentencia),
            ("Sentencia ejecutoriada.", 250, TipoActuacion.Resolucion),
            ("Requerimiento de pago y embargo trabado.", 280, TipoActuacion.Diligencia),
            ("Se ordena la práctica de avalúo.", 310, TipoActuacion.Acuerdo),
        },
        [EstadoJuicio.Concluido] = new[]
        {
            ("Auto admisorio.", 7, TipoActuacion.Acuerdo),
            ("Emplazamiento.", 21, TipoActuacion.Diligencia),
            ("Sentencia condenatoria.", 210, TipoActuacion.Sentencia),
            ("Sentencia ejecutoriada.", 250, TipoActuacion.Resolucion),
            ("Pago total del adeudo. Se ordena el archivo del expediente.", 330, TipoActuacion.Acuerdo),
        },
        [EstadoJuicio.Amparo] = new[]
        {
            ("Auto admisorio.", 7, TipoActuacion.Acuerdo),
            ("Sentencia de primera instancia adversa.", 210, TipoActuacion.Sentencia),
            ("Se promueve juicio de amparo directo.", 220, TipoActuacion.Escrito),
            ("Admisión del amparo. Autoridad responsable rinde informe.", 250, TipoActuacion.Resolucion),
        },
        [EstadoJuicio.Sobreseido] = new[]
        {
            ("Auto admisorio.", 7, TipoActuacion.Acuerdo),
            ("Sobreseimiento por desistimiento del actor.", 60, TipoActuacion.Resolucion),
        }
    };

    private static readonly (TipoPromocion Tipo, string Titulo, int OffsetDias)[] PromocionesTipicas =
    {
        (TipoPromocion.Demanda,         "Escrito inicial de demanda con anexos.", 0),
        (TipoPromocion.Contestacion,    "Contestación a la demanda oponiendo excepciones.", 50),
        (TipoPromocion.Ofrecimiento,    "Ofrecimiento de pruebas documentales y testimoniales.", 80),
        (TipoPromocion.DesahogoPrueba,  "Escrito para desahogo de prueba pericial.", 110),
        (TipoPromocion.Alegatos,        "Alegatos de bien probado.", 170),
        (TipoPromocion.Recurso,         "Recurso de apelación contra sentencia definitiva.", 225)
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

        var yaHay = await db.Asuntos.CountAsync();
        if (yaHay >= 20)
        {
            logger.LogInformation("DemoDataSeeder skipped: ya hay {Count} asuntos.", yaHay);
            return;
        }

        logger.LogInformation("DemoDataSeeder iniciando siembra ficticia…");

        // --- 1. Clientes ---
        var clientes = new List<Cliente>();
        for (int i = 0; i < 25; i++)
        {
            var esHombre = Rng.Next(2) == 0;
            var nombre = esHombre ? NombresHombres[Rng.Next(NombresHombres.Length)]
                                   : NombresMujeres[Rng.Next(NombresMujeres.Length)];
            var apP = Apellidos[Rng.Next(Apellidos.Length)];
            var apM = Apellidos[Rng.Next(Apellidos.Length)];
            clientes.Add(new Cliente
            {
                DespachoId = PilotoDespachoId,
                Tipo = TipoCliente.PersonaFisica,
                Nombre = nombre, ApellidoPaterno = apP, ApellidoMaterno = apM,
                Rfc = $"{apP.Substring(0, 2).ToUpper()}{apM[0]}{nombre[0]}{Rng.Next(600000, 999999)}",
                CorreoPrincipal = $"{nombre.ToLower()}.{apP.ToLower()}@correo.mx",
                TelefonoPrincipal = $"55{Rng.Next(10000000, 99999999)}",
                Ciudad = "Ciudad de México", Estado = "CDMX",
                CodigoPostal = $"0{Rng.Next(1000, 9999)}",
                Direccion = $"Calle {Apellidos[Rng.Next(Apellidos.Length)]} #{Rng.Next(1, 500)}, Col. {new[] { "Roma", "Condesa", "Del Valle", "Polanco", "Doctores", "Narvarte" }[Rng.Next(6)]}",
                Etiquetas = Rng.Next(4) == 0 ? "VIP,recurrente" : "activo",
                Activo = true, CreatedBy = "demo",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-Rng.Next(1, 18))
            });
        }
        for (int i = 0; i < 15; i++)
        {
            var razon = Empresas[i % Empresas.Length];
            clientes.Add(new Cliente
            {
                DespachoId = PilotoDespachoId,
                Tipo = TipoCliente.PersonaMoral,
                RazonSocial = razon,
                NombreComercial = razon.Split(' ')[0],
                RepresentanteLegal = $"{NombresHombres[Rng.Next(NombresHombres.Length)]} {Apellidos[Rng.Next(Apellidos.Length)]}",
                Rfc = $"{razon.Substring(0, 3).ToUpper()}{Rng.Next(600000, 999999)}",
                CorreoPrincipal = $"contacto@{razon.Split(' ')[0].ToLower()}.mx",
                TelefonoPrincipal = $"55{Rng.Next(10000000, 99999999)}",
                Ciudad = "Ciudad de México", Estado = "CDMX",
                CodigoPostal = $"0{Rng.Next(1000, 9999)}",
                Etiquetas = Rng.Next(3) == 0 ? "corporativo,retenedor" : "corporativo",
                Activo = true, CreatedBy = "demo",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-Rng.Next(1, 18))
            });
        }
        db.Clientes.AddRange(clientes);
        await db.SaveChangesAsync();

        // Refresco de IDs
        var clientesGuardados = await db.Clientes.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt).Take(clientes.Count).ToListAsync();

        // --- 2. Contador de folios (para no colisionar con futuros manuales) ---
        var contador2025 = new ContadorFolio
        {
            DespachoId = PilotoDespachoId, Anio = 2025, UltimoNumero = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var contador2026 = new ContadorFolio
        {
            DespachoId = PilotoDespachoId, Anio = 2026, UltimoNumero = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var existente2025 = await db.ContadoresFolio.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.DespachoId == PilotoDespachoId && c.Anio == 2025);
        var existente2026 = await db.ContadoresFolio.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.DespachoId == PilotoDespachoId && c.Anio == 2026);
        if (existente2025 is not null) contador2025 = existente2025;
        else db.ContadoresFolio.Add(contador2025);
        if (existente2026 is not null) contador2026 = existente2026;
        else db.ContadoresFolio.Add(contador2026);
        await db.SaveChangesAsync();

        // --- 3. Asuntos ---
        var estadosDistribucion = new[]
        {
            EstadoAsunto.Activo, EstadoAsunto.Activo, EstadoAsunto.Activo, EstadoAsunto.Activo,
            EstadoAsunto.Asignado, EstadoAsunto.Asignado,
            EstadoAsunto.Recibido,
            EstadoAsunto.EnEspera,
            EstadoAsunto.Cerrado, EstadoAsunto.Cerrado,
        };
        var estadosJuicioDistribucion = new[]
        {
            EstadoJuicio.Iniciado, EstadoJuicio.EnPruebas, EstadoJuicio.EnPruebas,
            EstadoJuicio.Alegatos, EstadoJuicio.Alegatos,
            EstadoJuicio.Sentencia, EstadoJuicio.Sentencia,
            EstadoJuicio.Apelacion,
            EstadoJuicio.Ejecucion,
            EstadoJuicio.Concluido, EstadoJuicio.Concluido,
            EstadoJuicio.Amparo
        };

        var asuntos = new List<Asunto>();
        var juicios = new List<Juicio>();
        var partes = new List<ParteJuicio>();
        var actuaciones = new List<Actuacion>();
        var promociones = new List<Promocion>();
        var audiencias = new List<Audiencia>();
        var plazos = new List<Plazo>();

        var fechaHoy = DateTime.UtcNow.Date;
        var fechaHoyOnly = DateOnly.FromDateTime(fechaHoy);

        for (int i = 0; i < 50; i++)
        {
            var cliente = clientesGuardados[Rng.Next(clientesGuardados.Count)];
            var tipo = TiposDeAsunto[Rng.Next(TiposDeAsunto.Length)];
            var estadoAsunto = estadosDistribucion[i % estadosDistribucion.Length];

            // Fecha de recepción: mezcla enero 2025 – julio 2026
            var mesesAtras = Rng.Next(1, 19);
            var fechaRecepcion = fechaHoy.AddMonths(-mesesAtras).AddDays(-Rng.Next(0, 28));
            var anioFolio = fechaRecepcion.Year;
            int folioNumero;
            if (anioFolio == 2025) { contador2025.UltimoNumero++; folioNumero = contador2025.UltimoNumero; }
            else { contador2026.UltimoNumero++; folioNumero = contador2026.UltimoNumero; }

            var esCobranza = tipo.MateriaKey == Materia.Cobranza
                             || (tipo.MateriaKey == Materia.Mercantil && Rng.Next(3) == 0);

            var asunto = new Asunto
            {
                DespachoId = PilotoDespachoId,
                Folio = $"JC-{anioFolio}-{folioNumero:D4}",
                Titulo = $"{tipo.Tipo} · {cliente.DisplayName}",
                MateriaKey = tipo.MateriaKey,
                ClienteId = cliente.Id,
                Estado = estadoAsunto,
                FechaRecepcion = new DateTimeOffset(fechaRecepcion, TimeSpan.Zero),
                Descripcion = tipo.DescripcionBase + $" Cuantía aproximada MXN {Rng.Next(50, 3500) * 1000}.",
                Cuantia = Rng.Next(50, 3500) * 1000,
                Prioridad = Rng.Next(1, 6),
                Etiquetas = Rng.Next(4) == 0 ? "urgente" : (Rng.Next(3) == 0 ? "retenedor" : ""),
                EsCobranza = esCobranza,
                FechaCierre = estadoAsunto == EstadoAsunto.Cerrado
                    ? new DateTimeOffset(fechaRecepcion.AddDays(Rng.Next(180, 500)), TimeSpan.Zero)
                    : null,
                CreatedBy = "demo",
                CreatedAt = new DateTimeOffset(fechaRecepcion, TimeSpan.Zero)
            };
            asuntos.Add(asunto);
        }

        db.Asuntos.AddRange(asuntos);
        await db.SaveChangesAsync();

        // --- 4. Juicios + toda la carnita procesal ---
        var asuntosGuardados = asuntos;
        int folio2025Cursor = 100;
        int folio2026Cursor = 100;

        foreach (var asunto in asuntosGuardados)
        {
            // 80% de los asuntos activos/asignados/alegatos/etc. tienen un juicio
            var creaJuicio = asunto.Estado != EstadoAsunto.Recibido
                             && asunto.Estado != EstadoAsunto.Cancelado
                             && Rng.Next(10) < 8;
            if (!creaJuicio) continue;

            var estadoJuicio = asunto.Estado == EstadoAsunto.Cerrado
                ? EstadoJuicio.Concluido
                : estadosJuicioDistribucion[Rng.Next(estadosJuicioDistribucion.Length)];

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
            var anioExp = fechaInicio.Year;
            var numExp = anioExp == 2025 ? ++folio2025Cursor : ++folio2026Cursor;

            var tipoJuicio = asunto.MateriaKey switch
            {
                Materia.Mercantil => Rng.Next(2) == 0 ? "Ordinario Mercantil" : "Ejecutivo Mercantil",
                Materia.Cobranza => "Ejecutivo Mercantil con garantía",
                Materia.Civil => Rng.Next(3) switch { 0 => "Ordinario Civil", 1 => "Especial de Desahucio", _ => "Rescisión de Contrato" },
                Materia.Familiar => Rng.Next(2) == 0 ? "Divorcio Necesario" : "Sucesión Intestamentaria",
                Materia.Laboral => "Ordinario Laboral",
                Materia.Amparo => "Amparo Indirecto",
                _ => "Ordinario Civil"
            };

            var juicio = new Juicio
            {
                DespachoId = PilotoDespachoId,
                AsuntoId = asunto.Id,
                NumeroExpediente = $"{numExp}/{anioExp}",
                Juzgado = juzgados[Rng.Next(juzgados.Length)],
                TipoJuicio = tipoJuicio,
                MateriaKey = asunto.MateriaKey,
                Estado = estadoJuicio,
                FechaInicio = DateOnly.FromDateTime(fechaInicio),
                FechaConclusion = estadoJuicio == EstadoJuicio.Concluido
                    ? DateOnly.FromDateTime(fechaInicio.AddDays(Rng.Next(300, 500)))
                    : null,
                Cuantia = asunto.Cuantia,
                Descripcion = asunto.Descripcion,
                Observaciones = Rng.Next(3) == 0 ? "Parte contraria ha manifestado interés en llegar a un convenio." : null,
                CreatedBy = "demo",
                CreatedAt = new DateTimeOffset(fechaInicio, TimeSpan.Zero)
            };
            juicios.Add(juicio);

            // Partes: actor (nuestro cliente) + demandado (parte externa)
            partes.Add(new ParteJuicio
            {
                DespachoId = PilotoDespachoId,
                JuicioId = juicio.Id,
                Rol = RolProcesal.Actor,
                ClienteId = asunto.ClienteId,
                CreatedBy = "demo", CreatedAt = juicio.CreatedAt
            });
            partes.Add(new ParteJuicio
            {
                DespachoId = PilotoDespachoId,
                JuicioId = juicio.Id,
                Rol = RolProcesal.Demandado,
                NombreLibre = $"{NombresHombres[Rng.Next(NombresHombres.Length)]} {Apellidos[Rng.Next(Apellidos.Length)]} {Apellidos[Rng.Next(Apellidos.Length)]}",
                Representante = Rng.Next(2) == 0 ? $"Lic. {Apellidos[Rng.Next(Apellidos.Length)]}" : null,
                CreatedBy = "demo", CreatedAt = juicio.CreatedAt
            });

            // Actuaciones según etapa
            var etapas = EtapasPorEstado[estadoJuicio];
            foreach (var etapa in etapas)
            {
                var fecha = fechaInicio.AddDays(etapa.Dias);
                if (fecha > fechaHoy) continue; // no dar actuaciones del futuro
                actuaciones.Add(new Actuacion
                {
                    DespachoId = PilotoDespachoId,
                    JuicioId = juicio.Id,
                    Tipo = etapa.Tipo,
                    Fecha = DateOnly.FromDateTime(fecha),
                    FechaNotificacion = Rng.Next(3) == 0 ? DateOnly.FromDateTime(fecha.AddDays(Rng.Next(1, 4))) : null,
                    Resumen = etapa.Resumen,
                    Detalle = Rng.Next(3) == 0 ? "Se agregaron copias de traslado al expediente." : null,
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fecha, TimeSpan.Zero)
                });
            }

            // Promociones del despacho
            var numPromociones = Math.Min(PromocionesTipicas.Length, etapas.Length);
            for (int p = 0; p < numPromociones; p++)
            {
                var prom = PromocionesTipicas[p];
                var fecha = fechaInicio.AddDays(prom.OffsetDias);
                if (fecha > fechaHoy) continue;
                promociones.Add(new Promocion
                {
                    DespachoId = PilotoDespachoId,
                    JuicioId = juicio.Id,
                    Tipo = prom.Tipo,
                    FechaPresentacion = DateOnly.FromDateTime(fecha),
                    Titulo = prom.Titulo,
                    Contenido = "Se presenta ante el juzgado con original y copia para acuse.",
                    NumeroAcuse = $"AC-{Rng.Next(100000, 999999)}",
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fecha, TimeSpan.Zero)
                });
            }

            // Audiencias — 1 a 3, alguna futura
            var numAudiencias = Rng.Next(1, 4);
            for (int a = 0; a < numAudiencias; a++)
            {
                var offset = 90 + a * 60 + Rng.Next(-15, 30);
                var fechaHora = fechaInicio.AddDays(offset).AddHours(9 + Rng.Next(0, 8));
                var estadoAud = fechaHora < fechaHoy
                    ? (Rng.Next(4) == 0 ? EstadoAudiencia.Diferida : EstadoAudiencia.Celebrada)
                    : EstadoAudiencia.Programada;
                audiencias.Add(new Audiencia
                {
                    DespachoId = PilotoDespachoId,
                    JuicioId = juicio.Id,
                    FechaHora = fechaHora,
                    Tipo = new[] { "Audiencia inicial", "Audiencia de pruebas", "Audiencia de conciliación", "Audiencia de alegatos" }[Rng.Next(4)],
                    Lugar = $"Sala {Rng.Next(1, 8)}",
                    Estado = estadoAud,
                    Resultado = estadoAud == EstadoAudiencia.Celebrada ? "Se desahogaron las pruebas ofrecidas." : null,
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fechaInicio, TimeSpan.Zero)
                });
            }

            // Plazos: 1-3, mezcla de cumplidos/vencidos/abiertos
            var numPlazos = Rng.Next(1, 4);
            for (int pl = 0; pl < numPlazos; pl++)
            {
                var dias = new[] { 3, 5, 8, 9, 10, 15 }[Rng.Next(6)];
                var fechaInicioPlazo = fechaInicio.AddDays(30 + pl * 50 + Rng.Next(0, 30));
                var fechaVenc = fechaInicioPlazo.AddDays(dias);
                var estadoPl = fechaVenc < fechaHoy
                    ? (Rng.Next(3) == 0 ? EstadoPlazo.Vencido : EstadoPlazo.Cumplido)
                    : EstadoPlazo.Abierto;
                plazos.Add(new Plazo
                {
                    DespachoId = PilotoDespachoId,
                    JuicioId = juicio.Id,
                    Descripcion = new[] { "Contestar demanda", "Ofrecer pruebas", "Desahogar prevención", "Interponer recurso", "Alegatos", "Presentar prueba pericial" }[Rng.Next(6)],
                    FechaInicio = DateOnly.FromDateTime(fechaInicioPlazo),
                    FechaVencimiento = DateOnly.FromDateTime(fechaVenc),
                    DiasOriginales = dias,
                    DiasHabiles = true,
                    Estado = estadoPl,
                    FechaCumplimiento = estadoPl == EstadoPlazo.Cumplido
                        ? new DateTimeOffset(fechaVenc.AddDays(-Rng.Next(0, dias)), TimeSpan.Zero) : null,
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fechaInicioPlazo, TimeSpan.Zero)
                });
            }
        }

        db.Juicios.AddRange(juicios);
        await db.SaveChangesAsync();
        db.PartesJuicio.AddRange(partes);
        db.Actuaciones.AddRange(actuaciones);
        db.Promociones.AddRange(promociones);
        db.Audiencias.AddRange(audiencias);
        db.Plazos.AddRange(plazos);
        await db.SaveChangesAsync();

        // --- 5. Módulo cobranza para asuntos EsCobranza ---
        var asuntosCobranza = asuntosGuardados.Where(a => a.EsCobranza).Take(12).ToList();
        var creditos = new List<Credito>();
        foreach (var a in asuntosCobranza)
        {
            var monto = (decimal)Rng.Next(100, 4000) * 1000;
            var saldo = a.Estado == EstadoAsunto.Cerrado ? 0 : monto * (decimal)(0.3 + Rng.NextDouble() * 0.7);
            var cliente = clientesGuardados.First(c => c.Id == a.ClienteId);
            creditos.Add(new Credito
            {
                DespachoId = PilotoDespachoId,
                AsuntoId = a.Id,
                DeudorClienteId = Rng.Next(2) == 0 ? cliente.Id : null,
                NombreDeudor = Rng.Next(2) == 0 ? cliente.DisplayName : $"{NombresHombres[Rng.Next(NombresHombres.Length)]} {Apellidos[Rng.Next(Apellidos.Length)]}",
                NumeroCredito = $"CR-{Rng.Next(100000, 999999)}",
                Acreedor = new[] { "Banco del Norte S.A.", "HSBC México", "BBVA Bancomer", "Santander Serfin", "Institución Financiera Aurora" }[Rng.Next(5)],
                Tipo = (TipoCredito)Rng.Next(0, 8),
                Estado = saldo == 0 ? EstadoCredito.Recuperado
                          : (a.Estado == EstadoAsunto.Activo ? EstadoCredito.Judicial : EstadoCredito.Cartera),
                MontoOriginal = monto,
                SaldoActual = Math.Round(saldo, 2),
                TasaInteres = (decimal)(0.10 + Rng.NextDouble() * 0.20),
                FechaOrigen = DateOnly.FromDateTime(a.FechaRecepcion.AddMonths(-Rng.Next(6, 24)).Date),
                FechaVencimiento = DateOnly.FromDateTime(a.FechaRecepcion.AddDays(-30).Date),
                DiasMora = Rng.Next(90, 900),
                Garantia = Rng.Next(2) == 0 ? "Inmueble ubicado en Col. Roma Norte, CDMX, con valor de avalúo MXN 3,200,000." : "Aval solidario del Sr. " + Apellidos[Rng.Next(Apellidos.Length)],
                CreatedBy = "demo",
                CreatedAt = a.CreatedAt
            });
        }
        db.Creditos.AddRange(creditos);
        await db.SaveChangesAsync();

        // Pagos y gestiones para créditos activos
        var pagos = new List<PagoCobranza>();
        var gestiones = new List<GestionCobranza>();
        foreach (var c in creditos)
        {
            // 1-4 gestiones por crédito
            var numGest = Rng.Next(1, 5);
            for (int g = 0; g < numGest; g++)
            {
                var fechaG = fechaHoy.AddDays(-Rng.Next(1, 300));
                gestiones.Add(new GestionCobranza
                {
                    DespachoId = PilotoDespachoId,
                    CreditoId = c.Id,
                    Fecha = fechaG,
                    Canal = new[] { "telefono", "visita", "whatsapp", "correo" }[Rng.Next(4)],
                    Resultado = (EstadoGestion)Rng.Next(0, 6),
                    PersonaContactada = $"{NombresMujeres[Rng.Next(NombresMujeres.Length)]} {Apellidos[Rng.Next(Apellidos.Length)]}",
                    Descripcion = new[]
                    {
                        "Se contactó al deudor. Manifestó dificultades económicas por pérdida de empleo.",
                        "Deudor promete cubrir el adeudo a más tardar el próximo mes.",
                        "No fue posible localizar al deudor en el domicilio registrado.",
                        "Se envió correo de recordatorio con propuesta de reestructura.",
                        "Deudor rechaza cualquier convenio; solicita se continúe el trámite judicial."
                    }[Rng.Next(5)],
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fechaG, TimeSpan.Zero)
                });
            }

            // 0-3 pagos parciales
            var numPagos = Rng.Next(0, 4);
            for (int p = 0; p < numPagos; p++)
            {
                var monto = Math.Round((decimal)Rng.Next(5, 40) * 1000, 2);
                var fechaP = fechaHoy.AddDays(-Rng.Next(30, 500));
                pagos.Add(new PagoCobranza
                {
                    DespachoId = PilotoDespachoId,
                    CreditoId = c.Id,
                    Fecha = DateOnly.FromDateTime(fechaP),
                    Monto = monto,
                    AplicadoCapital = monto * 0.7m,
                    AplicadoInteres = monto * 0.25m,
                    AplicadoGastos = monto * 0.05m,
                    MedioPago = new[] { "Transferencia SPEI", "Depósito ventanilla", "Cheque nominativo" }[Rng.Next(3)],
                    Referencia = $"REF-{Rng.Next(100000, 999999)}",
                    CreatedBy = "demo",
                    CreatedAt = new DateTimeOffset(fechaP, TimeSpan.Zero)
                });
            }
        }
        db.GestionesCobranza.AddRange(gestiones);
        db.PagosCobranza.AddRange(pagos);
        await db.SaveChangesAsync();

        logger.LogInformation("DemoDataSeeder terminó: {Clientes} clientes, {Asuntos} asuntos, {Juicios} juicios, {Act} actuaciones, {Prom} promociones, {Aud} audiencias, {Plazos} plazos, {Cred} créditos, {Gest} gestiones, {Pag} pagos.",
            clientes.Count, asuntos.Count, juicios.Count, actuaciones.Count,
            promociones.Count, audiencias.Count, plazos.Count, creditos.Count, gestiones.Count, pagos.Count);
    }
}
