import argparse
import json
import cv2
import mediapipe as mp
import sys
import threading
import queue


# MediaPipe pose indices (BlazePose)
L_SHOULDER = 11
R_SHOULDER = 12
L_ELBOW = 13
R_ELBOW = 14
L_WRIST = 15
R_WRIST = 16


# Stabilization params
SMOOTH_ALPHA = 0.35     # lower = smoother/stabler, higher = more responsive
HAND_HOLD_FRAMES = 6    # hold last hand for brief detection drops


def flatten_landmarks(hand_landmarks):
    vals = []
    for lm in hand_landmarks.landmark:
        vals.append(float(lm.x))
        vals.append(float(lm.y))
    return vals


def smooth_series(prev_vals, curr_vals, alpha=SMOOTH_ALPHA):
    if prev_vals is None:
        return curr_vals
    return [(alpha * c) + ((1.0 - alpha) * p) for p, c in zip(prev_vals, curr_vals)]


def xy_from_pose(pose_landmarks, idx):
    lm = pose_landmarks.landmark[idx]
    return [float(lm.x), float(lm.y)]


def draw_upper_body(frame, pose_landmarks):
    h, w, _ = frame.shape

    points = {
        "ls": pose_landmarks.landmark[L_SHOULDER],
        "rs": pose_landmarks.landmark[R_SHOULDER],
        "le": pose_landmarks.landmark[L_ELBOW],
        "re": pose_landmarks.landmark[R_ELBOW],
        "lw": pose_landmarks.landmark[L_WRIST],
        "rw": pose_landmarks.landmark[R_WRIST],
    }

    def px(name):
        p = points[name]
        return int(p.x * w), int(p.y * h)

    cv2.line(frame, px("ls"), px("rs"), (0, 255, 255), 2)
    cv2.line(frame, px("ls"), px("le"), (255, 200, 0), 2)
    cv2.line(frame, px("le"), px("lw"), (255, 200, 0), 2)
    cv2.line(frame, px("rs"), px("re"), (0, 200, 255), 2)
    cv2.line(frame, px("re"), px("rw"), (0, 200, 255), 2)

    for name in points:
        cv2.circle(frame, px(name), 4, (50, 255, 50), -1)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--camera", type=int, default=0)
    args = parser.parse_args()

    mp_holistic = mp.solutions.holistic
    holistic = mp_holistic.Holistic(
        static_image_mode=False,
        model_complexity=1,
        smooth_landmarks=True,
        min_detection_confidence=0.55,
        min_tracking_confidence=0.70,
    )

    cap = cv2.VideoCapture(args.camera, cv2.CAP_DSHOW)
    cap.set(cv2.CAP_PROP_FOURCC, cv2.VideoWriter_fourcc(*"MJPG"))
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)
    cap.set(cv2.CAP_PROP_FPS, 30)

    if not cap.isOpened():
        raise RuntimeError("Camera failed to open")

    show_pose_markers = True

    # command queue from collector stdin IPC
    cmd_q = queue.Queue()

    def stdin_reader():
        while True:
            line = sys.stdin.readline()
            if not line:
                break
            cmd_q.put(line.strip().upper())

    threading.Thread(target=stdin_reader, daemon=True).start()

    cached_left = None
    cached_right = None
    left_hold = 0
    right_hold = 0

    while True:
        # apply pending commands from collector
        while not cmd_q.empty():
            cmd = cmd_q.get_nowait()
            if cmd == "POSE_ON":
                show_pose_markers = True
            elif cmd == "POSE_OFF":
                show_pose_markers = False

        ok, frame = cap.read()
        if not ok:
            continue

        frame = cv2.flip(frame, 1)
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        result = holistic.process(rgb)

        out = {
            "left": None,
            "right": None,
            "pose_upper": None,
            "left_held": False,
            "right_held": False,
        }

        detected_left = None
        detected_right = None

        if result.left_hand_landmarks is not None:
            detected_right = flatten_landmarks(result.left_hand_landmarks)
            mp.solutions.drawing_utils.draw_landmarks(frame, result.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS)

        if result.right_hand_landmarks is not None:
            detected_left = flatten_landmarks(result.right_hand_landmarks)
            mp.solutions.drawing_utils.draw_landmarks(frame, result.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS)

        if detected_left is not None:
            cached_left = smooth_series(cached_left, detected_left)
            left_hold = HAND_HOLD_FRAMES
        elif cached_left is not None and left_hold > 0:
            left_hold -= 1
            out["left_held"] = True
        else:
            cached_left = None

        if detected_right is not None:
            cached_right = smooth_series(cached_right, detected_right)
            right_hold = HAND_HOLD_FRAMES
        elif cached_right is not None and right_hold > 0:
            right_hold -= 1
            out["right_held"] = True
        else:
            cached_right = None

        out["left"] = cached_left
        out["right"] = cached_right

        if result.pose_landmarks is not None:
            out["pose_upper"] = {
                "left_shoulder": xy_from_pose(result.pose_landmarks, R_SHOULDER),
                "left_elbow": xy_from_pose(result.pose_landmarks, R_ELBOW),
                "left_wrist": xy_from_pose(result.pose_landmarks, R_WRIST),
                "right_shoulder": xy_from_pose(result.pose_landmarks, L_SHOULDER),
                "right_elbow": xy_from_pose(result.pose_landmarks, L_ELBOW),
                "right_wrist": xy_from_pose(result.pose_landmarks, L_WRIST),
            }
            if show_pose_markers:
                draw_upper_body(frame, result.pose_landmarks)

        pose_state = "ON" if show_pose_markers else "OFF"
        cv2.putText(frame, f"Arms/Shoulders: {pose_state} (press A to toggle)", (20, 30),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 255), 2, cv2.LINE_AA)

        print(json.dumps(out), flush=True)

        cv2.imshow("mediapipe_hands", frame)
        key = cv2.waitKey(1) & 0xFF
        if key == 27:
            break
        if key in (ord("a"), ord("A")):
            show_pose_markers = not show_pose_markers

    cap.release()
    cv2.destroyAllWindows()


if __name__ == "__main__":
    main()
