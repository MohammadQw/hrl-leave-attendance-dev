# HRL — HR Leave & Attendance

Leave requests, approvals and balances for Diyar staff, following the Kuwait Private Sector
Labour Law (Law No. 6 of 2010) entitlements. Governed by **AKB** project **HRL**
(https://akb.diyardemo.com) — requirements, designs and approvals live there.

| Environment | Repository | Branches |
|---|---|---|
| Development | [hrl-leave-attendance-dev](https://github.com/MohammadQw/hrl-leave-attendance-dev) | `develop`, `feature/TASK-NNN-*` |
| Staging (UAT) | [hrl-leave-attendance-uat](https://github.com/MohammadQw/hrl-leave-attendance-uat) | `uat`, `release/*` |
| Production | [hrl-leave-attendance](https://github.com/MohammadQw/hrl-leave-attendance) | `main` only |

## Run

```bash
dotnet run --project src/HrLeave.Api      # http://localhost:5000/swagger-free minimal API
dotnet test                               # policy rules
```

## API

| Method | Path | Purpose |
|---|---|---|
| GET  | `/health` | Liveness |
| GET  | `/employees/{id}/balance` | Annual and sick leave remaining this year |
| POST | `/leave-requests` | Submit a request (validated against the policy) |
| POST | `/leave-requests/{id}/decision` | Manager approves or rejects |
