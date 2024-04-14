using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Search;

/*
Name: Brian Schneider
Description: This script handles data communication through websockets, including sending and receiving various types of data.
 */

public class WebsocketDataHandler : MonoBehaviour
{
    private WebSocketClient wsClient;
    [SerializeField] private bool debugMode = false;


    public void Start()
    {
        wsClient = GetComponent<WebSocketClient>();
        EventBus.Subscribe<VoiceCommandSend_Event>(e =>
        {
            Debug.Log("Voice command being sent");
            if (e.type == "recipe")
            {
                SendRequest_GetRecipe(e.command);
            }
            else
            {
                SendRequest_AskQuestion(e.command);
            }
        });
    }


    public void RecvResponse_Recipe(RecipeData data)
    {
        Debug.Log(data.recipe_name);
        EventBus.Publish<RecipeDataReceived_Event>(new(data));
    }

    public void RecvResponse_Popup(PopupData data)
    {
        Debug.Log(data.text);
        EventBus.Publish<CreatePopup_Event>(new(data));
    }

    public void RecvResponse_Ingredient(string data)
    {
        Debug.Log(data);
        EventBus.Publish<IngredientFound_Event>(new(data));
    }

    public void SendRequest_GetRecipe(string query)
    {
        Request_GetRecipe request = new();
        request.type = "GET_RECIPE";
        request.data = new(query);

        Debug.Log("(Send) Sending GET_RECIPE");
        
        // Convert the combined data to JSON format and send to WebSocket client
        string jsonData = JsonUtility.ToJson(request);
        
        wsClient.SendJsonData(jsonData);
    }
    public void SendRequest_AskQuestion(string query)
    {
        Request_GetRecipe request = new();
        request.type = "ASK_QUESTION";
        request.data = new(query);

        Debug.Log("(Send) Sending ASK_QUESTION");

        // Convert the combined data to JSON format and send to WebSocket client
        string jsonData = JsonUtility.ToJson(request);

        wsClient.SendJsonData(jsonData);
    }
    public void SendRequest_GetIngredient()
    {
        Request_GetIngredient request = new();
        request.type = "GET_INGREDIENT";
        request.data = "";

        Debug.Log("(Send) Sending GET_INGREDIENT");

        // Convert the combined data to JSON format and send to WebSocket client
        string jsonData = JsonUtility.ToJson(request);

        wsClient.SendJsonData(jsonData);
    }

    [ContextMenu("func GET_RECIPE")]
    public void Debugthis()
    {
        SendRequest_GetRecipe("greek salad");
    }
    [ContextMenu("func ASK_QUESTION")]
    public void Debugthis2()
    {
        SendRequest_AskQuestion("How many serving is this?");
    }
    [ContextMenu("func GET_INGREDIENT")]
    public void Debugthis3()
    {
        SendRequest_GetIngredient();
    }
}
