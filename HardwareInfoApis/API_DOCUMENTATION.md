# HardwareInfoApis - API Documentation

## Overview
This document describes the HTTP API exposed by the `HardwareInfoApis` project.
The API provides device registration and license-related endpoints used by client applications to register devices, check device status, and receive simple health information.

Base URL: `https://{host}/` (configured in `Program.cs`). Swagger is available in Development at `/swagger` and served at root when enabled.

Authentication: Endpoints support JWT Bearer authentication. Some endpoints are marked `[AllowAnonymous]` and do not require authentication.

---

## Common response wrapper
Most endpoints return an `ApiResponse<T>` object with the following shape:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* T */ },
  "errorCode": null,
  "timestamp": "2024-01-01T00:00:00Z"
}
```

On error `success` is `false` and `errorCode` contains a string representation of `ApiErrorCode` (see section below).

---

## Error codes (`ApiErrorCode`)
Common values used by the API:
- `None`
- `InvalidRequest`
- `InvalidFingerprint`
- `InvalidLicenseKey`
- `LicenseExpired`
- `LicenseRevoked`
- `DeviceLimitReached`
- `DeviceAlreadyRegistered`
- `DeviceBlocked`
- `LicenseInvalid`
- `ServerError`
- `DatabaseError`
- `ConfigurationError`
- `Unauthorized`
- `Forbidden`
- `InvalidToken`
- `TokenExpired`
- `RateLimited`
- `QuotaExceeded`

---

## Endpoints

### GET /api/health
- Description: Simple health check endpoint.
- Auth: None
- Response: `200 OK` with a small JSON object

Example response:

```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

---

### POST /api/devices/check
- Description: Verify whether a device fingerprint is registered and return basic device/license info.
- Auth: Anonymous (can also be protected)
- Request body: `CheckDeviceRequest`

`CheckDeviceRequest` (JSON):
```json
{
  "deviceFingerprint": "<64-char-sha256-lower-hex>",
  "appVersion": "1.0.0",
  "licenseKey": "OPTIONAL-LICENSE",
  "includeHardwareComparison": false,
  "updateLastSeen": true
}
```

- Success response: `200 OK` with `ApiResponse<CheckDeviceResponse>`
- Common errors: `InvalidFingerprint`, `ServerError`

Example success response:
```json
{
  "success": true,
  "message": "Device found",
  "data": {
    "isRegistered": true,
    "registrationDate": "2024-01-01T00:00:00Z",
    "lastSeenAt": "2024-01-02T12:00:00Z",
    "deviceName": "My PC",
    "licenseStatus": "Active",
    "requiresUpdate": false
  },
  "errorCode": null,
  "timestamp": "2024-01-02T12:00:00Z"
}
```

---

### POST /api/devices/register
- Description: Register a new device with its hardware snapshot and optional license key.
- Auth: Anonymous (can also be protected)
- Request body: `RegisterDeviceRequest`

`RegisterDeviceRequest` (JSON):
```json
{
  "deviceFingerprint": "<64-char-sha256-lower-hex>",
  "hardwareInfo": { /* DeviceHardwareInfo object with CPU, BIOS, Storage, Memory, OS */ },
  "appVersion": "1.0.0",
  "licenseKey": "OPTIONAL-LICENSE",
  "deviceName": "Optional Device Name"
}
```

- Success response: `201 Created` with `ApiResponse<RegisterDeviceResponse>`
- Common errors: `InvalidFingerprint`, `LicenseInvalid`, `DeviceAlreadyRegistered`, `ServerError`

Example success response (201):
```json
{
  "success": true,
  "message": "Device registered successfully",
  "data": {
    "deviceId": "123",
    "registrationDate": "2024-01-02T12:00:00Z",
    "licenseKey": "DEMO-UNLIMITED-KEY",
    "licenseExpiryDate": "2025-01-01T00:00:00Z"
  },
  "errorCode": null,
  "timestamp": "2024-01-02T12:00:00Z"
}
```

---

### GET /api/devices/{id}
- Description: Retrieve device details by numeric ID or by fingerprint (fallback when `id` is not numeric).
- Auth: Requires `Admin` role (controller configured with `[Authorize(Roles = "Admin")]` on this action).
- Response: `200 OK` with `ApiResponse<DeviceDto>`, or `404` if not found.

`DeviceDto` fields: `id`, `deviceFingerprint`, `deviceName`, `registeredAt`, `lastSeenAt`, `licenseStatus`.

---

### POST /api/devices/{id}/heartbeat
- Description: Update device last-seen timestamp (heartbeat).
- Auth: Anonymous
- Request body: `HeartbeatRequest` (contains `appVersion`)
- Responses: `204 No Content` on success, `404` if device not found.

Example `HeartbeatRequest`:
```json
{
  "appVersion": "1.0.1"
}
```

---

## Notes
- Swagger/OpenAPI: The project already registers `SwaggerGen` in `Program.cs`. Visit `/swagger` in development to view generated OpenAPI UI and try endpoints.
- Rate limiting: A fixed-window limiter named `api` is configured; excessive requests return `429 Too Many Requests`.
- Database: Entity Framework Core with SQL Server is configured via `ApplicationDbContext` and migrations are applied in development on app start.

---

## Where to look in the code
- Controllers: `HardwareInfoApis\Controllers` and `HardwareInfoApis\Api\Controllers`
- Models (requests/responses): `HardwareInfoApis\Models\Api\Requests` and `HardwareInfoApis\Models\Api\Responses`
- Services: `HardwareInfoApis\Api\Services` and `HardwareInfoApis\Api\Services\Interfaces`
- Middleware: `HardwareInfoApis\Middleware` and `HardwareInfoApis\Api\Middleware`
- App entry: `HardwareInfoApis\Program.cs`

---

If you want, I can:
- Generate a full OpenAPI (yaml/json) file from the running app.
- Commit this documentation to the repo and open a PR.
- Expand examples for the `DeviceHardwareInfo` model and license endpoints.
