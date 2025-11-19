# Guidelines: Extending the Acme Platform

## How to Add/Remove/Extend a Module

### 1. Add a New Module

- Create a new folder under `/modules` or `/src/Services` as appropriate.
- Use the naming convention: `ModuleName` (singular).
- Scaffold projects using the pattern: `{Module}.{Layer}` (e.g., `UserManagement.Api`).
- Add to the solution:
  ```sh
  dotnet new classlib -n UserManagement.Domain -o src/Modules/UserManagement/UserManagement.Domain
  dotnet sln add src/Modules/UserManagement/UserManagement.Domain/UserManagement.Domain.csproj
  # Repeat for Api, Application, Infrastructure, Contracts
  ```
- Register dependencies in DI in the `Api` or `Application` layer.
- Add configuration to `appsettings.{Environment}.json` if needed.

### 2. Remove a Module

- Remove the module folder and all projects from the solution.
- Clean up references in DI, configuration, and documentation.

### 3. Extend a Module

- Add new features in the appropriate layer (Domain, Application, Infrastructure, Api).
- Follow Clean Architecture and DDD principles.
- Add tests in `/tests/{Module}.UnitTests` and `/tests/{Module}.IntegrationTests`.

---

## 10-Minute First Endpoint

1. **Create a Request/Response DTO**
   - Place in `{Service}.Contracts`.
2. **Add a Command/Query Handler**
   - Use MediatR for CQRS-style handlers.
3. **Add a Controller or Minimal API Endpoint**
   - Place in `{Service}.Api/Controllers`.
4. **Register in DI**
   - Ensure handler and dependencies are registered.
5. **Add OpenAPI Annotations**
   - XML comments, `[SwaggerOperation]`, examples.
6. **Test**
   - Add unit/integration tests.
7. **Run and Verify**
   - `dotnet run` and check `/swagger`.

---

## Best Practices

- Use vertical slices for features.
- Keep domain logic persistence-ignorant.
- Use async/await and cancellation tokens.
- Enforce code style with `.editorconfig` and analyzers.
- Document all public APIs.

---

## References

- [Architecture Diagram](architecture-diagram.md)
- [Profile Deliverables](profile-deliverables.md)
- [Compliance Overview](compliance-overview.md)

---

_This guide helps new contributors quickly add or extend modules and endpoints._
