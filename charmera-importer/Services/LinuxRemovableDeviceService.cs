using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public sealed class LinuxRemovableDeviceService : IRemovableDeviceService
{
    private const string ProcMountsPath = "/proc/mounts";
    private const string SysBlockPath = "/sys/block";

    public Task<IReadOnlyList<RemovableDevice>> GetRemovableDevicesAsync(CancellationToken ct = default)
    {
        return Task.Run<IReadOnlyList<RemovableDevice>>(() =>
        {
            var mountsContent = File.Exists(ProcMountsPath) ? File.ReadAllText(ProcMountsPath) : string.Empty;
            var candidates = ParseCandidateMounts(mountsContent);

            var devices = new List<RemovableDevice>();
            foreach (var (devicePath, mountPoint) in candidates)
            {
                ct.ThrowIfCancellationRequested();

                if (!IsRemovable(devicePath, mountPoint))
                {
                    continue;
                }

                long? total = null;
                long? free = null;
                try
                {
                    var driveInfo = new DriveInfo(mountPoint);
                    total = driveInfo.TotalSize;
                    free = driveInfo.AvailableFreeSpace;
                }
                catch
                {
                    // Best-effort only — some mount points may not report usable stats.
                }

                var displayName = Path.GetFileName(mountPoint.TrimEnd('/'));
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = mountPoint;
                }

                devices.Add(new RemovableDevice(mountPoint, displayName, total, free));
            }

            return devices;
        }, ct);
    }

    // Extracted for testability: pure parsing of /proc/mounts content, no filesystem I/O.
    internal static List<(string DevicePath, string MountPoint)> ParseCandidateMounts(string procMountsContent)
    {
        var results = new List<(string, string)>();

        foreach (var line in procMountsContent.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            var devicePath = parts[0];
            var mountPoint = parts[1];

            if (!devicePath.StartsWith("/dev/", StringComparison.Ordinal))
            {
                continue;
            }

            if (!mountPoint.StartsWith("/media/", StringComparison.Ordinal) &&
                !mountPoint.StartsWith("/run/media/", StringComparison.Ordinal))
            {
                continue;
            }

            results.Add((devicePath, mountPoint));
        }

        return results;
    }

    // Extracted for testability: pure string transform, no filesystem I/O.
    internal static string ResolveParentDisk(string devicePath)
    {
        var deviceName = Path.GetFileName(devicePath);

        var match = Regex.Match(deviceName, @"^(?<disk>(sd[a-z]+|vd[a-z]+))\d+$");
        if (match.Success)
        {
            return match.Groups["disk"].Value;
        }

        match = Regex.Match(deviceName, @"^(?<disk>nvme\d+n\d+)p\d+$");
        if (match.Success)
        {
            return match.Groups["disk"].Value;
        }

        match = Regex.Match(deviceName, @"^(?<disk>mmcblk\d+)p\d+$");
        if (match.Success)
        {
            return match.Groups["disk"].Value;
        }

        return Regex.Replace(deviceName, @"\d+$", string.Empty);
    }

    private static bool IsRemovable(string devicePath, string mountPoint)
    {
        var disk = ResolveParentDisk(devicePath);
        var removableFlagPath = Path.Combine(SysBlockPath, disk, "removable");

        if (File.Exists(removableFlagPath))
        {
            try
            {
                var content = File.ReadAllText(removableFlagPath).Trim();
                if (content == "1")
                {
                    return true;
                }
            }
            catch
            {
                // Fall through to the mount-point-based fallback below.
            }
        }

        // Defensive fallback: some card readers misreport removable=0 for the reader disk
        // itself. A mount under /media or /run/media is still a strong removable-volume signal.
        return true;
    }
}
