# Requirements: Menu Photo Upload — Backend

## Summary

Restaurant operators upload entree and menu photos to attract diners. Photos are validated server-side (size ≤ 5MB, MIME type must be image/jpeg, image/png, or image/webp). Stored locally for dev, blob storage for prod. Photo URLs included in the restaurant detail response.

## Goals

- `POST /api/restaurants/{id}/photos` accepts file upload
- Validates size ≤ 5MB and MIME type (jpeg/png/webp)
- Returns 201 with photo URL
- Requires Operator role (JWT claim)

## Acceptance Criteria

- [ ] Valid upload returns 201 with photoUrl
- [ ] File > 5MB returns 400
- [ ] Non-image MIME type returns 400
- [ ] Non-Operator JWT returns 403
- [ ] Photos included in GET /api/restaurants/{id} response

## Technical Constraints

- Store files in `wwwroot/photos/` (dev); abstract via `IPhotoStorageService`
- `Photo` entity: Id, RestaurantId, Url, UploadedAt
