from flask import Flask, request
from flask_socketio import SocketIO, emit
from google_api.search import get_recipe_from_search
from google_api.question import get_question_response
from hololens_api.stream import main_stream
from hololens_api.gemini_images import prompt_with_latest_image
import google.generativeai as genai
import redis
import os
import threading
import socket
import json

from dotenv import load_dotenv
load_dotenv()

GET_STREAM = False

app = Flask(__name__)
socketio = SocketIO(app, cors_allowed_origins='*')
redis_client = redis.Redis(host=os.getenv('REDIS_HOST'), port=19005, username='default', password=os.getenv('REDIS_PASSWORD'), db=0)

genai.configure(api_key=os.getenv('GENAI_API_KEY'))
model = genai.GenerativeModel('gemini-pro')

class CosmoCook:
    def __init__(self, app):
        if GET_STREAM:
            self.stream = threading.Thread(target=main_stream)
            self.stream.start()
        else:
            self.stream = None
        self.app = app
        self.recipe = {}
        self.question = {}
        self.image = {}
        self.chat = None

    def get_recipe(self, search):
        search += ' recipe'  # add 'recipe' to the search query to get more accurate results
        cache_key = f"recipe:{search}"
        cached_data = redis_client.get(cache_key)
        
        # Start a new chat session
        self.start_chat()
        
        if cached_data:
            return cached_data.decode('utf-8')
        else:
            recipe = get_recipe_from_search(search, self.chat, redis_client)
            self.recipe = recipe
            redis_client.set(cache_key, recipe)
            return recipe

    def ask_question(self, question):
        cache_key = f"question:{self.recipe['recipe_name']}:{question}"
        cached_data = redis_client.get(cache_key)
        
        if cached_data:
            return cached_data.decode('utf-8')
        else:
            response = get_question_response(self.recipe, question, self.chat)
            self.question = response
            redis_client.set(cache_key, response)
            return response

    def start_chat(self):
        self.chat = model.start_chat()
        return self.chat

cosmo_cook = CosmoCook(app)

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

@socketio.on('message')
def handle_message(message):
    data = json.loads(message)
    if data['type'] == 'ASK_QUESTION':
        question = data['data']['question']
        response = cosmo_cook.ask_question(question)
        socketio.send(json.dumps({'type': 'QUESTION_RESPONSE', 'data': response}))
    elif data['type'] == 'GET_RECIPE':
        search = data['data']['search']
        recipe = cosmo_cook.get_recipe(search)
        socketio.send(json.dumps({'type': 'RECIPE_RESPONSE', 'data': recipe}))
    
if __name__ == '__main__':
    hostname = socket.gethostname()
    ip_address = socket.gethostbyname(hostname)
    print(f"WebSocket URL: ws://{ip_address}:5000")
    
    socketio.run(app, host='0.0.0.0', debug=True)