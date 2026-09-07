using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public interface IPhotoScannerService
{
    Task<IReadOnlyList<PhotoImportCandidate>> ScanAsync(string rootPath, CancellationToken ct = default);
}
