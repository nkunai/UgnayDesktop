# GestureCollector

C# dataset collector for gesture training with:

- MediaPipe 21-point hand landmarks (left + right)
- ESP32 UDP telemetry over Wi-Fi from two gloves
- CSV output for model training

## Data format

Each CSV row:

- label
- left hand landmarks: `left_0_x,left_0_y ... left_20_x,left_20_y`
- right hand landmarks: `right_0_x,right_0_y ... right_20_x,right_20_y`
- left glove: `left_roll,left_pitch,left_yaw`
- right glove: `right_roll,right_pitch,right_yaw`
- timestamp_unix_ms

## Requirements

- .NET SDK (same major used in repo)
- Python 3 with:
  - `mediapipe`
  - `opencv-python`

Install python deps:

```powershell
pip install mediapipe opencv-python
```

## Run

```powershell
cd C:\Users\Marissa Macatangay\Documents\GitHub\UgnayDesktop\GestureCollector
$env:DOTNET_CLI_HOME='C:\Users\Marissa Macatangay\Documents\GitHub\UgnayDesktop\.dotnet'
dotnet run -- --label=hello --out=dataset.csv --port=5005 --camera=1 --leftIp=192.168.1.101 --rightIp=192.168.1.102
```

If `leftIp/rightIp` are omitted, the collector auto-routes by packet shape:

- 3 values -> left glove (`roll,pitch,yaw`)
- 5 values -> right glove (`roll,pitch,yaw,heartRate,temp`) (only roll/pitch/yaw is saved)

## Controls
- startup does automatic calibration first (100 samples each hand)
- `R` recalibrate anytime
- `SPACE` record sample
- `ESC` stop

## Notes

- The Python worker opens the camera window (`mediapipe_hands`).
- Keep both gloves and both hands visible for best data quality.
