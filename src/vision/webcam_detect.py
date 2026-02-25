from ultralytics import YOLO
import cv2

# Load YOLOv8 nano model (downloads automatically first time)
model = YOLO(r"C:\Users\conor\runs\detect\train\weights\best.pt")
cap = cv2.VideoCapture(0)

# Open webcam
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    exit()

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Run detection
    results = model(frame, conf=0.6)

    boxes = results[0].boxes

    if boxes is not None and len(boxes) > 0:
        # Get box with highest conf
        best_box = boxes[boxes.conf.argmax()]

        # Draw only that box
        annotated_frame = results[0].plot(boxes=[best_box])
    else:
        annotated_frame = frame

    # Draw on frame
    annotated_frame = results[0].plot()

    cv2.imshow("YOLOv8 Webcam", annotated_frame)

    if cv2.waitKey(1) & 0xFF == ord("q"):
        break

cap.release()
cv2.destroyAllWindows()
