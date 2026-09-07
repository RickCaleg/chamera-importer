using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;

namespace charmera_importer.Services;

public sealed class ExifService : IExifService
{
    public Task<PhotoExifData?> ReadAsync(string filePath, CancellationToken ct = default)
    {
        return Task.Run<PhotoExifData?>(() =>
        {
            ct.ThrowIfCancellationRequested();

            IReadOnlyList<MetadataExtractor.Directory> directories;
            try
            {
                directories = ImageMetadataReader.ReadMetadata(filePath);
            }
            catch
            {
                // Not every file yields readable metadata (unsupported format, corrupt file, etc.) —
                // treat this as "no EXIF available" rather than failing the whole scan.
                return null;
            }

            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var gps = directories.OfType<GpsDirectory>().FirstOrDefault();

            string? cameraMake = ifd0?.GetString(ExifDirectoryBase.TagMake)?.Trim();
            string? cameraModel = ifd0?.GetString(ExifDirectoryBase.TagModel)?.Trim();

            DateTime? dateTaken = null;
            if (subIfd is not null && subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var originalDate))
            {
                dateTaken = originalDate;
            }
            else if (subIfd is not null && subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var digitizedDate))
            {
                dateTaken = digitizedDate;
            }
            else if (ifd0 is not null && ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var modifiedDate))
            {
                dateTaken = modifiedDate;
            }

            // Cameras that don't write full EXIF (e.g. this Kodak) still have baseline JPEG SOF
            // dimensions, so fall back to those rather than leaving Width/Height empty.
            var jpeg = directories.OfType<JpegDirectory>().FirstOrDefault();
            int? width = TryGetInt(subIfd, ExifDirectoryBase.TagExifImageWidth)
                ?? TryGetInt(ifd0, ExifDirectoryBase.TagImageWidth)
                ?? TryGetInt(jpeg, JpegDirectory.TagImageWidth);
            int? height = TryGetInt(subIfd, ExifDirectoryBase.TagExifImageHeight)
                ?? TryGetInt(ifd0, ExifDirectoryBase.TagImageHeight)
                ?? TryGetInt(jpeg, JpegDirectory.TagImageHeight);

            string? orientation = ifd0 is not null && ifd0.ContainsTag(ExifDirectoryBase.TagOrientation)
                ? ifd0.GetDescription(ExifDirectoryBase.TagOrientation)
                : null;

            double? focalLength = TryGetDouble(subIfd, ExifDirectoryBase.TagFocalLength);
            double? exposureTime = TryGetDouble(subIfd, ExifDirectoryBase.TagExposureTime);
            double? fNumber = TryGetDouble(subIfd, ExifDirectoryBase.TagFNumber);
            int? isoSpeed = TryGetInt(subIfd, ExifDirectoryBase.TagIsoEquivalent);

            string? gpsLatLong = null;
            if (gps is not null && gps.TryGetGeoLocation(out var location) && !location.IsZero)
            {
                gpsLatLong = $"{location.Latitude}, {location.Longitude}";
            }

            var allTags = new Dictionary<string, string>();
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    var key = $"{directory.Name} - {tag.Name}";
                    allTags[key] = tag.Description ?? string.Empty;
                }
            }

            return new PhotoExifData(
                cameraMake,
                cameraModel,
                dateTaken,
                width,
                height,
                orientation,
                focalLength,
                exposureTime,
                fNumber,
                isoSpeed,
                gpsLatLong,
                allTags);
        }, ct);
    }

    private static int? TryGetInt(MetadataExtractor.Directory? directory, int tagType)
    {
        if (directory is null || !directory.ContainsTag(tagType))
        {
            return null;
        }

        try
        {
            return directory.GetInt32(tagType);
        }
        catch
        {
            return null;
        }
    }

    private static double? TryGetDouble(MetadataExtractor.Directory? directory, int tagType)
    {
        if (directory is null || !directory.ContainsTag(tagType))
        {
            return null;
        }

        try
        {
            return directory.GetDouble(tagType);
        }
        catch
        {
            return null;
        }
    }
}
