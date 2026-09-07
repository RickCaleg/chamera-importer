using System.Threading;
using System.Threading.Tasks;

namespace charmera_importer.Services;

public interface IHashingService
{
    Task<string> ComputeSha256Async(string filePath, CancellationToken ct = default);
}
