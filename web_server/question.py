import google.generativeai as genai
from secret_data import google_api_key

def get_question_response(json_data, question):
    genai.configure(api_key=google_api_key)
    model = genai.GenerativeModel('gemini-pro')
    response = model.generate_content("You are a professional chef that can give expert advice on recipes. You will be given a recipe that is formatted in JSON alongside a question. Please answer the question fully and eloquently.\n\n" + json_data + "\n\n" + question)
    return response.text