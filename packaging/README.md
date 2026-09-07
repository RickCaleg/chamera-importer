# Release packaging

How the release binaries are built. All commands below assume the repo root as the
working directory and a version number in `$VERSION` (e.g. `VERSION=0.1.0`).

## Linux binary (portable)

```bash
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o /tmp/publish/linux charmera-importer/charmera-importer.csproj

tar -czf charmera-importer-$VERSION-linux-x64.tar.gz -C /tmp/publish/linux charmera-importer
```

## .deb and .rpm

Built with [nfpm](https://nfpm.goreleaser.com) — a single static Go binary, deliberately
chosen over `dpkg-deb`/`rpmbuild` so packaging doesn't need anything installed system-wide
(download the `nfpm_<version>_Linux_x86_64.tar.gz` release asset from
https://github.com/goreleaser/nfpm/releases, verify against `checksums.txt`, done).

```bash
VERSION=$VERSION LINUX_BINARY=/tmp/publish/linux/charmera-importer \
  envsubst < packaging/nfpm.yaml > /tmp/nfpm.resolved.yaml

nfpm package --config /tmp/nfpm.resolved.yaml --packager deb --target .
nfpm package --config /tmp/nfpm.resolved.yaml --packager rpm --target .
```

## Windows installer

```bash
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o /tmp/publish/windows charmera-importer/charmera-importer.csproj

dotnet publish -c Release installer/CharmeraImporterSetup/CharmeraImporterSetup.csproj \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:AppExePath=/tmp/publish/windows/charmera-importer.exe \
  -o /tmp/publish/installer

# Result: /tmp/publish/installer/CharmeraImporterSetup.exe
```

`installer/CharmeraImporterSetup` is a small hand-written installer (see its `Program.cs`),
not WiX or NSIS output. Both were tried first and abandoned:

- **WiX** (v5 and v6): its own CLI states it only supports Windows, and in practice its
  `Directory/@Name` path validation is broken when the tool runs on Linux (rejects any
  valid relative name as "not a relative path") — a genuine cross-platform bug, not a
  config mistake, confirmed while building this release.
- **NSIS**: not in the official Arch repos (AUR-only), and every native packaging tool
  here (`dpkg`, `rpm-tools`, NSIS) needs root to install, which wasn't available in the
  environment this release was built in.

The custom installer extracts the embedded app to `%LocalAppData%\Programs\CharmeraImporter`
(no admin rights needed), creates Start Menu/Desktop shortcuts, and registers a normal
"Add or Remove Programs" entry (`Uninstall.exe --uninstall` reverses all of it). It has
**not been run on real Windows** — only cross-compiled and sanity-checked (embedded resource
size, PE header) from Linux — since no Windows machine was available either. Treat it as
best-effort until someone tests it on real hardware and reports back.

If a real Windows box (or Wine) becomes available for a future release, switching back to
WiX (which works fine natively on Windows) for a proper MSI is worth reconsidering —
`packaging/windows/` no longer exists in this repo (removed as dead code) but the `Product`/
`Bundle` .wxs approach outlined above is a reasonable starting point to recreate.
