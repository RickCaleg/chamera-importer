using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Win32;

[assembly: SupportedOSPlatform("windows")]

namespace CharmeraImporterSetup;

// A minimal, dependency-free installer: extracts the embedded app exe, creates Start
// Menu/Desktop shortcuts, and registers a normal "Add or Remove Programs" entry — all
// per-user (HKCU + %LocalAppData%), so it never needs administrator elevation.
internal static class Program
{
    private const string AppName = "Charmera Importer";
    private const string AppVersion = "0.1.0";
    private const string UninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\CharmeraImporter";

    private static int Main(string[] args)
    {
        var uninstall = Array.Exists(args, a => a.Equals("--uninstall", StringComparison.OrdinalIgnoreCase));
        var silent = Array.Exists(args, a => a.Equals("--silent", StringComparison.OrdinalIgnoreCase) || a.Equals("-y", StringComparison.OrdinalIgnoreCase));
        var customDir = Array.Find(args, a => a.StartsWith("--dir=", StringComparison.OrdinalIgnoreCase));
        var installDir = customDir is not null
            ? customDir["--dir=".Length..]
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "CharmeraImporter");

        try
        {
            return uninstall ? RunUninstall(installDir, silent) : RunInstall(installDir, silent);
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            PauseUnlessSilent(silent);
            return 1;
        }
    }

    private static int RunInstall(string installDir, bool silent)
    {
        Console.WriteLine($"{AppName} {AppVersion} - Setup");
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"Install location: {installDir}");

        if (!silent)
        {
            Console.Write("Continue? [Y/n] ");
            var response = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(response) && !response.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Setup cancelled.");
                return 1;
            }
        }

        Directory.CreateDirectory(installDir);

        var appExePath = Path.Combine(installDir, "charmera-importer.exe");
        Console.WriteLine("Extracting application files...");
        using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("charmera-importer.exe")
            ?? throw new InvalidOperationException("Embedded application payload not found in this installer."))
        using (var fileStream = File.Create(appExePath))
        {
            resourceStream.CopyTo(fileStream);
        }

        // Copy this installer itself into the install dir so uninstall keeps working even
        // after the original downloaded Setup.exe is deleted.
        var uninstallerPath = Path.Combine(installDir, "Uninstall.exe");
        var runningExePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Could not determine the running installer's path.");
        File.Copy(runningExePath, uninstallerPath, overwrite: true);

        Console.WriteLine("Creating shortcuts...");
        var startMenuDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft", "Windows", "Start Menu", "Programs");
        CreateShortcut(Path.Combine(startMenuDir, $"{AppName}.lnk"), appExePath, installDir);
        CreateShortcut(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{AppName}.lnk"),
            appExePath, installDir);

        Console.WriteLine("Registering with Windows (Add or Remove Programs)...");
        using (var key = Registry.CurrentUser.CreateSubKey(UninstallKeyPath))
        {
            key.SetValue("DisplayName", AppName);
            key.SetValue("DisplayVersion", AppVersion);
            key.SetValue("Publisher", "Richardson Calegari Saconi");
            key.SetValue("DisplayIcon", appExePath);
            key.SetValue("InstallLocation", installDir);
            key.SetValue("UninstallString", $"\"{uninstallerPath}\" --uninstall");
            key.SetValue("QuietUninstallString", $"\"{uninstallerPath}\" --uninstall --silent");
            key.SetValue("NoModify", 1, RegistryValueKind.DWord);
            key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            key.SetValue("EstimatedSize", new FileInfo(appExePath).Length / 1024, RegistryValueKind.DWord);
        }

        Console.WriteLine();
        Console.WriteLine("Installation complete.");
        PauseUnlessSilent(silent);
        return 0;
    }

    private static int RunUninstall(string installDir, bool silent)
    {
        Console.WriteLine($"{AppName} - Uninstall");
        Console.WriteLine(new string('-', 40));

        if (!silent)
        {
            Console.Write($"Remove {AppName} from {installDir}? [Y/n] ");
            var response = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(response) && !response.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Cancelled.");
                return 1;
            }
        }

        Registry.CurrentUser.DeleteSubKeyTree(UninstallKeyPath, throwOnMissingSubKey: false);

        var startMenuShortcut = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft", "Windows", "Start Menu", "Programs", $"{AppName}.lnk");
        var desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{AppName}.lnk");
        File.Delete(startMenuShortcut);
        File.Delete(desktopShortcut);

        Console.WriteLine("Removing application files...");

        // Uninstall.exe is itself inside installDir and can't delete its own running file on
        // Windows, so everything else is removed now, and a short-lived detached helper
        // process finishes the job (deleting this exe + the now-empty folder) after we exit.
        var appExePath = Path.Combine(installDir, "charmera-importer.exe");
        if (File.Exists(appExePath))
        {
            File.Delete(appExePath);
        }

        var runningExePath = Environment.ProcessPath;
        if (runningExePath is not null && string.Equals(Path.GetFullPath(runningExePath), Path.GetFullPath(Path.Combine(installDir, "Uninstall.exe")), StringComparison.OrdinalIgnoreCase))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c ping -n 2 127.0.0.1 >nul & rmdir /s /q \"{installDir}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
        else if (Directory.Exists(installDir))
        {
            Directory.Delete(installDir, recursive: true);
        }

        Console.WriteLine("Uninstall complete.");
        PauseUnlessSilent(silent);
        return 0;
    }

    private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("WScript.Shell is not available on this system.");
        dynamic shell = Activator.CreateInstance(shellType)!;
        try
        {
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            try
            {
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = workingDirectory;
                shortcut.IconLocation = targetPath;
                shortcut.Save();
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            }
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
        }
    }

    private static void PauseUnlessSilent(bool silent)
    {
        if (silent)
        {
            return;
        }

        Console.WriteLine("Press Enter to close...");
        Console.ReadLine();
    }
}
