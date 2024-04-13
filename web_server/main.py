from flask import Flask, request
import asyncio
import websockets
import json
from google_api.search import get_recipe_from_search
from google_api.question import get_question_response
from hololens_api.stream import main_stream
from hololens_api.gemini_images import prompt_with_latest_image
import google.generativeai as genai
import redis
import os
import threading
import socket
import multiprocessing
from PIL import Image
import requests
import base64

from dotenv import load_dotenv
load_dotenv()
GET_STREAM = False
app = Flask(__name__)
redis_client = redis.Redis(host=os.getenv('REDIS_HOST'), port=19005, username='default', password=os.getenv('REDIS_PASSWORD'), db=0)


class CosmoCook:
    def __init__(self):
        if GET_STREAM:
            self.stream = threading.Thread(target=main_stream)
            self.stream.start()
        else:
            self.stream = None
        self.recipe = {}
        self.question = {}
        self.image = {}
        self.chat = None

    def get_recipe(self, search, no_cache = False):
        search += ' recipe'
        cache_key = f"recipe:{search}"
        cached_data = redis_client.get(cache_key)
        self.start_chat()
        if cached_data and not no_cache:
            print('Cache hit, returning cached recipe')
            self.recipe = json.loads(cached_data.decode('utf-8'))
            return self.recipe
        else:
            recipe = get_recipe_from_search(search, self.chat, redis_client, no_cache)
            print('Recipe fetched from search')
            self.recipe = recipe
            recipe = recipe.replace('\n', ' ')
            recipe = recipe.replace('\"', '"')

            print('Replaced newlines with spaces, parsing JSON')

            try:
                recipe = json.loads(recipe)
            except:
                print('Error parsing JSON, returning raw recipe')
                return recipe
            recipe = self.download_images(recipe, no_cache)
            redis_client.set(cache_key, json.dumps(recipe))
            
            return recipe

    def get_ingredient(self):
        # 'ingredients': [{'name':'onion'}, {'name':'celery'}, {'name':'chicken'}]
        
        ingredients = [x.get('name', "N/A").lower() for x in self.recipe.get('ingredients', {})]
        print("Ingredients ", ingredients)
        if not ingredients:
            return "N/A"
        
        prompt = f"Describe which ingredient is being held in the hand in the image given. Make sure that the ingredient comes from this list of ingredients {ingredients}. Return a response as a single string: '<ingredient_name>'"
        res = prompt_with_latest_image(prompt).strip().lower().replace('\'', '')
        print(f"Response: {res}")
        if res in ingredients:
            return res
        return "N/A"

    def download_images(self, recipe, no_cache = False):
        image_count = 0
        for step in recipe["steps"]:
            if step["image_url"]:
                image_count += 1

        print(f"Downloading {image_count} images")

        for step in recipe["steps"]:
            if "image_url" not in step:
                step["image_url"] = None
            elif step["image_url"]:
                cache_key = f"image:{step['image_url']}"
                cached_data = redis_client.get(cache_key)

                if cached_data and not no_cache:
                    print('Cache hit, returning cached image')
                    step["image_url"] = cached_data.decode('utf-8')
                else:
                    try:
                        response = requests.get(step["image_url"], stream=True)
                        img = Image.open(response.raw)
                        img.save('temp_image.jpg')

                        with open('temp_image.jpg', 'rb') as image_file:
                            image_data = base64.b64encode(image_file.read()).decode('utf-8')
                            step["image_url"] = image_data
                            redis_client.set(cache_key, image_data)

                    except Exception as e:
                        print(f"Error downloading image: {e}")
                        step["image_url"] = None

                    os.remove('temp_image.jpg')

        print('Images downloaded')
        
        return recipe

    def ask_question(self, question, no_cache = False):
        cache_key = f"question:{self.recipe['recipe_name']}:{question}"
        cached_data = redis_client.get(cache_key)
        if cached_data and not no_cache:
            print('Cache hit, returning cached question')
            return cached_data.decode('utf-8')
        else:
            response = get_question_response(self.recipe, question, self.chat)
            print('Response received')
            self.question = response
            redis_client.set(cache_key, response)
            return response

    def start_chat(self):
        print('Starting a new chat session')
        print(os.getenv("GOOGLE_API_KEYS"))
        keys = json.loads(os.getenv("GOOGLE_API_KEYS"))
        last_key_used = 0
        try:
            last_key_used = int(redis_client.get('last_key_used'))
            api_key = keys[last_key_used % len(keys)]
            print(f'Rotating to key {last_key_used % len(keys)}')
        except:
            print('last_key_used not found, using first key')
            api_key = keys[0]
            
        genai.configure(api_key=api_key)
        model = genai.GenerativeModel('gemini-pro')
        
        redis_client.set('last_key_used', last_key_used + 1)
        
        self.chat = model.start_chat()
        return self.chat

cosmo_cook = CosmoCook()

@app.route('/')
def index():
    return "Cosmo Cook is running!"

@app.route('/api/get_recipe')
def get_recipe():
    search = request.args.get('search')
    no_cache = request.args.get('no_cache')
    if no_cache == 'true':
        no_cache = True
    else:
        no_cache = False
    return cosmo_cook.get_recipe(search, no_cache)

@app.route('/api/ask_question')
def ask_question():
    question = request.args.get('question')
    no_cache = request.args.get('no_cache')
    if no_cache == 'true':
        no_cache = True
    else:
        no_cache = False
    return cosmo_cook.ask_question(question, no_cache)

async def handle_message(websocket, path):
    async for message in websocket:
        if (message == 'ping'):
            await websocket.send('pong')
            continue

        try:
            data = json.loads(message)
            print('Received:', data)
        except ValueError:
            print("Invalid JSON")
            await websocket.send(json.dumps({'type': 'ERROR', 'data': 'Invalid JSON'}))
            continue

        if data['type'] == 'ASK_QUESTION':
            print('Received ASK_QUESTION request')
            question = data['data']['question']
            response = cosmo_cook.ask_question(question)
            await websocket.send(json.dumps({'type': 'QUESTION_RESPONSE', 'data': response}))
        elif data['type'] == 'GET_RECIPE':
            print('Received GET_RECIPE request')
            search = data['data']['query']
            recipe = cosmo_cook.get_recipe(search)
            await websocket.send(json.dumps({'type': 'RECIPE_RESPONSE', 'data': recipe}))
        elif data['type'] == 'GET_INGREDIENT':
            print('Received GET_INGREDIENT')
            ingredient = cosmo_cook.get_ingredient()
            if ingredient != "N/A":
                await websocket.send(json.dumps({'type': 'INGREDIENT_RESPONSE', 'data': ingredient}))

def get_ip():
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        ip = s.getsockname()[0]
        s.close()
    except Exception:
        ip = "127.0.0.1"
    return ip

class FlaskAppProcess(multiprocessing.Process):
    def run(self):
        print(f"Server started on http://localhost:8000")
        app.run(host='0.0.0.0', port=8000, debug=True, use_reloader=False)

class WebSocketServerProcess(multiprocessing.Process):
    def run(self):
        start_server = websockets.serve(handle_message, "0.0.0.0", 8001)    
        print(f"Websocket server started on ws://{get_ip()}:8001")
        asyncio.get_event_loop().run_until_complete(start_server)
        asyncio.get_event_loop().run_forever()

if __name__ == "__main__":
    flask_process = FlaskAppProcess()    
    ws_process = WebSocketServerProcess()

    flask_process.start()
    ws_process.start()

    flask_process.join()
    ws_process.join()
