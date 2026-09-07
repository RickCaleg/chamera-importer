using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using charmera_importer.Models;

namespace charmera_importer.Services;

public sealed class WindowsRemovableDeviceService : IRemovableDeviceService
{
    public Task<IReadOnlyList<RemovableDevice>> GetRemovableDevicesAsync(CancellationToken ct = default)
    {
        return Task.Run<IReadOnlyList<RemovableDevice>>(() =>
        {
            var devices = new List<RemovableDevice>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                ct.ThrowIfCancellationRequested();

                if (drive.DriveType != DriveType.Removable || !drive.IsReady)
                {
                    continue;
                }

                var displayName = string.IsNullOrWhiteSpace(drive.VolumeLabel)
                    ? drive.Name
                    : drive.VolumeLabel;

                devices.Add(new RemovableDevice(
                    drive.RootDirectory.FullName,
                    displayName,
                    drive.TotalSize,
                    drive.AvailableFreeSpace));
            }

            return devices;
        }, ct);
    }
}
