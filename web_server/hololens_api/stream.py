import numpy as np
import cv2
import requests
from requests.auth import HTTPBasicAuth
import threading
import os
import time 
# from dotenv import load_dotenv
# load_dotenv()

# Suppress ugly ssl error
import urllib3
from requests.packages.urllib3.exceptions import InsecureRequestWarning
# Disable urllib3 warnings globally
urllib3.disable_warnings(InsecureRequestWarning)

hololens_ip = os.getenv('HOLOLENS_IP')
username = os.getenv('HOLOLENS_USERNAME')
password = os.getenv("HOLOLENS_PASSWORD")
live_stream_endpoint = f'http://{username}:{password}@{hololens_ip}/api/holographic/stream/live_high.mp4?holo=false&pv=true&loopback=true'
snip_buffer = 3

def read_video_snippet(file_name, i):    
    read_chunks = 0    
    response = requests.get(live_stream_endpoint, auth=HTTPBasicAuth(username, password), verify=False, stream=True, allow_redirects=True)    
    if response.status_code != 200:
        print(f"Error: Could not access {live_stream_endpoint}")    
    else:    
        # print("Successfully opened get")
        if os.path.exists(file_name):
            os.remove(file_name)
        with open(file_name, "wb+") as f:
            start = time.time()
            for chunk in response:
                read_chunks += 1
                f.write(chunk)  
                if time.time() - start > 10:
                    break
    # print("Stopping ", i)

def main_stream():
    i = 0
    while True:
        seconds = time.time()
        # print(f"Starting thread at: {seconds}")
        idx = i%snip_buffer
        threading.Thread(target=read_video_snippet, args=(f"./hololens_api/snips/test{idx}.mp4", idx, )).start()
        i += 1
        time.sleep(8)

if __name__ == '__main__':
    main_stream()
