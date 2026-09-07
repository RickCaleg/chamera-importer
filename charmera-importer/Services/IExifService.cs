using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public interface IExifService
{
    Task<PhotoExifData?> ReadAsync(string filePath, CancellationToken ct = default);
}
