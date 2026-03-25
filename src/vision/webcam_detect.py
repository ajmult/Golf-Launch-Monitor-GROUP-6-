import math
import json
from ultralytics import YOLO
import cv2
import datetime
from deep_sort_realtime.deepsort_tracker import DeepSort

CONFIDENCE_THRESHOLD = 0.6
GREEN = (0, 255, 0)
WHITE = (255, 255, 255)

OUTPUT_FILE = "balldata.json"

# launch angle config
MIN_DISPLACEMENT = 5  # PIXELS
SAMPLES_NEEDED = 5    # frame average

# Load trained YOLOv8 model
model = YOLO(r"C:\Users\conor\runs\detect\train\weights\best.pt")

# Open webcam
cap = cv2.VideoCapture(0)
if not cap.isOpened():
    print("Error: Could not open webcam.")
    exit()

# DeepSort tracker
tracker = DeepSort(max_age=50)

# Stores last known center
track_positions = {}

# launch angle state per tracked object
launch_data = {}

while True:
    start = datetime.datetime.now()

    ret, frame = cap.read()
    if not ret:
        break

    # Run YOLO detection
    detections = model(frame)[0]

    ds_input = []
    for data in detections.boxes.data.tolist():
        confidence = data[4]
        if confidence < CONFIDENCE_THRESHOLD:
            continue

        xmin, ymin, xmax, ymax = int(data[0]), int(data[1]), int(data[2]), int(data[3])
        class_id = int(data[5])

        ds_input.append([[xmin, ymin, xmax - xmin, ymax - ymin], confidence, class_id])

    # Update tracker
    tracks = tracker.update_tracks(ds_input, frame=frame)

    for track in tracks:
        if not track.is_confirmed():
            continue

        track_id = track.track_id
        ltrb = track.to_ltrb()
        xmin, ymin, xmax, ymax = int(ltrb[0]), int(ltrb[1]), int(ltrb[2]), int(ltrb[3])

        # Current center
        cx, cy = (xmin + xmax) // 2, (ymin + ymax) // 2

        # Get previous position BEFORE updating
        if track_id in track_positions:
            prev_cx, prev_cy = track_positions[track_id]
            dx = cx - prev_cx
            dy = cy - prev_cy
            displacement = (dx**2 + dy**2) ** 0.5
        else:
            prev_cx, prev_cy = cx, cy
            dx, dy, displacement = 0, 0, 0.0

        # Launch angle tracking
        if track_id not in launch_data:
            launch_data[track_id] = {"samples": [], "angle": None}

        ld = launch_data[track_id]

        if ld["angle"] is None and displacement >= MIN_DISPLACEMENT:
            ld["samples"].append((dx, dy))

        if ld["angle"] is None and len(ld["samples"]) >= SAMPLES_NEEDED:
            avg_dx = sum(s[0] for s in ld["samples"]) / len(ld["samples"])
            avg_dy = sum(s[1] for s in ld["samples"]) / len(ld["samples"])

            ld["angle"] = math.degrees(math.atan2(-avg_dy, avg_dx))

        launch_angle = ld["angle"]

        # JSON Serialization for Unity
        ball_data = {
            "pos1": [prev_cx, prev_cy],
            "pos2": [cx, cy],
            "speed": displacement,
            "size1": xmax - xmin,
            "size2": ymax - ymin
        }

        with open(OUTPUT_FILE, "w") as f:
            json.dump(ball_data, f)

        # Update stored position AFTER serialization
        track_positions[track_id] = (cx, cy)

        # Drawing
        cv2.rectangle(frame, (xmin, ymin), (xmax, ymax), GREEN, 2)

        cv2.rectangle(frame, (xmin, ymin - 20), (xmin + 40, ymin), GREEN, -1)
        cv2.putText(frame, str(track_id), (xmin + 4, ymin - 6),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, WHITE, 2)

        cv2.circle(frame, (cx, cy), 4, (0, 0, 255), -1)

        cv2.putText(frame, f"d={displacement:.1f}px",
                    (xmin, ymax + 18),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, GREEN, 2)

        if launch_angle is not None:
            cv2.putText(frame, f"LA={launch_angle:.1f}deg",
                        (xmin, ymax + 36),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 165, 0), 2)

    # FPS
    elapsed = (datetime.datetime.now() - start).total_seconds()
    fps_text = f"FPS: {1 / elapsed:.1f}" if elapsed > 0 else "FPS: --"
    cv2.putText(frame, fps_text, (10, 30),
                cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 0, 255), 2)

    cv2.imshow("Golf Ball Tracker", frame)

    if cv2.waitKey(1) & 0xFF == ord("q"):
        break

cap.release()
cv2.destroyAllWindows()
