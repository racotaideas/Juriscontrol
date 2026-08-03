using JurisControl.Data;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Clientes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public IndexModel(JurisControlDbContext db) => _db = db;

    public List<Cliente> Items { get; private set; } = new();
    public string? Q { get; set; }
    public string? Tipo { get; set; }
    public int Total { get; private set; }

    public async Task OnGetAsync(string? q, string? tipo)
    {
        Q = q?.Trim();
        Tipo = tipo;

        var query = _db.Clientes.AsNoTracking().Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var like = $"%{Q}%";
            query = query.Where(c =>
                EF.Functions.Like(c.Nombre ?? "", like) ||
                EF.Functions.Like(c.ApellidoPaterno ?? "", like) ||
                EF.Functions.Like(c.ApellidoMaterno ?? "", like) ||
                EF.Functions.Like(c.RazonSocial ?? "", like) ||
                EF.Functions.Like(c.NombreComercial ?? "", like) ||
                EF.Functions.Like(c.Rfc ?? "", like) ||
                EF.Functions.Like(c.CorreoPrincipal ?? "", like));
        }

        if (Enum.TryParse<TipoCliente>(Tipo, out var t))
            query = query.Where(c => c.Tipo == t);

        Total = await query.CountAsync();
        Items = await query.OrderBy(c => c.RazonSocial ?? c.ApellidoPaterno ?? c.Nombre).Take(200).ToListAsync();
    }
}
