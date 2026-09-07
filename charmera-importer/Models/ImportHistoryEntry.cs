using System;

namespace charmera_importer.Models;

public sealed record ImportHistoryEntry(
    string Sha256Hash,
    string OriginalFileName,
    string DestinationPath,
    DateTime ImportedAtUtc,
    long FileSizeBytes);
