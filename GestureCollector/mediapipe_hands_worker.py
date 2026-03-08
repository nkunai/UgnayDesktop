import argparse
import json
import cv2
import mediapipe as mp


def flatten_landmarks(hand_landmarks):
    vals = []
    for lm in hand_landmarks.landmark:
        vals.append(float(lm.x))
        vals.append(float(lm.y))
    return vals


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--camera", type=int, default=0)
    args = parser.parse_args()

    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(
        static_image_mode=False,
        max_num_hands=2,
        min_detection_confidence=0.3,
        min_tracking_confidence=0.3,
        model_complexity=1,
    )

    draw = mp.solutions.drawing_utils
    cap = cv2.VideoCapture(args.camera, cv2.CAP_DSHOW)
    cap.set(cv2.CAP_PROP_FOURCC, cv2.VideoWriter_fourcc(*"MJPG"))
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)
    cap.set(cv2.CAP_PROP_FPS, 30)

    if not cap.isOpened():
        raise RuntimeError("Camera failed to open")

    while True:
        ok, frame = cap.read()
        if not ok:
            continue

        frame = cv2.flip(frame, 1)
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        result = hands.process(rgb)

        out = {"left": None, "right": None}

        if result.multi_hand_landmarks and result.multi_handedness:
            for i, hand in enumerate(result.multi_hand_landmarks):
                label = result.multi_handedness[i].classification[0].label.lower()
                vals = flatten_landmarks(hand)

                if label == "left":
                    out["left"] = vals
                elif label == "right":
                    out["right"] = vals

                draw.draw_landmarks(frame, hand, mp_hands.HAND_CONNECTIONS)

        print(json.dumps(out), flush=True)

        cv2.imshow("mediapipe_hands", frame)
        key = cv2.waitKey(1) & 0xFF
        if key == 27:
            break

    cap.release()
    cv2.destroyAllWindows()


if __name__ == "__main__":
    main()