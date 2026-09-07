using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public sealed class JsonAppSettingsService : IAppSettingsService
{
    private readonly string settingsFilePath;
    private readonly SemaphoreSlim gate = new(1, 1);

    public JsonAppSettingsService(string settingsFilePath)
    {
        this.settingsFilePath = settingsFilePath;
    }

    public async Task<AppSettings> LoadAsync(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            if (!File.Exists(settingsFilePath))
            {
                return new AppSettings(null, null, null, null);
            }

            await using var stream = File.OpenRead(settingsFilePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, cancellationToken: ct);
            return settings ?? new AppSettings(null, null, null, null);
        }
        catch
        {
            // Missing/corrupt settings file — fall back to defaults rather than failing startup.
            return new AppSettings(null, null, null, null);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var directory = Path.GetDirectoryName(settingsFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = settingsFilePath + ".tmp";
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, settings, cancellationToken: ct);
            }

            File.Move(tempPath, settingsFilePath, overwrite: true);
        }
        finally
        {
            gate.Release();
        }
    }
}
