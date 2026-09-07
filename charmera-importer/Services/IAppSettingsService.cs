using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public interface IAppSettingsService
{
    // Returns defaults (all null / AppendOriginalFileName = true) if no settings file exists yet
    // or it can't be read — first-run and corrupt-file cases are treated the same.
    Task<AppSettings> LoadAsync(CancellationToken ct = default);

    Task SaveAsync(AppSettings settings, CancellationToken ct = default);
}
