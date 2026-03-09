# UgnayDesktop

Windows Forms desktop application for login, dashboards, MQTT integration, and Twilio notifications.

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

## MFA (Email Verification)

Login now supports MFA-ready email verification for `Admin` and `Teacher` accounts **when an email is configured**.

Current behavior:
- A 6-digit code is generated at login and expires in 5 minutes.
- Until an email provider is selected, the app uses a local sender that writes codes to `mfa-email-preview.log` in the app folder.
- Accounts without an email still use password-only login for now, so rollout can be done gradually.

To plug in a real provider later:
- Implement `Services/IEmailVerificationSender`.
- Replace `LocalEmailVerificationSender` in `Forms/LoginForm.cs` with your provider implementation.

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

## Diagnostics Logging

The app writes runtime diagnostics to daily log files in the app output folder:

- `logs/ugnay-YYYYMMDD.log`

Current log coverage includes MQTT connection/subscription events and telemetry parsing errors.
## Notes for Contributors

- Do not commit generated folders: `bin/`, `obj/`, `publish/`, `.dotnet/`, `.nuget/`.
- Do not commit local database files (`*.db`).
- Assets under `Assets/Images` are copied to output automatically.



