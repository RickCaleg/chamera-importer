using System;
using System.Collections.Generic;

namespace charmera_importer.Models;

public sealed record PhotoExifData(
    string? CameraMake,
    string? CameraModel,
    DateTime? DateTaken,
    int? Width,
    int? Height,
    string? Orientation,
    double? FocalLengthMm,
    double? ExposureTimeSeconds,
    double? FNumber,
    int? IsoSpeed,
    string? GpsLatLong,
    IReadOnlyDictionary<string, string> AllTags);
