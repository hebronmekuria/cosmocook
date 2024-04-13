from search import get_recipe_from_search
from question import get_question_response
from image_request import get_first_image_base64
import google.generativeai as genai
from dotenv import load_dotenv
load_dotenv()

genai.configure(api_key=os.getenv('GENAI_API_KEY'))
model = genai.GenerativeModel('gemini-pro')
chat = model.start_chat()

json_data = get_recipe_from_search("Chicken Alfredo recipe", chat)
print(json_data)
question_response = get_question_response(json_data, "How much pasta should I buy, and what kind?", chat)
print(question_response)
# base64_image = get_first_image_base64("minced garlic")
