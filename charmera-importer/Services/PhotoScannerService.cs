using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public sealed class PhotoScannerService : IPhotoScannerService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".heic", ".heif", ".tif", ".tiff", ".bmp", ".webp", ".gif",
        ".cr2", ".nef", ".arw", ".dng",
    };

    public Task<IReadOnlyList<PhotoImportCandidate>> ScanAsync(string rootPath, CancellationToken ct = default)
    {
        return Task.Run<IReadOnlyList<PhotoImportCandidate>>(() =>
        {
            var dcimPath = Path.Combine(rootPath, "DCIM");
            var scanRoot = Directory.Exists(dcimPath) ? dcimPath : rootPath;

            var candidates = new List<PhotoImportCandidate>();

            foreach (var filePath in Directory.EnumerateFiles(scanRoot, "*", SearchOption.AllDirectories))
            {
                ct.ThrowIfCancellationRequested();

                if (!SupportedExtensions.Contains(Path.GetExtension(filePath)))
                {
                    continue;
                }

                FileInfo info;
                try
                {
                    info = new FileInfo(filePath);
                }
                catch
                {
                    continue;
                }

                candidates.Add(new PhotoImportCandidate
                {
                    SourcePath = filePath,
                    FileName = info.Name,
                    FileSizeBytes = info.Length,
                    FileSystemDateModified = info.LastWriteTime,
                });
            }

            return candidates;
        }, ct);
    }
}
