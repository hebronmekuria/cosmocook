import google.generativeai as genai

def get_question_response(json_data, question, chat):
    response = chat.send_message("You are a professional chef that can give expert advice on recipes. You will be given a recipe that is formatted in JSON alongside a question. Please answer the question fully and eloquently.\n\n" + json_data + "\n\n" + question)
    return response.text