# Action Required: Menu Photo Upload — Backend

## Before Implementation

- [ ] **Configure photo storage path** — Add `Photos:StoragePath` to `appsettings.Development.json` (e.g., `"./uploads/photos"`). The directory will be created by the storage service on first upload. This path is gitignored; do not commit uploaded files.

---

> For production, replace the local-disk `IPhotoStorageService` implementation with a blob storage provider (Azure Blob, AWS S3, etc.) configured via `Photos:StorageProvider` in environment config.
