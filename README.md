# UgnayDesktop

Windows Forms desktop application for login, dashboards, UDP glove telemetry, and TextBee notifications.

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

Default teacher account for SMS/TextBee testing:
- Full Name: `teachertester`
- Username: `teacher`
- Password: `teacher`
- Teacher Phone: `+639186764468`

## TextBee Environment Variables (Optional)

Set these only if you will use SMS features:

- `TEXTBEE_API_KEY`
- `TEXTBEE_DEVICE_ID`
- `TEXTBEE_API_BASE_URL` (optional, defaults to `https://api.textbee.dev`)

## TextBee Webhook Notes

For incoming SMS/status callbacks, create a webhook in TextBee dashboard and point it to your server endpoint.
Your endpoint should verify `X-Signature` with `HMAC_SHA256(JSON payload, signing secret)`.

## UI Progress Snapshot (March 13, 2026)

This section is a dated snapshot captured during in-progress refactoring.

### Current State (right now)
- The workspace is **not buildable** at the moment.
- Build error: `CS1513 } expected` in `Forms/TeacherDashboard.cs:98`.

### What is finished
1. TextBee migration and setup
- Added TextBee service: `Services/TextBeeService.cs`
- Removed Twilio package from `UgnayDesktop.csproj`
- Updated teacher dashboard actions/messages to TextBee in `Forms/TeacherDashboard.cs`
- Updated button labels in `Forms/TeacherDashboard.Designer.cs`
- Updated docs/env vars in `README.md`

2. Default teacher seed for testing
- Implemented in `Data/DbInitializer.cs`:
  - Full name: `teachertester`
  - Username: `teacher`
  - Password: `teacher`
  - Phone: `+639186764468`

3. Add Student modal
- Added modal form: `Forms/AddStudentDialog.cs`
- `Add Student` button now opens modal (wired in `Forms/TeacherDashboard.cs`)

### What is unfinished / partially done
1. Student cards conversion (your latest request)
- New card engine file was added: `Forms/TeacherDashboard.StudentCards.cs`
- This includes:
  - Full-name card heading
  - Glove-device ID row
  - Recognized gesture
  - Heart rate BPM
  - Sweatness level
  - Temperature
  - Realtime update hooks from sensor listener

- But integration into `Forms/TeacherDashboard.cs` is **incomplete/broken**:
  - `LoadStudents()` method got malformed during replacement.
  - Old/new code blocks are mixed in that file.
  - That is the direct reason the build fails.

2. Stage3 layout hook for cards
- `Forms/TeacherDashboard.GestureStage3.cs` currently has no functional card-layout update yet (only trivial file change).

### Whole UI progress in this thread
1. Twilio UI controls were switched to TextBee labels/actions (`TextBee Link`, `TextBee Test`).
2. Add-student inline form was replaced with a modal workflow.
3. Then we started replacing student grid/list with monitoring cards (your current requirement).
4. You added requirement: "show glove-device ID above recognized gesture."
5. I started implementing that card system and live placeholders, but the turn was interrupted mid-refactor, leaving `TeacherDashboard.cs` inconsistent.

Post-refactor status (March 13, 2026): TeacherDashboard.cs syntax issue (CS1513) is fixed, student-card refactor cleanup is applied, and dotnet build --configfile .\\NuGet.Config now succeeds.

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

