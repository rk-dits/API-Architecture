# Testing & Coverage

## Test Coverage (Coverlet)

- All test projects use `coverlet.collector` for code coverage.
- Coverage is collected in CI and published to Codecov.
- Minimum coverage thresholds can be enforced in CI.

### Add Coverlet to Test Projects

```sh
dotnet add package coverlet.collector --version 6.0.0 --package-directory ./packages
```

## Mutation Testing (Stryker.NET)

- Stryker.NET is used for mutation testing to ensure test quality.
- Run locally:
  ```sh
  dotnet tool install -g dotnet-stryker
  dotnet stryker --solution-path Acme.Platform.sln --reporters "[\"html\",\"progress\"]"
  ```
- Configure thresholds in `stryker-config.json`:

```json
{
  "thresholds": {
    "high": 80,
    "low": 60,
    "break": 50
  },
  "reporters": ["html", "progress"]
}
```

## Quality Gates

- CI fails if coverage or mutation score drops below thresholds.
- Linting enforced via `dotnet format` and analyzers.

---

_See also: [CI Pipeline](../pipelines/github-actions/ci.yml)_
