using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace charmera_importer.Services;

public sealed class ThumbnailService : IThumbnailService
{
    public Task<Bitmap?> CreateThumbnailAsync(string filePath, int maxWidth = 220, CancellationToken ct = default)
    {
        return Task.Run<Bitmap?>(() =>
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                using var stream = File.OpenRead(filePath);
                return Bitmap.DecodeToWidth(stream, maxWidth);
            }
            catch
            {
                // Unsupported format for decoding (e.g. RAW: .cr2/.nef/.arw/.dng) or corrupt file —
                // the UI falls back to a placeholder; EXIF reading is unaffected since it's independent.
                return null;
            }
        }, ct);
    }
}
