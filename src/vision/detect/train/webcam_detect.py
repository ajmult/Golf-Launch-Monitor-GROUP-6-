from ultralytics import YOLO
import cv2
import datetime
from deep_sort_realtime.deepsort_tracker import DeepSort

CONFIDENCE_THRESHOLD = 0.6
GREEN = (0, 255, 0)
WHITE = (255, 255, 255)

# Load trained YOLOv8 model
model = YOLO(r"C:\Users\conor\runs\detect\train\weights\best.pt")

# Open webcam
cap = cv2.VideoCapture(0)
if not cap.isOpened():
    print("Error: Could not open webcam.")
    exit()

# DeepSort tracker — max_age is how many frames a track survives without a detection
tracker = DeepSort(max_age=50)

# Stores the last known centre position for each tracked object
track_positions = {}

while True:
    start = datetime.datetime.now()

    ret, frame = cap.read()
    if not ret:
        break

    # Run YOLO detection on the current frame
    detections = model(frame)[0]

    # Build the list of detections DeepSort expects: [[x, y, w, h], confidence, class_id]
    ds_input = []
    for data in detections.boxes.data.tolist():
        confidence = data[4]
        if confidence < CONFIDENCE_THRESHOLD:
            continue
        xmin, ymin, xmax, ymax = int(data[0]), int(data[1]), int(data[2]), int(data[3])
        class_id = int(data[5])
        ds_input.append([[xmin, ymin, xmax - xmin, ymax - ymin], confidence, class_id])

    # Update tracker with current detections — returns confirmed tracks
    tracks = tracker.update_tracks(ds_input, frame=frame)

    for track in tracks:
        if not track.is_confirmed():
            continue

        track_id = track.track_id
        ltrb = track.to_ltrb()  # Bounding box as [left, top, right, bottom]
        xmin, ymin, xmax, ymax = int(ltrb[0]), int(ltrb[1]), int(ltrb[2]), int(ltrb[3])

        # Calculate centre of the bounding box
        cx, cy = (xmin + xmax) // 2, (ymin + ymax) // 2

        # Calculate pixel displacement from the previous frame for this track
        if track_id in track_positions:
            prev_cx, prev_cy = track_positions[track_id]
            dx = cx - prev_cx  # Horizontal movement in pixels
            dy = cy - prev_cy  # Vertical movement in pixels
            displacement = (dx**2 + dy**2) ** 0.5
        else:
            dx, dy, displacement = 0, 0, 0.0

        # Store current centre for next frame comparison
        track_positions[track_id] = (cx, cy)

        # These are the values we need to pass as of rn
        # track_id    : unique ID for this object across frames
        # cx, cy      : current centre position in pixels
        # dx, dy      : movement vector since last frame
        # displacement: total pixels moved since last frame

        # Draw bounding box
        cv2.rectangle(frame, (xmin, ymin), (xmax, ymax), GREEN, 2)

        # Draw ID label
        cv2.rectangle(frame, (xmin, ymin - 20), (xmin + 40, ymin), GREEN, -1)
        cv2.putText(frame, str(track_id), (xmin + 4, ymin - 6), cv2.FONT_HERSHEY_SIMPLEX, 0.5, WHITE, 2)

        # Draw centre dot
        cv2.circle(frame, (cx, cy), 4, (0, 0, 255), -1)

        # Show displacement on frame
        cv2.putText(frame, f"d={displacement:.1f}px", (xmin, ymax + 18),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, GREEN, 2)

    # Calculate and display FPS
    elapsed = (datetime.datetime.now() - start).total_seconds()
    fps_text = f"FPS: {1 / elapsed:.1f}" if elapsed > 0 else "FPS: --"
    cv2.putText(frame, fps_text, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 0, 255), 2)

    cv2.imshow("Golf Ball Tracker", frame)

    if cv2.waitKey(1) & 0xFF == ord("q"):
        break

cap.release()
cv2.destroyAllWindows()