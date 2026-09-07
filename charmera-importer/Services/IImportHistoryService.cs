using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public interface IImportHistoryService
{
    Task LoadAsync(CancellationToken ct = default);
    bool ContainsHash(string sha256Hash);
    Task RecordImportAsync(ImportHistoryEntry entry, CancellationToken ct = default);
}
