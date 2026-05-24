# PulseEDR — Manual Test Plan
**Version:** 1.0  
**Tester:** Laurentiu  
**Date:** 2026-05-24

## Scope
Manual functional testing of PulseEDR Desktop + API endpoints.

## Prerequisites
- Docker Desktop running (`pulseedr-postgres` healthy)
- API running on `http://localhost:5015`
- Desktop app running

## Test Cases

| ID | Feature | Steps | Expected Result | Status |
|---|---|---|---|---|
| TC-01 | Login valid | Enter admin/admin, click Connect | "Connected as admin" shown in green | ✅ |
| TC-02 | Login invalid | Enter admin/wrong, click Connect | Status shows error | ✅ |
| TC-03 | Dashboard load | After login, click Dashboard | 6 cards with live data | ✅ |
| TC-04 | Run Scan | Click Scan → Start Scan | Risk Score + Severity + Alerts appear | ✅ |
| TC-05 | Scan result | After scan completes | machineName, riskScore, alertCount populated | ✅ |
| TC-06 | Alerts view | Click Alerts | DataGrid populated with Severity/Score/Category/Title | ✅ |
| TC-07 | Health check | GET /api/health via Swagger | 200 OK + status: ok | ✅ |
| TC-08 | Unauthorized | GET /api/scan without token | 401 Unauthorized | ✅ |
| TC-09 | CVE list | GET /api/cve via Swagger | List of known CVEs returned | ✅ |
| TC-10 | What-If | POST /api/whatif via Swagger | Risk analysis returned | ✅ |

## Test Environment
- OS: Windows 11
- .NET: 8.0.421
- PostgreSQL: 16 (Docker)
- Browser: Swagger UI

## Results Summary
- Total: 10
- Passed: 10
- Failed: 0