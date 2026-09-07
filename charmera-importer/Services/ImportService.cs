using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Localization;
using charmera_importer.Models;

namespace charmera_importer.Services;

public sealed class ImportService : IImportService
{
    private readonly IHashingService hashingService;
    private readonly IImportHistoryService historyService;

    public ImportService(IHashingService hashingService, IImportHistoryService historyService)
    {
        this.hashingService = hashingService;
        this.historyService = historyService;
    }

    public async Task ImportAsync(
        IReadOnlyList<PhotoImportCandidate> candidates,
        ImportSettings settings,
        IProgress<ImportProgress> progress,
        CancellationToken ct = default)
    {
        var total = candidates.Count;
        var completed = 0;

        foreach (var candidate in candidates)
        {
            ct.ThrowIfCancellationRequested();
            progress.Report(new ImportProgress(completed, total, candidate.FileName));

            try
            {
                await ImportOneAsync(candidate, settings, ct);
            }
            catch (Exception ex)
            {
                candidate.Status = ImportStatus.Error;
                candidate.StatusMessage = LocalizedStrings.Instance.ImportError(ex.Message);
            }

            completed++;
            progress.Report(new ImportProgress(completed, total, candidate.FileName));
        }
    }

    private async Task ImportOneAsync(PhotoImportCandidate candidate, ImportSettings settings, CancellationToken ct)
    {
        candidate.Sha256Hash ??= await hashingService.ComputeSha256Async(candidate.SourcePath, ct);

        if (historyService.ContainsHash(candidate.Sha256Hash))
        {
            candidate.Status = ImportStatus.Duplicate;
            candidate.StatusMessage = LocalizedStrings.Instance.AlreadyImportedMessage;
            TryDeleteSource(candidate, settings);
            return;
        }

        var destinationFolder = ImportPathResolver.ResolveDestinationFolder(candidate, settings);
        var destinationFileName = ImportPathResolver.ResolveDestinationFileName(candidate, settings);
        var desiredPath = Path.Combine(destinationFolder, destinationFileName);

        if (File.Exists(desiredPath) && await IsSameContentAsync(candidate, desiredPath, ct))
        {
            // Target already holds identical content (e.g. the history file was lost or this
            // is the first run against a pre-populated destination) — treat as a duplicate
            // instead of writing a redundant "_1" copy next to it.
            candidate.Status = ImportStatus.Duplicate;
            candidate.StatusMessage = LocalizedStrings.Instance.AlreadyAtDestinationMessage;
            TryDeleteSource(candidate, settings);
            return;
        }

        var destinationPath = ImportPathResolver.ResolveNonCollidingPath(desiredPath, File.Exists);

        Directory.CreateDirectory(destinationFolder);
        File.Copy(candidate.SourcePath, destinationPath, overwrite: false);

        await historyService.RecordImportAsync(
            new ImportHistoryEntry(candidate.Sha256Hash, candidate.FileName, destinationPath, DateTime.UtcNow, candidate.FileSizeBytes),
            ct);

        candidate.Status = ImportStatus.Imported;
        candidate.StatusMessage = LocalizedStrings.Instance.ImportedMessage;
        TryDeleteSource(candidate, settings);
    }

    private async Task<bool> IsSameContentAsync(PhotoImportCandidate candidate, string existingPath, CancellationToken ct)
    {
        if (new FileInfo(existingPath).Length != candidate.FileSizeBytes)
        {
            return false;
        }

        var existingHash = await hashingService.ComputeSha256Async(existingPath, ct);
        return existingHash == candidate.Sha256Hash;
    }

    // Only ever called once a photo is confirmed safely stored (freshly copied, or already
    // present at the destination/in history) — never for ImportStatus.Error. A failure here is
    // appended to the existing status message rather than replacing it, so the successful
    // import/dedup result isn't lost from view.
    private static void TryDeleteSource(PhotoImportCandidate candidate, ImportSettings settings)
    {
        if (!settings.DeleteSourceAfterImport)
        {
            return;
        }

        try
        {
            File.Delete(candidate.SourcePath);
        }
        catch (Exception ex)
        {
            candidate.StatusMessage = $"{candidate.StatusMessage} ({LocalizedStrings.Instance.DeleteFailedNote(ex.Message)})";
        }
    }
}
