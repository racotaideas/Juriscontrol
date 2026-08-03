using JurisControl.Data;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Asuntos;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public DetailsModel(JurisControlDbContext db) => _db = db;

    public Asunto Asunto { get; private set; } = null!;
    public Cliente? Cliente { get; private set; }
    public ApplicationUser? Responsable { get; private set; }
    public List<Documento> Documentos { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var a = await _db.Asuntos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound();
        Asunto = a;
        Cliente = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == a.ClienteId);
        if (a.ResponsableId.HasValue)
            Responsable = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == a.ResponsableId.Value);
        Documentos = await _db.Documentos.AsNoTracking()
            .Where(d => d.AsuntoId == a.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        return Page();
    }
}
