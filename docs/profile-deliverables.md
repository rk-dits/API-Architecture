# Profile-Specific Deliverables: Acme Platform API

This document outlines deliverables and configuration profiles for different deployment and usage scenarios, as required by the architecture prompt.

## 1. Development Profile

- **Purpose:** Local development, debugging, and rapid iteration
- **Features:**
  - Dev token endpoint enabled
  - Relaxed CORS policy
  - In-memory or local database
  - Verbose logging
  - Swagger UI enabled
- **Config:** `appsettings.Development.json`

## 2. Staging Profile

- **Purpose:** Pre-production validation, integration testing
- **Features:**
  - Realistic data and integrations
  - Production-like security settings
  - Rate limiting and security headers enforced
  - Swagger UI enabled (restricted access)
- **Config:** `appsettings.Staging.json`

## 3. Production Profile

- **Purpose:** Live environment, real users and data
- **Features:**
  - All security controls enforced
  - Dev/test endpoints disabled
  - Strict CORS policy
  - Centralized logging and monitoring
  - Swagger UI disabled or protected
- **Config:** `appsettings.Production.json`

## 4. Compliance Profile (Optional)

- **Purpose:** Enhanced controls for regulated workloads (HIPAA, SOC2, GDPR)
- **Features:**
  - Audit logging enabled
  - Data encryption at rest and in transit
  - Automated compliance checks
  - Data retention and disposal policies enforced
- **Config:** `appsettings.Compliance.json` (if needed)

## How to Use Profiles

- ASP.NET Core automatically selects the profile based on the `ASPNETCORE_ENVIRONMENT` variable.
- Example:
  ```sh
  set ASPNETCORE_ENVIRONMENT=Staging
  dotnet run
  ```
- Each profile can override settings in `appsettings.json`.

## Next Steps

- [ ] Review and complete all environment-specific configuration files
- [ ] Add compliance profile if required by customer/regulation
- [ ] Document any custom profile logic in the codebase

---

_This document ensures clarity and traceability for environment-specific deliverables and configuration._
