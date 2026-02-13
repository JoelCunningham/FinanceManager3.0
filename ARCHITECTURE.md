# Architecture Documentation

## Overview

Finance Manager 3.0 follows **Clean Architecture** principles to maintain separation of concerns, testability, and maintainability. The application is structured in four distinct layers, each with specific responsibilities and dependencies flowing inward toward the domain.

## Architectural Principles

1. **Dependency Rule**: Dependencies point inward. Inner layers know nothing about outer layers.
2. **Domain-Centric**: The domain layer is at the core and contains no external dependencies.
3. **Separation of Concerns**: Each layer has a specific responsibility and encapsulates its implementation details.
4. **Testability**: Business logic is isolated and can be tested independently of infrastructure concerns.

## Layer Structure

```
Presentation   ─┐
                ├──> Application ───> Domain
Infrastructure ─┘
```

### Domain Layer (FinanceManager.Domain)

**Purpose**: Contains core business entities and domain logic with no external dependencies.

#### Responsibilities
- Define core business entities
- Enforce business rules and invariants
- No knowledge of persistence, UI, or external services

#### Dependencies
This layer is completely independent

### Application Layer (FinanceManager.Application)

**Purpose**: Contains business logic, orchestrates workflows, and defines interfaces for external dependencies.

#### Responsibilities
- Implement use cases and business workflows
- Define repository and service interfaces
- Coordinate between domain entities
- Transform data between layers using DTOs

#### Dependencies
- FinanceManager.Domain

### Infrastructure Layer (FinanceManager.Infrastructure)

**Purpose**: Implements interfaces defined in the Application layer for data access, external services, and cross-cutting concerns.

#### Responsibilities
- Implement data persistence (currently in-memory)
- Parse transaction files from various banks
- Integrate with external services
- Handle infrastructure-specific concerns

#### Dependencies
- FinanceManager.Application
- FinanceManager.Domain

### Presentation Layer (FinanceManager.WebApp)

**Purpose**: Blazor Server application providing the user interface.

#### Responsibilities
- Render UI components
- Handle user interactions
- Manage presentation state
- Route user requests

#### Dependencies
- FinanceManager.Application
- FinanceManager.Domain

## Design Patterns

### Repository Pattern
Abstracts data access behind interfaces, allowing the application layer to remain independent of data storage implementation.

### Unit of Work Pattern
Coordinates multiple repository operations into a single transaction to maintain data consistency.

### Service Layer Pattern
Encapsulates business logic and orchestrates operations across multiple repositories.

### DTO Pattern
Transfers data between layers without exposing domain entities to the presentation layer.

### Strategy Pattern
Parser implementations allow different parsing strategies based on bank file format.

### Dependency Injection
All dependencies are injected through constructors, promoting loose coupling and testability.

## Best Practices
1. **Keep Domain Pure**: No external dependencies in the domain layer
2. **Interface-Driven Development**: Define contracts in application layer
3. **Dependency Injection**: Use DI for all dependencies
4. **Single Responsibility**: Each class has one reason to change
5. **Immutability**: Prefer immutable DTOs where possible
6. **Async/Await**: Use async operations for I/O-bound work
7. **Proper Error Handling**: Handle exceptions at appropriate layers

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft .NET Application Architecture](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
- [Blazor Server Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
