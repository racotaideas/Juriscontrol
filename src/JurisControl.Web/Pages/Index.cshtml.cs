using JurisControl.Data;
using JurisControl.Data.TenantContext;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages;

public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly ITenantContext _tenant;

    public IndexModel(JurisControlDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public string RazonSocial { get; private set; } = "—";
    public string NombreUsuario { get; private set; } = "—";
    public string MateriasAtiende { get; private set; } = "—";
    public bool ModoCobranza { get; private set; }
    public Guid DespachoId => _tenant.DespachoId ?? Guid.Empty;

    public async Task OnGetAsync()
    {
        NombreUsuario = User.Identity?.Name ?? "usuario";

        // La query filter automáticamente restringe al despacho del usuario logueado —
        // aunque busquemos "cualquier despacho", solo aparece el suyo.
        var despacho = await _db.Despachos.AsNoTracking().SingleOrDefaultAsync();
        if (despacho is not null)
        {
            RazonSocial = despacho.RazonSocial;
            MateriasAtiende = despacho.MateriasAtiende.Replace(",", ", ");
            ModoCobranza = despacho.ModoCobranza;
        }
    }
}
