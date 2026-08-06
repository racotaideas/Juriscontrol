using JurisControl.Data;
using JurisControl.Data.TenantContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITenantContext _tenant;
    private readonly JurisControlDbContext _db;
    public IndexModel(ITenantContext tenant, JurisControlDbContext db)
    {
        _tenant = tenant; _db = db;
    }

    public bool ModoCobranza { get; private set; }

    public async Task OnGetAsync()
    {
        var miId = _tenant.DespachoId;
        var d = miId.HasValue
            ? await _db.Despachos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == miId.Value)
            : null;
        ModoCobranza = d?.ModoCobranza ?? false;
    }
}
