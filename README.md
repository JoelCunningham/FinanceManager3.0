<div align="center">

# 💰 Finance Manager 3.0

**Your Personal Finance Companion**

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?logo=blazor)](https://blazor.net/)

_A modern finance management application built with Blazor Interactive Server Components that helps you import, categorize, and manage financial transactions from multiple banks._

[Features](#-features) • [Getting Started](#-getting-started) • [Architecture](#-architecture) • [Documentation](#-documentation)

</div>

---

## ✨ Features

<table>
<tr>
<td width="50%">

### 📥 Transaction Import

Import transactions from multiple bank file formats including Westpac and Vanguard.

### 📊 Transaction Management

View, filter, and categorize your imported transactions with ease.

</td>
<td width="50%">

### 🏷️ Category Management

Organize transactions with custom categories and category groups.

### 🔄 Transfers & Reimbursements

Track inter-account transfers and manage reimbursable expenses.

</td>
</tr>
</table>

### 🎨 Modern UI

Built with **Blazor Interactive Server Components** for a responsive, interactive experience with real-time updates.

---

## 🛠️ Tech Stack

| Technology                                                                                               | Description                         |
| -------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| ![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)                     | Latest .NET framework with C# 14.0  |
| ![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?style=flat-square&logo=blazor) | Interactive server-side rendering   |
| ![Architecture](https://img.shields.io/badge/Architecture-Clean-blue?style=flat-square)                  | Separated into four distinct layers |

---

## 📁 Project Structure

```
📦 FinanceManager
├── 🏛️ FinanceManager.Domain          # Core business entities and domain logic
├── 💼 FinanceManager.Application     # Business logic and services
├── 🔧 FinanceManager.Infrastructure  # Data access, parsers, and external integrations
└── 🌐 FinanceManager.WebApp          # Blazor Interactive Server UI application
```

---

## 🚀 Getting Started

### Prerequisites

- ✅ [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- ✅ [Visual Studio 2062](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) (recommended)

### 📦 Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/JoelCunningham/FinanceManager3.0.git
   cd FinanceManager3.0
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Setup secrets**
	```bash
   dotnet user-secrets init --project FinanceManager.WebApp
   dotnet user-secrets set "Email:Smpt:Password" "your_password"
   ```

4. **Run the application**

   ```bash
   cd FinanceManager.WebApp
   dotnet run
   ```

5. **Open in browser** 🌐

   Navigate to `https://localhost:7292` (or the URL shown in the console)

### 🗄️ EF Core (SQLite)

This app uses EF Core with SQLite. The WebApp expects a connection string in one of these settings:

- `ConnectionStrings__Default`
- `FINANCEMANAGER__CONNECTIONSTRING`

Example (PowerShell):

```ps
$env:ConnectionStrings__Default="Data Source=finance_manager.db"
```

#### Migrations

After changing the model, create and apply a migration:

```ps
dotnet tool run dotnet-ef migrations add AddYourChange --project FinanceManager.Infrastructure --startup-project FinanceManager.WebApp
dotnet tool run dotnet-ef database update --project FinanceManager.Infrastructure --startup-project FinanceManager.WebApp
```

---

## 🏗️ Architecture

The application follows **Clean Architecture** principles with four distinct layers:

```
┌─────────────────────────────────────────┐
│         🌐 Presentation Layer           │  Blazor Interactive Server UI
├─────────────────────────────────────────┤
│        🔧 Infrastructure Layer          │  Data Access & External Services
├─────────────────────────────────────────┤
│          💼 Application Layer           │  Business Logic & Use Cases
├─────────────────────────────────────────┤
│             🏛️ Domain Layer             │  Core Business Entities
└─────────────────────────────────────────┘
```

```
FinanceManager
├── FinanceManager.Domain
├── FinanceManager.Application
├── FinanceManager.Infrastructure
└── FinanceManager.WebApp
```

Dependencies flow **inward** toward the domain, ensuring core business logic remains independent of external concerns.

> 📚 **For detailed documentation**, see [ARCHITECTURE.md](ARCHITECTURE.md)

---

## 📚 Documentation

| Document                                 | Description                               |
| ---------------------------------------- | ----------------------------------------- |
| [📖 Architecture Guide](ARCHITECTURE.md) | Detailed architecture and design patterns |
| [📝 Style Guide](STYLEGUIDE.md)          | Coding standards and conventions          |

---

<div align="center">

**Built by Joel Cunningham**

[⬆ Back to Top](#-finance-manager-30)

</div>
