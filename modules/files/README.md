# Files module

Business file management module for Platform hosts. Orchestrates `IBlobStoringService` through `IFileManager` with optional HTTP API.

## Projects

| Project | Role |
|---------|------|
| `Platform.Modules.Files` | Core: `IFileManager`, validation, storage resolver |
| `Platform.Modules.Files.API` | HTTP API (`UseHttpApi`) — opt-in |

## Host wiring

Blob storage and Files module are registered separately (same pattern as Setting):

```csharp
using Platform.BlobStoring.Extensions;
using Platform.Modules.Files.Extensions;
using Platform.Modules.Files.API.Extensions;

// Blob provider (host-owned)
builder.AddCoreBlobStoring()
    .UseFileSystem();

// Files module
builder.AddCoreFiles()
    .UseHttpApi();
```

Sample app uses `AddSampleFiles()` which wires optional SharePoint/Synology when configured.

## API routes

| Route | Description |
|-------|-------------|
| `api/v{version}/files/{bucket}/*` | Upload, download, list, rename, move, delete |
| `api/v{version}/files/tree` | Folder tree (requires `UseSharePointTree` / `UseSynologyTree`) |
