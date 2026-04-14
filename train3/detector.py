import math
import json
import socket
import threading
import cv2


class Config:
    MODEL_PATH = "best.onnx"

    CONFIDENCE_THRESHOLD = 0.7

    JSON_FILE = "balldata.json"

    USE_TCP_BRIDGE = True
    TCP_HOST = "192.168.20.147"
    TCP_PORT = 5555


class CameraSource:

    def __init__(self):
        from picamera2 import Picamera2

        self.cam = Picamera2()

        config = self.cam.create_video_configuration(
            main={"size": (1280, 720), "format": "BGR888"}
        )

        self.cam.configure(config)
        self.cam.start()

        print("[Camera] Picamera2 opened")

    def read(self):
        return True, self.cam.capture_array()


class CSharpBridge:

    def __init__(self, host, port):
        self.sock = None

        try:
            self.sock = socket.create_connection((host, port), timeout=1)
            print(f"[Bridge] Connected to Unity on {host}:{port}")
        except:
            print("[Bridge] Unity not running — JSON only")

    def send(self, data):
        if not self.sock:
            return

        try:
            self.sock.sendall((json.dumps(data) + "\n").encode())
        except:
            self.sock = None


def run():

    cfg = Config()

    cam = CameraSource()
    net = cv2.dnn.readNet(cfg.MODEL_PATH)

    bridge = CSharpBridge(cfg.TCP_HOST, cfg.TCP_PORT)

    prev_pos = None
    detection_streak = 0
    shot_active = False

    print("[Golf Monitor] Running")

    while True:

        ok, frame = cam.read()
        if not ok:
            continue

        blob = cv2.dnn.blobFromImage(frame, 1/255, (640,640), swapRB=True)
        net.setInput(blob)
        outputs = net.forward()

        best_det = None
        best_conf = 0

        # ───── find best detection ─────
        for det in outputs[0]:

            conf = float(det[4])

            if conf > cfg.CONFIDENCE_THRESHOLD and conf > best_conf:

                cx = int(det[0] * frame.shape[1])
                cy = int(det[1] * frame.shape[0])
                w = int(det[2] * frame.shape[1])
                h = int(det[3] * frame.shape[0])

                if w < 10 or h < 10:
                    continue

                best_conf = conf
                best_det = (cx, cy, w, h)

        # ───── NO detection ─────
        if best_det is None:
            detection_streak = 0
            prev_pos = None
            shot_active = False
            continue

        # ───── CONSISTENCY CHECK ─────
        detection_streak += 1

        # require at least 3 frames of consistent detection
        if detection_streak < 3:
            continue

        cx, cy, w, h = best_det

        if prev_pos is None:
            prev_pos = (cx, cy)
            continue

        prev_cx, prev_cy = prev_pos

        dx = cx - prev_cx
        dy = cy - prev_cy

        displacement = math.sqrt(dx*dx + dy*dy)

        prev_pos = (cx, cy)

        # ignore jitter
        if displacement < 5:
            continue

        # ───── SHOT DETECTION ─────
        if displacement > 15:

            if not shot_active:

                shot_active = True

                angle = math.degrees(math.atan2(-dy, dx))

                ball_data = {
                    "pos1": [prev_cx, prev_cy],
                    "pos2": [cx, cy],
                    "speed": displacement,
                    "size1": w,
                    "size2": h,
                    "track_id": 1,
                    "launch_angle": angle
                }

                with open(cfg.JSON_FILE, "w") as f:
                    json.dump(ball_data, f)

                bridge.send(ball_data)

                print(f"[SHOT] speed={displacement:.1f} angle={angle:.1f}")

        if displacement < 3:
            shot_active = False


if __name__ == "__main__":
    run()