using JurisControl.Data;
using JurisControl.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Documentos;

[Authorize]
public class DownloadModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly IFileStorage _storage;
    public DownloadModel(JurisControlDbContext db, IFileStorage storage)
    {
        _db = db; _storage = storage;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        // El Global Query Filter garantiza que solo veo documentos de mi despacho.
        var doc = await _db.Documentos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (doc is null) return NotFound();
        if (string.IsNullOrWhiteSpace(doc.StorageRef)) return NotFound();

        Stream stream;
        try { stream = _storage.OpenRead(doc.StorageRef); }
        catch (FileNotFoundException) { return NotFound(); }

        return File(stream, doc.ContentType ?? "application/octet-stream", doc.Nombre);
    }
}
