# UgnayDesktop

Windows Forms desktop application for login, dashboards, UDP glove telemetry, and Twilio notifications.

## Requirements

- Windows 10/11
- .NET SDK 10.0.103 (or compatible 10.0 feature band)
- Visual Studio with Desktop development with .NET workload

## First-Time Setup

1. Clone the repository.
2. Open the project folder in Visual Studio.
3. Restore/build once.
4. Add your logo file (optional) to:
   - `Assets/Images/UgnayLogo.png`

The app seeds a default admin account on first run:
- Username: `admin`
- Password: `admin123`

## Twilio Environment Variables (Optional)

Set these only if you will use SMS features:

- `TWILIO_ACCOUNT_SID`
- `TWILIO_AUTH_TOKEN`
- `TWILIO_FROM_PHONE_NUMBER`

## Build and Run (CLI)

```powershell
dotnet restore --configfile .\NuGet.Config
dotnet build --configfile .\NuGet.Config
dotnet run --project .\UgnayDesktop.csproj
```

## Notes for Contributors

- Do not commit generated folders: `bin/`, `obj/`, `publish/`, `.dotnet/`, `.nuget/`.
- Do not commit local database files (`*.db`).
- Assets under `Assets/Images` are copied to output automatically.

