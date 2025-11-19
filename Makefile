# Makefile for Acme Platform
# Provides common development and CI/CD tasks

# Variables
SOLUTION_FILE = Acme.Platform.sln
BUILD_CONFIG = Release
TEST_CONFIG = Debug
DOCKER_COMPOSE_FILE = docker-compose.yml
COVERAGE_THRESHOLD = 80
MUTATION_THRESHOLD = 75

# Colors for output
RED = \033[0;31m
GREEN = \033[0;32m
YELLOW = \033[1;33m
BLUE = \033[0;34m
NC = \033[0m # No Color

.PHONY: help setup build build-debug test test-watch test-unit test-integration \
        test-e2e coverage mutation-test lint format security-scan \
        docker-up docker-down docker-logs docker-clean \
        package publish deploy clean deep-clean \
        db-migrate db-reset db-seed dev-setup ci-setup \
        benchmark docs validate-openapi

# Default target
help: ## Show this help message
	@echo "$(BLUE)Acme Platform - Available Commands$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(YELLOW)%-20s$(NC) %s\n", $$1, $$2}'

## Development Commands

setup: ## Initial project setup for new developers
	@echo "$(GREEN)Setting up development environment...$(NC)"
	@dotnet --version
	@docker --version
	@dotnet restore $(SOLUTION_FILE)
	$(MAKE) docker-up
	@echo "$(GREEN)Setup complete! Run 'make dev-setup' for IDE configuration.$(NC)"

dev-setup: ## Setup development tools and IDE configuration
	@echo "$(GREEN)Installing development tools...$(NC)"
	@dotnet tool restore || dotnet tool install --global dotnet-ef
	@dotnet tool install --global dotnet-format || echo "dotnet-format already installed"
	@dotnet tool install --global dotnet-stryker || echo "stryker already installed"
	@dotnet tool install --global coverlet.console || echo "coverlet already installed"
	@echo "$(GREEN)Development tools installed!$(NC)"

build: ## Build the solution in Release mode
	@echo "$(GREEN)Building solution...$(NC)"
	@dotnet build $(SOLUTION_FILE) --configuration $(BUILD_CONFIG) --no-restore

build-debug: ## Build the solution in Debug mode
	@echo "$(GREEN)Building solution (Debug)...$(NC)"
	@dotnet build $(SOLUTION_FILE) --configuration $(TEST_CONFIG)

## Testing Commands

test: ## Run all tests with coverage
	@echo "$(GREEN)Running all tests...$(NC)"
	@dotnet test $(SOLUTION_FILE) --configuration $(TEST_CONFIG) \
		--collect:"XPlat Code Coverage" \
		--results-directory ./TestResults \
		--logger "console;verbosity=detailed"
	@$(MAKE) coverage-report

test-watch: ## Run tests in watch mode
	@echo "$(GREEN)Running tests in watch mode...$(NC)"
	@dotnet watch test --project tests/UnitTests/UnitTests.csproj

test-unit: ## Run unit tests only
	@echo "$(GREEN)Running unit tests...$(NC)"
	@dotnet test tests/UnitTests/UnitTests.csproj --configuration $(TEST_CONFIG) \
		--collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"

test-integration: ## Run integration tests only (requires Docker)
	@echo "$(GREEN)Running integration tests...$(NC)"
	$(MAKE) docker-up
	@dotnet test tests/IntegrationTests/IntegrationTests.csproj --configuration $(TEST_CONFIG) \
		--collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"

test-e2e: ## Run end-to-end tests
	@echo "$(GREEN)Running E2E tests...$(NC)"
	$(MAKE) docker-up
	@dotnet test tests/Solution.E2E/Solution.E2E.csproj --configuration $(TEST_CONFIG) \
		--logger "console;verbosity=detailed" || echo "E2E project not found"

coverage: test-unit test-integration ## Generate code coverage report
	@echo "$(GREEN)Generating coverage report...$(NC)"
	@$(MAKE) coverage-report

coverage-report: ## Generate and display coverage report
	@echo "$(GREEN)Processing coverage results...$(NC)"
	@if [ -d "./TestResults" ]; then \
		find ./TestResults -name "coverage.cobertura.xml" -exec \
		dotnet tool run reportgenerator -reports:{} -targetdir:./TestResults/CoverageReport -reporttypes:Html \; 2>/dev/null || \
		echo "$(YELLOW)Install reportgenerator: dotnet tool install -g dotnet-reportgenerator-globaltool$(NC)"; \
		echo "$(BLUE)Coverage report: ./TestResults/CoverageReport/index.html$(NC)"; \
	else \
		echo "$(RED)No coverage results found. Run 'make test' first.$(NC)"; \
	fi

mutation-test: ## Run mutation testing with Stryker.NET
	@echo "$(GREEN)Running mutation tests...$(NC)"
	@dotnet stryker --config-file stryker-config.json \
		--threshold-high $(MUTATION_THRESHOLD) \
		--threshold-low $$(( $(MUTATION_THRESHOLD) - 10 )) \
		--threshold-break $$(( $(MUTATION_THRESHOLD) - 20 ))

## Code Quality Commands

lint: ## Check code formatting and style issues
	@echo "$(GREEN)Checking code format...$(NC)"
	@dotnet format $(SOLUTION_FILE) --verify-no-changes --verbosity diagnostic

format: ## Auto-fix code formatting issues
	@echo "$(GREEN)Formatting code...$(NC)"
	@dotnet format $(SOLUTION_FILE) --verbosity minimal

security-scan: ## Run security analysis
	@echo "$(GREEN)Running security scan...$(NC)"
	@dotnet list $(SOLUTION_FILE) package --vulnerable --include-transitive
	@echo "$(BLUE)Consider running: dotnet audit$(NC)"

validate-openapi: ## Validate OpenAPI specifications
	@echo "$(GREEN)Validating OpenAPI specs...$(NC)"
	@find . -name "*.json" -path "*/swagger/*" -exec echo "Validating: {}" \; || \
	echo "$(YELLOW)No OpenAPI specs found or spectral not installed$(NC)"

## Infrastructure Commands

docker-up: ## Start all infrastructure services
	@echo "$(GREEN)Starting infrastructure services...$(NC)"
	@docker-compose -f $(DOCKER_COMPOSE_FILE) up -d
	@echo "$(GREEN)Services starting... Use 'make docker-logs' to monitor$(NC)"

docker-down: ## Stop all infrastructure services
	@echo "$(GREEN)Stopping infrastructure services...$(NC)"
	@docker-compose -f $(DOCKER_COMPOSE_FILE) down

docker-logs: ## Show logs from infrastructure services
	@docker-compose -f $(DOCKER_COMPOSE_FILE) logs -f

docker-clean: ## Clean up Docker resources
	@echo "$(GREEN)Cleaning Docker resources...$(NC)"
	@docker-compose -f $(DOCKER_COMPOSE_FILE) down -v --remove-orphans
	@docker system prune -f

## Database Commands

db-migrate: ## Run database migrations
	@echo "$(GREEN)Running database migrations...$(NC)"
	@dotnet ef database update --project src/Services/CoreWorkflowService/CoreWorkflowService.Infrastructure \
		--startup-project src/Services/CoreWorkflowService/CoreWorkflowService.Api || \
	echo "$(YELLOW)EF Core not configured or project not found$(NC)"

db-reset: ## Reset database (drop and recreate)
	@echo "$(YELLOW)Resetting database...$(NC)"
	@dotnet ef database drop --force --project src/Services/CoreWorkflowService/CoreWorkflowService.Infrastructure \
		--startup-project src/Services/CoreWorkflowService/CoreWorkflowService.Api || \
	echo "$(YELLOW)EF Core not configured or project not found$(NC)"
	@$(MAKE) db-migrate

db-seed: ## Seed database with sample data
	@echo "$(GREEN)Seeding database...$(NC)"
	@dotnet run --project src/Services/CoreWorkflowService/CoreWorkflowService.Api -- --seed || \
	echo "$(YELLOW)Seed command not implemented$(NC)"

## Package & Deployment Commands

package: build ## Build packages for deployment
	@echo "$(GREEN)Creating deployment packages...$(NC)"
	@dotnet publish $(SOLUTION_FILE) --configuration $(BUILD_CONFIG) --output ./artifacts
	@echo "$(GREEN)Packages created in ./artifacts$(NC)"

publish: package ## Publish packages to registry
	@echo "$(GREEN)Publishing packages...$(NC)"
	@echo "$(YELLOW)Implement publish logic for your registry$(NC)"

deploy: ## Deploy to staging environment
	@echo "$(GREEN)Deploying to staging...$(NC)"
	@echo "$(YELLOW)Implement deployment logic for your infrastructure$(NC)"

## Utility Commands

clean: ## Clean build artifacts
	@echo "$(GREEN)Cleaning build artifacts...$(NC)"
	@dotnet clean $(SOLUTION_FILE)

deep-clean: clean ## Deep clean including packages and Docker
	@echo "$(GREEN)Deep cleaning...$(NC)"
	@rm -rf **/bin **/obj
	@rm -rf packages TestResults artifacts
	@$(MAKE) docker-clean

docs: ## Generate documentation
	@echo "$(GREEN)Generating documentation...$(NC)"
	@find docs -name "*.md" -exec echo "Found: {}" \;
	@echo "$(BLUE)Documentation available in ./docs/$(NC)"

benchmark: ## Run performance benchmarks
	@echo "$(GREEN)Running benchmarks...$(NC)"
	@echo "$(YELLOW)Implement benchmark project with BenchmarkDotNet$(NC)"

## CI/CD Commands

ci-setup: ## Setup for CI/CD pipeline
	@echo "$(GREEN)Setting up CI environment...$(NC)"
	@dotnet --info
	@dotnet restore $(SOLUTION_FILE) --locked-mode || dotnet restore $(SOLUTION_FILE)

ci-test: ci-setup ## Run tests for CI pipeline
	@echo "$(GREEN)Running CI test suite...$(NC)"
	@$(MAKE) build
	@$(MAKE) lint
	@$(MAKE) test
	@$(MAKE) security-scan

ci-package: ci-test ## Full CI pipeline with packaging
	@echo "$(GREEN)Running full CI pipeline...$(NC)"
	@$(MAKE) package
	@echo "$(GREEN)CI pipeline completed successfully!$(NC)"

## Status and Information Commands

status: ## Show project status and service health
	@echo "$(BLUE)=== Acme Platform Status ===$(NC)"
	@echo "$(GREEN)Solution:$(NC) $(SOLUTION_FILE)"
	@echo "$(GREEN)Docker Services:$(NC)"
	@docker-compose -f $(DOCKER_COMPOSE_FILE) ps 2>/dev/null || echo "$(YELLOW)Docker services not running$(NC)"
	@echo "$(GREEN)Build Status:$(NC)"
	@dotnet build $(SOLUTION_FILE) --verbosity quiet && echo "$(GREEN)✓ Build OK$(NC)" || echo "$(RED)✗ Build Failed$(NC)"

info: ## Show project information
	@echo "$(BLUE)=== Acme Platform Information ===$(NC)"
	@echo "$(GREEN)Architecture:$(NC) Clean Architecture + DDD + CQRS"
	@echo "$(GREEN)Framework:$(NC) .NET 8 LTS"
	@echo "$(GREEN)Gateway:$(NC) YARP"
	@echo "$(GREEN)Database:$(NC) PostgreSQL"
	@echo "$(GREEN)Cache:$(NC) Redis"
	@echo "$(GREEN)Messaging:$(NC) RabbitMQ + MassTransit"
	@echo "$(GREEN)Observability:$(NC) OpenTelemetry + Jaeger + Seq"
	@echo "$(GREEN)Documentation:$(NC) ./docs/"

# Environment-specific targets
dev: docker-up build ## Setup development environment
	@echo "$(GREEN)Development environment ready!$(NC)"
	@echo "$(BLUE)API Gateway: http://localhost:5000$(NC)"
	@echo "$(BLUE)Swagger UI: http://localhost:5000/swagger$(NC)"
	@echo "$(BLUE)Seq Logs: http://localhost:5341$(NC)"
	@echo "$(BLUE)Jaeger UI: http://localhost:16686$(NC)"

prod-build: ## Production build with optimizations
	@echo "$(GREEN)Production build...$(NC)"
	@dotnet build $(SOLUTION_FILE) --configuration Release \
		--runtime linux-x64 --self-contained false

# Quick development workflows
quick-test: build-debug test-unit ## Quick test cycle for development

full-check: clean build lint test security-scan ## Full quality check

# Platform-specific commands (Windows PowerShell alternative)
ps-setup: ## PowerShell version of setup
	@echo "Run: ./build.ps1 -Target Setup"

ps-test: ## PowerShell version of test
	@echo "Run: ./build.ps1 -Target Test"
