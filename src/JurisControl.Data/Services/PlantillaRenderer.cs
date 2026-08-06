using System.Globalization;
using System.Text.RegularExpressions;
using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Data.Services;

/// <summary>
/// Motor de resolución de tokens para plantillas ("machotes" del manual clásico).
/// Toma una plantilla con tokens {{TOKEN}} y los reemplaza contra los datos
/// del asunto/juicio/cliente. Los tokens siguen la convención MAYÚSCULAS_CON_UNDERSCORE.
/// </summary>
public interface IPlantillaRenderer
{
    Task<string> RenderAsync(Plantilla plantilla, Guid asuntoId, Guid? juicioId, CancellationToken ct = default);
    IReadOnlyList<TokenInfo> TokensDisponibles { get; }
}

public sealed record TokenInfo(string Token, string Descripcion);

public sealed class PlantillaRenderer : IPlantillaRenderer
{
    private readonly JurisControlDbContext _db;
    public PlantillaRenderer(JurisControlDbContext db) => _db = db;

    // Tokens documentados. La UI los ofrece como picker al editar la plantilla.
    public IReadOnlyList<TokenInfo> TokensDisponibles { get; } = new List<TokenInfo>
    {
        new("FECHA_ACTUAL", "Fecha del día en formato yyyy-MM-dd"),
        new("FECHA_ACTUAL_LETRA", "Fecha del día en letra (ej. cinco de agosto de dos mil veintiséis)"),
        new("CIUDAD_ACTUAL", "Ciudad de México (constante)"),

        new("NOMBRE_DESPACHO", "Razón social del despacho"),
        new("NOMBRE_ABOGADO", "Nombre completo del abogado responsable del asunto"),

        new("FOLIO_ASUNTO", "Folio interno JC-YYYY-NNNN"),
        new("TITULO_ASUNTO", "Título del asunto"),
        new("MATERIA", "Materia del asunto"),
        new("CUANTIA", "Cuantía numérica del asunto"),
        new("CUANTIA_LETRAS", "Cuantía escrita con letra"),

        new("NOMBRE_CLIENTE", "Nombre completo o razón social del cliente"),
        new("RFC_CLIENTE", "RFC del cliente"),
        new("DIRECCION_CLIENTE", "Dirección del cliente"),
        new("CORREO_CLIENTE", "Correo principal del cliente"),
        new("TELEFONO_CLIENTE", "Teléfono principal del cliente"),

        new("NUMERO_EXPEDIENTE", "Número de expediente asignado por el juzgado"),
        new("JUZGADO", "Nombre del juzgado o tribunal"),
        new("TIPO_JUICIO", "Tipo de juicio"),
        new("FECHA_INICIO_JUICIO", "Fecha de inicio del juicio"),

        new("NOMBRE_DEL_DEMANDADO", "Nombre de la parte demandada (o primer demandado si son varios)"),
        new("NOMBRES_DEMANDADOS", "Todos los demandados separados por coma"),
        new("NOMBRE_DEL_ACTOR", "Nombre de la parte actora"),
    };

    public async Task<string> RenderAsync(Plantilla plantilla, Guid asuntoId, Guid? juicioId, CancellationToken ct = default)
    {
        var asunto = await _db.Asuntos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == asuntoId, ct)
            ?? throw new InvalidOperationException("Asunto no encontrado.");
        var despacho = await _db.Despachos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == asunto.DespachoId, ct);
        var cliente = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == asunto.ClienteId, ct);

        Juicio? juicio = null;
        List<ParteJuicio> partes = new();
        if (juicioId.HasValue)
        {
            juicio = await _db.Juicios.AsNoTracking().FirstOrDefaultAsync(j => j.Id == juicioId.Value, ct);
            if (juicio is not null)
                partes = await _db.PartesJuicio.AsNoTracking().Where(p => p.JuicioId == juicio.Id).ToListAsync(ct);
        }

        Domain.Entities.ApplicationUser? responsable = null;
        if (asunto.ResponsableId.HasValue)
            responsable = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == asunto.ResponsableId.Value, ct);

        var demandados = await ResolverPartesAsync(partes.Where(p => p.Rol == Domain.Enums.RolProcesal.Demandado));
        var actores = await ResolverPartesAsync(partes.Where(p => p.Rol == Domain.Enums.RolProcesal.Actor));

        var hoy = DateTime.Now;
        var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FECHA_ACTUAL"] = hoy.ToString("yyyy-MM-dd"),
            ["FECHA_ACTUAL_LETRA"] = FechaEnLetra(DateOnly.FromDateTime(hoy)),
            ["CIUDAD_ACTUAL"] = "Ciudad de México",

            ["NOMBRE_DESPACHO"] = despacho?.RazonSocial ?? "",
            ["NOMBRE_ABOGADO"] = responsable?.NombreCompleto ?? "",

            ["FOLIO_ASUNTO"] = asunto.Folio,
            ["TITULO_ASUNTO"] = asunto.Titulo,
            ["MATERIA"] = Domain.Enums.Materia.Label(asunto.MateriaKey),
            ["CUANTIA"] = asunto.Cuantia?.ToString("C2", new CultureInfo("es-MX")) ?? "N/A",
            ["CUANTIA_LETRAS"] = asunto.Cuantia.HasValue ? NumeroEnLetra(asunto.Cuantia.Value) : "N/A",

            ["NOMBRE_CLIENTE"] = cliente?.DisplayName ?? "",
            ["RFC_CLIENTE"] = cliente?.Rfc ?? "",
            ["DIRECCION_CLIENTE"] = ArmarDireccion(cliente),
            ["CORREO_CLIENTE"] = cliente?.CorreoPrincipal ?? "",
            ["TELEFONO_CLIENTE"] = cliente?.TelefonoPrincipal ?? "",

            ["NUMERO_EXPEDIENTE"] = juicio?.NumeroExpediente ?? "N/A",
            ["JUZGADO"] = juicio?.Juzgado ?? "N/A",
            ["TIPO_JUICIO"] = juicio?.TipoJuicio ?? "N/A",
            ["FECHA_INICIO_JUICIO"] = juicio?.FechaInicio.ToString("yyyy-MM-dd") ?? "N/A",

            ["NOMBRE_DEL_DEMANDADO"] = demandados.FirstOrDefault() ?? "N/A",
            ["NOMBRES_DEMANDADOS"] = string.Join(", ", demandados),
            ["NOMBRE_DEL_ACTOR"] = actores.FirstOrDefault() ?? cliente?.DisplayName ?? "N/A",
        };

        return Regex.Replace(plantilla.Cuerpo, @"\{\{([A-Z_]+)\}\}", m =>
        {
            var key = m.Groups[1].Value;
            return valores.TryGetValue(key, out var v) ? v : m.Value;
        });
    }

    private async Task<List<string>> ResolverPartesAsync(IEnumerable<ParteJuicio> partes)
    {
        var lista = new List<string>();
        foreach (var p in partes)
        {
            if (p.ClienteId.HasValue)
            {
                var c = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == p.ClienteId.Value);
                if (c is not null) lista.Add(c.DisplayName);
            }
            else if (!string.IsNullOrWhiteSpace(p.NombreLibre))
            {
                lista.Add(p.NombreLibre);
            }
        }
        return lista;
    }

    private static string ArmarDireccion(Domain.Entities.Cliente? c)
    {
        if (c is null) return "";
        var partes = new[] { c.Direccion, c.Ciudad, c.Estado, c.CodigoPostal }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        return string.Join(", ", partes);
    }

    // Fecha en letra al estilo mexicano: "cinco de agosto de dos mil veintiséis"
    private static string FechaEnLetra(DateOnly f)
    {
        var meses = new[] { "", "enero","febrero","marzo","abril","mayo","junio",
                            "julio","agosto","septiembre","octubre","noviembre","diciembre" };
        return $"{NumeroEnLetraSimple(f.Day)} de {meses[f.Month]} de {NumeroEnLetraSimple(f.Year)}";
    }

    private static readonly string[] Unidades =
        { "", "uno","dos","tres","cuatro","cinco","seis","siete","ocho","nueve","diez",
          "once","doce","trece","catorce","quince","dieciséis","diecisiete","dieciocho","diecinueve","veinte" };

    private static readonly string[] Decenas =
        { "", "diez","veinte","treinta","cuarenta","cincuenta","sesenta","setenta","ochenta","noventa" };

    private static readonly string[] Centenas =
        { "", "ciento","doscientos","trescientos","cuatrocientos","quinientos",
          "seiscientos","setecientos","ochocientos","novecientos" };

    private static string NumeroEnLetraSimple(int n)
    {
        if (n == 0) return "cero";
        if (n < 0) return "menos " + NumeroEnLetraSimple(-n);
        if (n <= 20) return Unidades[n];
        if (n < 30) return "veinti" + Unidades[n - 20];
        if (n < 100)
        {
            var d = n / 10;
            var u = n % 10;
            return u == 0 ? Decenas[d] : Decenas[d] + " y " + Unidades[u];
        }
        if (n == 100) return "cien";
        if (n < 1000)
        {
            var c = n / 100;
            var r = n % 100;
            return r == 0 ? Centenas[c] : Centenas[c] + " " + NumeroEnLetraSimple(r);
        }
        if (n < 1_000_000)
        {
            var miles = n / 1000;
            var r = n % 1000;
            var partMiles = miles == 1 ? "mil" : NumeroEnLetraSimple(miles) + " mil";
            return r == 0 ? partMiles : partMiles + " " + NumeroEnLetraSimple(r);
        }
        var millones = n / 1_000_000;
        var resto = n % 1_000_000;
        var partMill = millones == 1 ? "un millón" : NumeroEnLetraSimple(millones) + " millones";
        return resto == 0 ? partMill : partMill + " " + NumeroEnLetraSimple(resto);
    }

    private static string NumeroEnLetra(decimal monto)
    {
        var entero = (int)decimal.Truncate(monto);
        var centavos = (int)((monto - decimal.Truncate(monto)) * 100);
        var parte1 = NumeroEnLetraSimple(entero) + " pesos";
        var parte2 = centavos == 0
            ? "00/100 M.N."
            : centavos.ToString("D2") + "/100 M.N.";
        return $"{parte1} {parte2}";
    }
}
