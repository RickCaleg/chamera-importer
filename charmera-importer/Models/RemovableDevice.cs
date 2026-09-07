namespace charmera_importer.Models;

public sealed record RemovableDevice(
    string RootPath,
    string DisplayName,
    long? TotalSizeBytes,
    long? AvailableFreeBytes);
