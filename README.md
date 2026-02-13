# Finance Manager 3.0

A personal finance management application built with Blazor Server that helps you import, categorize, and manage financial transactions from multiple banks.

## Features

- **Transaction Import**: Import transactions from multiple bank file formats
  - Westpac
  - Vanguard
- **Transaction Management**: View, categorize, and manage imported transactions
- **Category Management**: Organize transactions with custom categories
- **Transfers**: Track transfers between accounts
- **Reimbursements**: Manage reimbursable expenses
- **Interactive UI**: Built with Blazor Server for a responsive, interactive experience

## Tech Stack

- **.NET 10** with C# 14.0
- **Blazor Server**: Interactive server-side rendering
- **Clean Architecture**: Separated into Domain, Application, Infrastructure, and WebApp layers

## Project Structure

```
FinanceManager
├── FinanceManager.Domain          # Core business entities and domain logic
├── FinanceManager.Application     # Business logic and services
├── FinanceManager.Infrastructure  # Data access, parsers, and external integrations
└── FinanceManager.WebApp          # Blazor Server UI application
```

## Getting Started

### Prerequisites

- .NET 10 SDK or later
- A code editor (Visual Studio 2026 recommended)

### Running the Application

1. Clone the repository:
   ```bash
   git clone https://github.com/JoelCunningham/FinanceManager3.0.git
   cd FinanceManager3.0
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   cd FinanceManager.WebApp
   dotnet run
   ```

4. Open your browser and navigate to `https://localhost:5001` (or the URL shown in the console)

## Architecture

The application follows **Clean Architecture** principles with four distinct layers:

- **Domain**: Core business entities and domain logic
- **Application**: Business logic, services, and interfaces
- **Infrastructure**: Data access, file parsers, and external integrations
- **WebApp**: Blazor Server UI and components

Dependencies flow inward toward the domain, ensuring the core business logic remains independent of external concerns.

> 📚 For detailed architecture documentation, see [ARCHITECTURE.md](ARCHITECTURE.md)

**Note**: Currently uses in-memory storage. Database implementation planned for future releases.

## Contributing

This is a personal project, but suggestions and improvements are welcome! Feel free to open an issue or submit a pull request.

## License

This project is licensed under the terms specified in the repository.
