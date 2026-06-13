# Requirements: Menu Photo Upload — Backend

## Summary

Restaurant operators need the ability to upload photos associated with their restaurant's menu and ambiance. This feature adds file upload validation (size ≤ 5 MB, MIME type JPEG/PNG/WebP), server-side storage, a `Photo` entity, and a `photos` array in the restaurant detail response. Only users with `role = "Operator"` may upload.

## Goals

- `POST /api/restaurants/{id}/photos` accepts an image file, validates it, stores it, and returns 201 with the photo URL.
- Files over 5 MB return 400.
- Non-JPEG/PNG/WebP files return 400.
- `GET /api/restaurants/{id}` response includes a `photos` array.
- Only Operator-role users may upload.

## Non-Goals

- No photo editing, cropping, or resizing.
- No diner-submitted photos.
- No CDN integration (local disk in dev, blob storage in prod via config).

## Acceptance Criteria

- [ ] `POST /api/restaurants/{id}/photos` by an Operator returns 201 with photo URL.
- [ ] File > 5 MB returns 400.
- [ ] Non-image MIME type returns 400.
- [ ] Non-operator JWT returns 403.
- [ ] `GET /api/restaurants/{id}` includes `photos` array.

## Technical Constraints

- `Photo` entity: `Id`, `RestaurantId`, `Url`, `UploadedAt`.
- `IPhotoStorageService` abstraction in `Infrastructure/Photos/`; local disk implementation for dev.
- Operator role check via `[Authorize(Roles = "Operator")]` or JWT `role` claim check at the endpoint.
- Max file size: 5 MB (5_242_880 bytes).
- Accepted MIME types: `image/jpeg`, `image/png`, `image/webp`.
