import requests
import json
from secret_data import google_api_key

def get_json_recipe(text):
    url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro-latest:generateContent?key={}".format(google_api_key)

    headers = {
        'Content-Type': 'application/json'
    }

    data = {
        "contents": [{
        "parts":[{
            "text": """
            Give recipe data for a recipe that is parsed from the internet using this schema:
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
            This is the recipe from the internet\n\n
            """ + text
            }]
        }],
        "generationConfig": {
            "response_mime_type": "application/json"
        }
    }

    response = requests.post(url, headers=headers, data=json.dumps(data))
    response_json = response.json()

    # Assuming the response is a list of items, you can print the first item
    print(response_json)