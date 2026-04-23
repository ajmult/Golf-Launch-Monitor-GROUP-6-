from picamera import PiCamera
from time import sleep

camera = PiCamera()
camera.start_preview()
sleep(60) # Preview for 60 seconds
camera.stop_preview()