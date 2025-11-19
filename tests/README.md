# Test Coverage & Scaffolding

## Unit Tests

- Located in `tests/UnitTests/`
- Uses xUnit for test framework
- Covers core domain logic, value objects, and utility methods
- Example files: `OperationTests.cs`, `ResultTests.cs`, `WorkflowCaseTests.cs`

## Integration Tests

- Located in `tests/IntegrationTests/`
- Uses xUnit, FluentAssertions, Testcontainers, and Microsoft.AspNetCore.Mvc.Testing
- Covers API endpoints, database integration, and messaging (RabbitMQ)
- Example files: `IntegrationHubApiTests.cs`, `OutboxRabbitMqTests.cs`

## How to Run All Tests

```sh
dotnet test --collect:"XPlat Code Coverage"
```

## How to View Coverage Report

1. Run tests with coverage collection (see above)
2. Use a tool like [coverlet](https://github.com/coverlet-coverage/coverlet) or [ReportGenerator](https://github.com/danielpalme/ReportGenerator) to generate HTML reports:
   ```sh
   dotnet tool install -g dotnet-reportgenerator-globaltool
   reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
   ```
3. Open `coverage-report/index.html` in your browser

## Next Steps

- [ ] Add more unit tests for edge cases and error handling
- [ ] Add integration tests for all major API endpoints
- [ ] Add automated test coverage checks to CI pipeline
- [ ] Expand test data and scenario coverage

---

_This scaffold ensures a foundation for robust automated testing and coverage tracking._
