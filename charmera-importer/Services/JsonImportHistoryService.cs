using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public sealed class JsonImportHistoryService : IImportHistoryService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string historyFilePath;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<string, ImportHistoryEntry> entriesByHash = new();

    public JsonImportHistoryService(string historyFilePath)
    {
        this.historyFilePath = historyFilePath;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            if (!File.Exists(historyFilePath))
            {
                return;
            }

            await using var stream = File.OpenRead(historyFilePath);
            var document = await JsonSerializer.DeserializeAsync<HistoryDocument>(stream, SerializerOptions, ct);

            entriesByHash.Clear();
            if (document?.Entries is not null)
            {
                foreach (var entry in document.Entries)
                {
                    entriesByHash[entry.Sha256Hash] = entry;
                }
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public bool ContainsHash(string sha256Hash) => entriesByHash.ContainsKey(sha256Hash);

    public async Task RecordImportAsync(ImportHistoryEntry entry, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            entriesByHash[entry.Sha256Hash] = entry;
            await WriteToDiskAsync(ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task WriteToDiskAsync(CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(historyFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var document = new HistoryDocument(1, new List<ImportHistoryEntry>(entriesByHash.Values));

        var tempPath = historyFilePath + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, ct);
        }

        File.Move(tempPath, historyFilePath, overwrite: true);
    }

    private sealed record HistoryDocument(int Version, List<ImportHistoryEntry> Entries);
}
