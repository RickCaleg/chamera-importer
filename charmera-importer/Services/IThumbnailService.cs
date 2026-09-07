using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace charmera_importer.Services;

public interface IThumbnailService
{
    Task<Bitmap?> CreateThumbnailAsync(string filePath, int maxWidth = 220, CancellationToken ct = default);
}
