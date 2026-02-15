# Style Guide

This document defines the coding standards and conventions for the Finance Manager 3.0 project.

## Table of Contents

- [General Principles](#general-principles)
- [Naming Conventions](#naming-conventions)
- [C# Language Features](#c-language-features)
- [File Structures](#file-structures)
- [Misc](#misc)

## General Principles

1. **Follow Clean Architecture**: Respect layer boundaries and dependency rules
2. **SOLID Principles**: Write maintainable, extensible code
3. **DRY (Don't Repeat Yourself)**: Extract common functionality
4. **KISS (Keep It Simple)**: Favor simplicity over cleverness
5. **Consistency**: Follow existing patterns in the codebase

## Naming Conventions
| Element                          | Convention                           | Example                           |
| -------------------------------- | ------------------------------------ | --------------------------------- |
| Classes                          | PascalCase                           | `Transaction`, `ImportService`    |
| Interfaces                       | PascalCase <br/> `I` prefix          | `ITransactionRepository`          |
| Properties                       | PascalCase                           | `BankName`, `Amount`              |
| Boolean properties               | PascalCase <br/> `Is/Has/Can` prefix | `IsReviewed`, `HasErrors`         |
| Methods                          | PascalCase                           | `GetParser()`                     |
| Async methods                    | PascalCase <br/> `Async` suffix      | `SaveAsync()`                     |
| Method parameters                | camelCase                            | `transaction`, `bankName`         |
| Primary constructor parameters   | PascalCase                           | `Service(IRepository Repository)` |
| Blazor parameters                | PascalCase                           | `SelectedBank`                    |
| Event callbacks                  | PascalCase <br/> `On` prefix         | `OnFileUploaded`                  |
| CSS classes                      | kebab-case                           | `transaction-list`                |
| HTML ids 					       | camelCase                            | `autoAssign`, `cateogryInput`     |                  |

## C# Language Features

### Primary constructors:
```csharp
public class ImportService(ParserService parserService) { }
```

### Use required properties
```csharp
public sealed class Transaction : IEntity
{
    public required string Description { get; set; }
    public required BankRecord Record { get; set; }
}
```

### Use pattern matching
```csharp
if (Model.SelectedBank is null) return;
```

### Use target-typed new
```csharp
private ImportPageModel Model = new();
```

### Use file-scoped namespaces
```csharp
namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;

public class CategoryService { }
```

### Use var for obvious types

```csharp
var transactions = new List<Transaction>();
var count = results.Count;
```

## File Structures

#### C# Class Structure
```csharp
// 1. File-scoped namespace
namepace FinanceManager.Application.Services;

// 2. Using directives
using FinanceManager.Application.DTOs;

// 3. Class declaration with primary constructor
public class TransactionService(ITransactionRepository transactionRepository)
{
    // 4. Private fields
    private readonly ITransactionRepository Repository;
    
    // 5. Public properties
    public int Count { get; private set; }
    
    // 6. Public methods
    public async Task<Transaction> GetByIdAsync(Guid id)
    {
        return await Repository.GetByIdAsync(id);
    }
    
    // 7. Private methods
    private void ValidateTransaction(Transaction transaction)
    {
        // Validation logic
    }
}
```

#### Blazor Component Structure
```razor
@* 1. Page directive (if applicable) *@
@page "/Import"

@* 2. Render mode *@
@rendermode InteractiveServer

@* 3. Dependency injection *@
@inject ImportService ImportService
@inject NavigationManager Navigation

@* 4. Code block *@
@code {
    @* 5. Parameters *@
    [Parameter] public Bank? SelectedBank { get; set; }

    @* 6. Private fields and properties *@
    private ImportPageModel Model = new();
    
    @* 7. Lifecycle methods *@
    protected override async Task OnInitializedAsync()
    {
        // Initialization logic
    }

    @* 8. Event handlers and other methods *@
    private async Task SaveAsync()
    {
        // Implementation
    }
}

@* 9. Markup *@
<PageTitle>Import Transactions</PageTitle>

<h1 class="page-heading">Import transactions</h1>

@if (Model.SelectedBank is not null)
{
    <BankSelectList SelectedBank="Model.SelectedBank" />
}
```

## Misc

### Blazor

#### Use [Parameter] inline with parameters
```razor
[Parameter] public Bank? SelectedBank { get; set; }
```

#### Do not use [EditorRequired] or required parameters in Blazor components
```razor
[Parameter, EditorRequired] public requied Bank? SelectedBank { get; set; }
```

### CSS

#### Nest styles
```css
.modal-body { 
    .modal-content { }
    .modal-header { }
}
```