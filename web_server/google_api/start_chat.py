import os
import json
import google.generativeai as genai

def start_chat(redis_client, **kwargs):
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
    model = genai.GenerativeModel('gemini-pro', **kwargs)
    
    redis_client.set('last_key_used', last_key_used + 1)
    
    chat = model.start_chat(enable_automatic_function_calling=True)
    return chat