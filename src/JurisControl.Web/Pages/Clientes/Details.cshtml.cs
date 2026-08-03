using JurisControl.Data;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Clientes;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public DetailsModel(JurisControlDbContext db) => _db = db;

    public Cliente Cliente { get; private set; } = null!;
    public List<Asunto> Asuntos { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var c = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        Cliente = c;
        Asuntos = await _db.Asuntos.AsNoTracking()
            .Where(a => a.ClienteId == id)
            .OrderByDescending(a => a.FechaRecepcion)
            .Take(50)
            .ToListAsync();
        return Page();
    }
}
