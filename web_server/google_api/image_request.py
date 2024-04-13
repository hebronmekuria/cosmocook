import requests
import base64
import os

# Your API key and Custom Search Engine ID
API_KEY = os.getenv('GOOGLE_CUSTOM_SEARCH_KEY')
CSE_ID = "1679c5a234c424bfc"

def get_first_image_url(query):
    url = f"https://www.googleapis.com/customsearch/v1?q={query}&cx={CSE_ID}&searchType=image&key={API_KEY}"
    response = requests.get(url)
    data = response.json()
    if "items" in data and len(data["items"]) > 0:
        first_image_url = data["items"][0]["link"]
        return first_image_url
    return None

def image_to_base64(url):
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36"
    }
    response = requests.get(url, headers=headers)
    if response.status_code == 200:
        # Encode the image as base64
        base64_image = base64.b64encode(response.content).decode('utf-8')
        return base64_image
    print("Error fetching image:", response.status_code)
    return None

def get_first_image_base64(query: str) -> str:
    file_query = query.replace(" ", "_")
    file = f"image_data/{file_query}.txt"
    if os.path.exists(file):
        with open(file, 'r') as f:
            return f.read()
    image_url = get_first_image_url(query)
    if image_url:
        base64_img = image_to_base64(image_url)
        if base64_img:
            with open(file, 'w+') as f:
                f.write(base64_img)
            return base64_img
    return None
