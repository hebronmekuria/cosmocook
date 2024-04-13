from googleapiclient.discovery import build
from google_images_search import GoogleImagesSearch
import os
import requests
import base64
from PIL import Image
import redis

YOUTUBE_API_KEY = os.getenv("GOOGLE_CONSOLE_API_KEY")

redis_client = redis.Redis(host=os.getenv('REDIS_HOST'), port=19005, username='default', password=os.getenv('REDIS_PASSWORD'), db=0)

def get_question_response(json_data, question, chat, redis_client):
    print('Asking question:', question)

    prompt = f"""You are a professional chef that can give expert advice on recipes. You will be given a recipe that is formatted in JSON alongside a question. Please answer the question fully and eloquently.
        You have access to the following tools:
        - search_youtube(query: str) -> str: Searches YouTube for a relevant video and returns the video URL.
        - search_images(query: str) -> str: Searches Google Images for a relevant image and returns the base64-encoded image data.

        Analyze the question and determine if a video or image would enhance your response. If so, use the appropriate tool to find relevant media.

        Return your response in the following JSON format:
        {{
        "text": "your text response",
        "video_url": "the YouTube video URL, if applicable",
        "image": "the base64-encoded image data, if applicable"
        }}

        Recipe JSON:
        {json_data}

        Question:
        {question}
        """
    print('Sending prompt to Gemini', prompt)

    response = chat.send_message(prompt)
    result = response.text
    print('Response received:', result)

    # make sure the 'text' field is present
    if 'text' not in result:
        result['text'] = response.text

    # remove the 'image_data' field if both 'video_url' and 'image_data' are present
    if 'video_url' in result and 'image_data' in result:
        del result['image_data']

    return result

def search_youtube(query: str) -> str:
    """Searches YouTube and returns a video link"""
    print('Searching for YouTube video:', query)
    youtube = build('youtube', 'v3', developerKey=YOUTUBE_API_KEY)
    request = youtube.search().list(
        part='snippet',
        maxResults=1,
        q=query
    )
    print('Executing search')
    response = request.execute()
    video_id = response['items'][0]['id']['videoId']
    print('Video found, ID:', video_id)
    return f'https://www.youtube.com/watch?v={video_id}'

def search_images(query: str) -> str:
    """Searches Google Images and returns a link to the first image"""
    gis = GoogleImagesSearch(YOUTUBE_API_KEY, os.getenv("GOOGLE_CONSOLE_SEARCH_ENGINE_ID"))
    print('Searching for image:', query)
    _search_params = {
        'q': query,
        'num': 1,
        'safe': 'off',
        'fileType': 'jpg'
    }
    gis.search(search_params=_search_params)
    url = gis.results()[0].url
    print('Image found, URL:', url)
    
    cache_key = f"image:{url}"
    cached_data = redis_client.get(cache_key)
    if cached_data:
        print('Cache hit, returning cached base64 image')
        return cached_data.decode('utf-8')
    else:
        try:
            print('Downloading image')
            response = requests.get(url, stream=True)
            img = Image.open(response.raw)
            img.save('temp_image.jpg')
            image_data = None
            
            with open('temp_image.jpg', 'rb') as image_file:
                print('Image downloaded, encoding to base64')
                image_data = base64.b64encode(image_file.read()).decode('utf-8')
                redis_client.set(cache_key, image_data)
            
            os.remove('temp_image.jpg')
            return image_data
                
        except Exception as e:
            print(f"Error downloading image: {e}")
            return None
        