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

from dotenv import load_dotenv
load_dotenv()
GET_STREAM = False
app = Flask(__name__)
redis_client = redis.Redis(host=os.getenv('REDIS_HOST'), port=19005, username='default', password=os.getenv('REDIS_PASSWORD'), db=0)
genai.configure(api_key=os.getenv('GENAI_API_KEY'))
model = genai.GenerativeModel('gemini-pro')

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

    def get_recipe(self, search):
        search += ' recipe'
        cache_key = f"recipe:{search}"
        cached_data = redis_client.get(cache_key)
        self.start_chat()
        if cached_data:
            print('Cache hit, returning cached recipe')
            return cached_data.decode('utf-8')
        else:
            recipe = get_recipe_from_search(search, self.chat, redis_client)
            print('Recipe fetched from search')
            self.recipe = recipe
            redis_client.set(cache_key, recipe)
            return recipe

    def ask_question(self, question):
        cache_key = f"question:{self.recipe['recipe_name']}:{question}"
        cached_data = redis_client.get(cache_key)
        if cached_data:
            print('Cache hit, returning cached question')
            return cached_data.decode('utf-8')
        else:
            response = get_question_response(self.recipe, question, self.chat)
            print('Response received')
            self.question = response
            redis_client.set(cache_key, response)
            return response

    def start_chat(self):
        self.chat = model.start_chat()
        return self.chat

cosmo_cook = CosmoCook()

@app.route('/')
def index():
    return "Cosmo Cook is running!"

@app.route('/api/get_recipe')
def get_recipe():
    search = request.args.get('search')
    return cosmo_cook.get_recipe(search)

@app.route('/api/ask_question')
def ask_question():
    question = request.args.get('question')
    return cosmo_cook.ask_question(question)

async def handle_message(websocket, path):
    async for message in websocket:
        if (message == 'ping'):
            await websocket.send('pong')
            continue

        try:
            data = json.loads(message)
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
            search = data['data']['search']
            recipe = cosmo_cook.get_recipe(search)
            await websocket.send(json.dumps({'type': 'RECIPE_RESPONSE', 'data': recipe}))

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
