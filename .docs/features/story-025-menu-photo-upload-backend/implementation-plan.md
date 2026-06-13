# Implementation Plan: Menu Photo Upload — Backend

## Phase 1 — Entity, Migration, and Storage Service (parallel)

- [ ] **task-01-photo-entity-migration** — `Photo` domain entity, EF model, Fluent config, migration.
- [ ] **task-02-photo-storage-service** — `IPhotoStorageService` interface + `LocalDiskPhotoStorageService` that saves files to a configured path and returns the URL.

## Phase 2 — Data and Application layers (parallel)

- [ ] **task-03-photo-data-handlers** — `SavePhotoCommand(restaurantId, url)` + handler and `GetPhotosQuery(restaurantId)` + handler.
- [ ] **task-04-photo-application-layer** — `UploadPhotoRequest` handler with file size + MIME validation, calls `IPhotoStorageService.StoreAsync(...)`, dispatches `SavePhotoCommand`.

## Phase 3 — Endpoints

- [ ] **task-05-photo-endpoints** — `POST /api/restaurants/{id}/photos` with `IFormFile` + Operator role check, and extension of `GET /api/restaurants/{id}` to include `photos`.
