from googlesearch import search
import requests
from bs4 import BeautifulSoup
import google.generativeai as genai
import os
import json

schema = """
            {
            "type": "object",
            "properties": {
                "recipe_name": {
                "type": "string"
                },
                "description": {
                "type": "string"
                },
                "prep_time_minutes": {
                "type": "integer",
                "minimum": 0
                },
                "cook_time_minutes": {
                "type": "integer",
                "minimum": 0
                },
                "total_time_minutes": {
                "type": "integer",
                "minimum": 0
                },
                "ingredients": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                    "name": {
                        "type": "string"
                    },
                    "quantity": {
                        "type": "string"
                    },
                    "unit": {
                        "type": "string"
                    }
                    },
                    "required": ["name", "quantity", "unit"]
                }
                },
                "steps": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                    "step_number": {
                        "type": "integer"
                    },
                    "description": {
                        "type": "string"
                    },
                    "image_url": {
                        "type": "string",
                        "format": "uri"
                    }
                    },
                    "required": ["step_number", "description", "image_url"]
                }
                },
                "servings": {
                "type": "integer",
                "minimum": 1
                }
            },
            "required": ["recipe_name", "description", "prep_time_minutes", "cook_time_minutes", "total_time_minutes", "ingredients", "steps", "servings"]
            }
            """

def get_first_google_url(query):
    try:
        search_results = search(query)
        first_url = next(search_results)
        return first_url
    except StopIteration:
        return None
    
def get_text_from_url(url):
    try:
        response = requests.get(url, allow_redirects=True, timeout=2)
        soup = BeautifulSoup(response.content, 'html.parser')

        # Replace <img> tags with their src attribute in the text
        for img_tag in soup.find_all('img'):
            if 'src' in img_tag.attrs:
                img_tag.replace_with(img_tag['src'])

        # Extract text including image URLs
        text = soup.get_text()

        return text
    except requests.exceptions.RequestException as e:
        print("Error fetching URL:", e)
        return None

def get_recipe_from_search(query, chat, redis_client):
    first_url = get_first_google_url(query)
    cache_key = f"recipe:{query}"
    
    # Check if the recipe is already cached in redis
    cached_data = redis_client.get(cache_key)
    if cached_data:
        return cached_data.decode('utf-8')
    
    text = get_text_from_url(first_url)
    if text is None:
        print("Error fetching text from URL")

    response = chat.send_message("You are a professional chef, that can expertly create recipes. You will be given recipe that is scraped from the internet, and your job is to create a json recipe using the following schema. \n\n" + schema + "\n\n" + text)
    redis_client.set(cache_key, f"{response.text}")
    
    return response.text