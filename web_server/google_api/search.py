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
        headers = {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        }
        response = requests.get(url, headers=headers, allow_redirects=True, timeout=2)
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
    print('Fetching recipe from search:', query)
    first_url = get_first_google_url(query)
    cache_key = f"recipe_search:{first_url}"
    
    # Check if the recipe is already cached in redis
    cached_data = redis_client.get(cache_key)
    if cached_data:
        print('Cache hit, returning cached data')
        return cached_data.decode('utf-8')
    
    print('Fetching recipe from URL:', first_url)
    text = get_text_from_url(first_url)
    if text is None:
        print("Error fetching text from URL")
    print(text)

    print('Sending raw recipe text and schema to Gemini')
    response = chat.send_message("You are a professional chef, that can expertly create recipes. You must create a recipe for the following food: {query}. The recipe is scraped from the internet, please make sure to focus on the MAIN ingredient of the page and do not get distracted, and your job is to create a json recipe using the following schema. \n\n" + schema + "\n\n" + text)
    redis_client.set(cache_key, f"{response.text}")

    return response.text