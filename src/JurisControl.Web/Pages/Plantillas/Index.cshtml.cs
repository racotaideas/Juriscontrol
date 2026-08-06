using JurisControl.Data;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Plantillas;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public IndexModel(JurisControlDbContext db) => _db = db;

    public List<Plantilla> Items { get; private set; } = new();
    public string? Q { get; set; }

    public async Task OnGetAsync(string? q)
    {
        Q = q?.Trim();
        var query = _db.Plantillas.AsNoTracking().Where(p => p.Activa);
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var like = $"%{Q}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Clave, like) ||
                EF.Functions.Like(p.Nombre, like) ||
                EF.Functions.Like(p.Categoria, like));
        }
        Items = await query.OrderBy(p => p.Categoria).ThenBy(p => p.Nombre).ToListAsync();
    }
}
