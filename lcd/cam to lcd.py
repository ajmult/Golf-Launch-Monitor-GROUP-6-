import picamera
import time

with picamera.PiCamera(stereo_mode='side-by-side', stereo_decimate=False) as camera:
    # Set resolution to match your 2-inch LCD (e.g., 320x240)
    camera.resolution = (320, 240)
    camera.framerate = 30
    
    # Start the preview; FBCP will automatically mirror this to the LCD
    camera.start_preview()
    
    try:
        while True:
            time.sleep(1) # Keep the script running for a constant feed
    except KeyboardInterrupt:
        camera.stop_preview()