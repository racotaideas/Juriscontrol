namespace JurisControl.Data.Services;

/// <summary>
/// Storage local en disco para adjuntos del despacho. Escribe en
/// <c>{ContentRoot}/App_Data/uploads/{despachoId}/{yyyyMMdd}/{fileId}{ext}</c>.
/// Para v2 se puede migrar a Azure Blob / S3 implementando la misma interfaz.
/// </summary>
public interface IFileStorage
{
    /// <summary>Guarda el stream y devuelve la ruta relativa (StorageRef) para persistir en BD.</summary>
    Task<string> SaveAsync(Guid despachoId, string originalFileName, Stream contenido, CancellationToken ct = default);

    /// <summary>Abre el archivo apuntado por storageRef. Lanza si no existe.</summary>
    Stream OpenRead(string storageRef);

    /// <summary>Borra el archivo del disco. No lanza si ya no existe.</summary>
    void Delete(string storageRef);
}

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(string contentRootPath)
    {
        _root = Path.Combine(contentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Guid despachoId, string originalFileName, Stream contenido, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";
        var fileId = Guid.NewGuid().ToString("N");
        var fecha = DateTime.UtcNow.ToString("yyyyMMdd");
        var relDir = Path.Combine(despachoId.ToString(), fecha);
        var absDir = Path.Combine(_root, relDir);
        Directory.CreateDirectory(absDir);
        var absPath = Path.Combine(absDir, fileId + ext);
        await using var fs = File.Create(absPath);
        await contenido.CopyToAsync(fs, ct);
        return Path.Combine(relDir, fileId + ext).Replace('\\', '/');
    }

    public Stream OpenRead(string storageRef)
    {
        var abs = Path.Combine(_root, storageRef);
        return File.OpenRead(abs);
    }

    public void Delete(string storageRef)
    {
        var abs = Path.Combine(_root, storageRef);
        if (File.Exists(abs)) File.Delete(abs);
    }
}
