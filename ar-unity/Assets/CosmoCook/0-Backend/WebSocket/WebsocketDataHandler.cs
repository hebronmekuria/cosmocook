using UnityEngine;
using System.Collections.Generic;
using System;

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

        Debug.Log("(Send) Sending INITIAL data");
        
        // Convert the combined data to JSON format and send to WebSocket client
        string jsonData = JsonUtility.ToJson(request);
        
        wsClient.SendJsonData(jsonData);
    }

    [ContextMenu("func Debug")]
    public void Debugthis()
    {
        SendRequest_GetRecipe("greek salad");
    }
}
