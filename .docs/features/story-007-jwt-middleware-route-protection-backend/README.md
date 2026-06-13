# JWT Middleware & Route Protection — Backend

## Overview

This feature configures JWT Bearer authentication middleware and CORS policy in the TableNow API so that reservation endpoints are protected and unauthenticated requests return 401. JWT secret and settings are loaded from environment configuration. The CORS policy allows only the known Angular origin in development and the production frontend origin in prod. All reservation endpoint groups use `.RequireAuthorization()`.

## Quick Links

- [Requirements](./requirements.md) — full requirements and acceptance criteria
- [Action Required](./action-required.md) — manual steps needing human action
- [Implementation Plan](./implementation-plan.md) — phased task checklist

## Dependency Graph

```mermaid
graph TD
    task-01-jwt-bearer-cors-setup["01: JWT Bearer & CORS Setup"]
```

## Phases

| Phase | Tasks | Description |
|------|-------|-------------|
| 1 | task-01 | Add JWT bearer middleware, configure `JwtOptions`, wire up CORS, and apply `.RequireAuthorization()` to reservation routes. |

## Task Status

### Phase 1
- [ ] [task-01-jwt-bearer-cors-setup](./tasks/task-01-jwt-bearer-cors-setup.md) — JWT bearer authentication + CORS middleware
