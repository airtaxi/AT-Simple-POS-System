# Copilot Instructions — AT Simple POS System

> **Maintenance rule:** If the project structure documented in this file changes (pages, managers, data types, etc.), update this file to reflect the current state.

## Build

```powershell
dotnet build App/App.csproj
```

Uno Single Project — one csproj targets all six platforms (Android, iOS, macOS Catalyst, Windows/WinUI, WebAssembly, Desktop/Skia). The Uno SDK version is pinned in `global.json`.

No test projects exist in this solution.

## Architecture

**Uno Platform cross-platform POS app** using MVVM with static service managers and local JSON file persistence (no database).

### Data flow

```
Pages (XAML + code-behind)
  └─ ViewModels (CommunityToolkit.Mvvm ObservableObject)
       └─ Static Managers (ItemManager, TransactionManager, RecordManager)
            └─ Configuration (thread-safe JSON read/write → %LOCALAPPDATA%\AT POS\settings.json)
```

- **ItemManager / TransactionManager / SellerManager** — static classes providing CRUD over items, transactions, and sellers via `Configuration`.
- **RecordManager** — computes `Record` objects on-the-fly by grouping transactions by `RecordId`. Records are never persisted directly.
- **SellerManager** — manages seller CRUD. Removing a seller cascades to remove that seller's share entries from all items.
- **Configuration** — single JSON file persistence with an in-memory `Dictionary` cache, a `Lock` for thread safety, and a 50ms debounced write buffer. All reads/writes go through `GetValue<T>` / `SetValue`.

### Navigation

Frame-based navigation through `MainPage.Navigate(Type pageType)` and `MainPage.GoBack()`. `MainPage` hosts a `NavigationView` with a responsive pane (breakpoint at 650px). A global loading overlay is available via `MainPage.ShowLoading()` / `HideLoading()`.

### Pages

| Page | Purpose |
|------|---------|
| `MainPage` | Root shell with NavigationView |
| `Menus/ItemsPage` | POS shopping interface (cart, totals, payment) |
| `Menus/ManagePage` | Item CRUD with image upload and per-item seller share editing |
| `Menus/SellersPage` | Seller CRUD with empty-state UI |
| `Menus/SettingsPage` | App configuration |
| `Menus/ReportPage` | Report menu with tabs for items, records, and sellers |
| `Menus/Report/ItemsReportPage` | Item sales summary with Excel export (ClosedXML) |
| `Menus/Report/RecordsReportPage` | Transaction record listing |
| `Menus/Report/SellersReportPage` | Per-seller revenue breakdown with item detail |

## Conventions

### MVVM pattern

- ViewModels inherit `ObservableObject` from **CommunityToolkit.Mvvm**.
- Properties use the `[ObservableProperty]` source generator attribute with `[NotifyPropertyChangedFor]` for dependents.
- No `ICommand` / `RelayCommand` usage — interaction is event-driven (`QuantityChanged`, `Deleted` events on ViewModels), with handlers in code-behind.

### Data models

- `Item`, `Transaction`, `Record`, `Seller`, `ItemSellerShare` are plain C# classes in `App/DataTypes/`.
- `Transaction.Id`, `Item.Id`, and `Seller.Id` are GUIDs. `Transaction.RecordId` groups transactions into logical records.
- `Item.SalesQuantity` and `Item.IsSoldout` are computed properties (not stored).
- `Item.Shares` holds a list of `ItemSellerShare` (SellerId + Percentage) for multi-seller revenue splitting.
- Item images are stored as `byte[]` (`ImageBinary`).

### Serialization

All JSON serialization uses **System.Text.Json source generation** via `SourceGenerationContext` (AOT/trimming safe). When adding new serializable types, register them with `[JsonSerializable]` on that context.

### Localization

Two languages: English (`Strings/en/`) and Korean (`Strings/ko/`) using `.resw` resource files. Each page has its own `.resw` file. Strings are loaded via `Localization.GetLocalizedString(key)` and language is overridden at startup through `Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride`.

`Constants.cs` exposes localized dialog button labels (Ok, Yes, No, etc.) as static properties.

### Platform-specific code

Platform targets live under `App/Platforms/`. iOS has a custom `FolderSavePickerExtension` in `App/Extensions/` (guarded by `#if __IOS__`). Desktop uses `SkiaHostBuilder`.

### Style

- File-scoped namespaces (`csharp_style_namespace_declarations = file_scoped`).
- `ImplicitUsings` enabled; additional global usings in `GlobalUsings.cs`.
- XAML indentation: 4 spaces, UTF-8 BOM.
- C# indentation: 4 spaces.

## C# Code Style

applyTo: `**/*.cs`

- All comments MUST be written in English.
- Nullable reference types are disabled (`<Nullable>disable</Nullable>`). Do not use `?` annotations or `!` null-forgiving operator.
- Single-line `if`, `for`, `foreach`, `while` statements MUST omit braces: `if (condition) myValue = true;`
- If the line exceeds ~100 characters, break to the next line (still no braces).
- Single-line methods MUST use expression-bodied syntax (`=>`).
- Single-line `try`/`catch`/`finally` blocks each stay on one line: `try { /* code */ } catch (Exception exception) { /* handle */ }`
- Variable names MUST NOT use abbreviations. Use full descriptive names.
- MUST use primary constructors wherever possible.
- MUST use collection expressions (`[item1, item2]`) wherever possible.
- Actively use the latest C# language features and syntax.
