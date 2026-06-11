# Requirements: Menu Photo Upload — Backend

## Summary

Operators (`role = "Operator"`) upload photos via `POST /api/restaurants/{id}/photos`. Validates file size (≤5 MB) and MIME type (JPEG/PNG/WebP). Photos included in restaurant detail response.

## Goals

- Operator can upload photos
- 400 for oversized or invalid file type
- Photos included in restaurant detail
- Non-operator returns 403

## Acceptance Criteria

- [ ] Valid upload returns 201 with photo URL
- [ ] File > 5 MB returns 400
- [ ] Non-image MIME type returns 400
- [ ] Non-operator returns 403
- [ ] Photos array in restaurant detail response

## Technical Constraints

- `Photo` entity: `Id, RestaurantId, Url, UploadedAt`
- Local disk storage for dev; configurable path
- Operator check via JWT `role` claim
