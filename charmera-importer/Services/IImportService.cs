using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public interface IImportService
{
    Task ImportAsync(
        IReadOnlyList<PhotoImportCandidate> candidates,
        ImportSettings settings,
        IProgress<ImportProgress> progress,
        CancellationToken ct = default);
}
