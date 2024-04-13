from flask import Flask, request
from google_api.search import get_recipe_from_search

app = Flask(__name__)

@app.route("/api/get_recipe")
def get_recipe():
    search = request.args.get('search')
    
    return get_recipe_from_search(search)