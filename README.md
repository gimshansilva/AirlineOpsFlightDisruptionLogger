# AirlineOpsFlightDisruptionLogger
# Sri Lankan Airlines Ltd - Flight Disruption Logger

A single-page ASP.NET Core 8 MVC module for logging flight disruptions and immediately viewing the latest operational events in a table for Sri Lankan Airlines Ltd.

## Assignment scope

The module implements the supplied requirements:

- ASP.NET Core 8 MVC
- Dapper (micro-ORM) with Microsoft.Data.SqlClient for data access
- Microsoft SQL Server LocalDB
- Database: `AirlineOpsDB`
- Lookup table: `DisruptionTypes`
- Transaction table: `DisruptionLogs`
- 5 seeded disruption categories
- Flight number validation
- Positive integer delay validation
- Optional remarks with a 500-character limit
- Passenger-care checkboxes for hotel and meal vouchers
- Latest disruptions shown first
- Human-readable disruption name loaded with a Dapper JOIN query
- Yes/No visual indicators for passenger-care fields

## Prerequisites on Windows

1. .NET 8 SDK
2. SQL Server Express LocalDB (normally installed with Visual Studio)
3. Visual Studio 2022 or VS Code

Verify .NET:

```powershell
dotnet --version
```

The target framework is `net8.0`.

## Run the application

From the project directory:

```powershell
dotnet restore
dotnet run
```

At first startup the application (via `Data/DatabaseInitializer.cs`, using Dapper) creates `AirlineOpsDB` and its tables if they do not exist, and seeds the five lookup values when the lookup table is empty.

The expected LocalDB connection is:

```text
Server=(localdb)\\MSSQLLocalDB;Database=AirlineOpsDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

A manual SQL script is also included at:

```text
Database/01_Create_AirlineOpsDB.sql
```

Run that script in SQL Server Management Studio or Azure Data Studio only if you prefer to provision the database manually before launching the application.

## Main project structure

```text
AirlineOpsFlightDisruptionLogger/
├── Controllers/
│   └── DisruptionsController.cs
├── Data/
│   └── DatabaseInitializer.cs
├── Database/
│   └── 01_Create_AirlineOpsDB.sql
├── Models/
│   ├── DisruptionLog.cs
│   └── DisruptionType.cs
├── ViewModels/
│   ├── DisruptionLogInputViewModel.cs
│   └── FlightDisruptionPageViewModel.cs
├── Views/
│   ├── Disruptions/Index.cshtml
│   └── Shared/_Layout.cshtml
├── wwwroot/css/site.css
├── Program.cs
├── appsettings.json
└── AirlineOpsFlightDisruptionLogger.csproj
```

## Functional test checklist

### Valid submission

- Flight number: `UL302`
- Disruption: `Technical Delay`
- Delay: `45`
- Hotel: Yes
- Meal voucher: Yes
- Remarks: `Engineering inspection completed`

Expected result: the record is saved and appears at the top of the table.

### Validation checks

- Empty flight number → validation error
- Invalid flight number such as `302` → validation error
- Delay `0` or negative → validation error
- Missing disruption reason → validation error
- Remarks longer than 500 characters → validation error

### Database checks

Confirm these objects exist in `AirlineOpsDB`:

```sql
SELECT * FROM dbo.DisruptionTypes;
SELECT * FROM dbo.DisruptionLogs ORDER BY LoggedAtUtc DESC, Id DESC;
```

## Git submission

Create the repository and push the completed implementation:

```powershell
git init
git add .
git commit -m "Implement flight disruption logger"
git branch -M main
git remote add origin https://github.com/<your-username>/<your-repository>.git
git push -u origin main
```

