import cv2
import os


def find_newest_snip(dir="./hololens_api/snips"):
    max_mtime = 0
    for dirname,subdirs,files in os.walk(dir):
        for fname in files:
            full_path = os.path.join(dirname, fname)
            mtime = os.stat(full_path).st_mtime
            if mtime > max_mtime:
                max_mtime = mtime
                max_dir = dirname
                max_file = fname
    return os.path.join(max_dir, max_file)

def get_last_frame(file_name):
    cap = cv2.VideoCapture(file_name)

    # Read the last frame
    grabbing = True
    i = 0
    while grabbing:
        i += 1
        grabbing = cap.grab()

    cap.set(cv2.CAP_PROP_POS_FRAMES, i-1)
    ret, last_frame = cap.retrieve()
    cap.release()
    # Check if the frame is read successfully
    if not ret:
        print("Error: Failed to read last frame")
        return None
    else:
        return last_frame        
