from flask import Flask, request
from google_api.search import get_recipe_from_search
from google_api.question import get_question_response
import google.generativeai as genai
import redis
import os
from dotenv import load_dotenv
load_dotenv()

app = Flask(__name__)
redis_client = redis.Redis(host=os.getenv('REDIS_HOST'), port=19005, username='default', password=os.getenv('REDIS_PASSWORD'), db=0)

genai.configure(api_key=os.getenv('GENAI_API_KEY'))
model = genai.GenerativeModel('gemini-pro')

class CosmoCook:
    def __init__(self, app):
        self.app = app
        self.recipe = {}
        self.question = {}
        self.image = {}
        self.chat = None
        self.register_routes()

    def register_routes(self):
        self.app.add_url_rule('/api/get_recipe', 'get_recipe', self.get_recipe, methods=['GET'])
        self.app.add_url_rule('/api/ask_question', 'ask_question', self.ask_question, methods=['GET'])

    def get_recipe(self):
        search = request.args.get('search')
        cache_key = f"recipe:{search}"
        cached_data = redis_client.get(cache_key)
        
        # Start a new chat session
        self.start_chat()
        if cached_data:
            return cached_data.decode('utf-8')
        else:
            recipe = get_recipe_from_search(search + ' recipe', self.chat)
            self.recipe = recipe
            redis_client.set(cache_key, recipe)
            return recipe

    def ask_question(self):
        question = request.args.get('question')
        cache_key = f"question:{self.recipe.recipe_name}:{question}"
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

if __name__ == '__main__':
    app.run()