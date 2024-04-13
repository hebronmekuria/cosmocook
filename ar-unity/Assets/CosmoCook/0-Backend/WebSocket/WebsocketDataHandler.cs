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

    public void HandleRecipeData(RecipeData data)
    {
        EventBus.Publish<RecipeDataReceived_Event>(new(data));
    }

    // Public functions for to call to send data
    public void SendInitialData(string color, string name)
    {
        InitialData emptyInitials = new InitialData();
        emptyInitials.color = color;
        emptyInitials.name = name;
        Debug.Log("TODO not impl");
        // if (debugMode) Debug.Log("(GET) WebsocketDataHandler.cs: Sending INITIAL data");
        // 
        // // Create a new CombinedData instance
        // InitialData combinedData = new InitialData
        // {
        //     type = "Initial",
        //     use = "PUT",
        //     color = data.color,
        //     name = data.name
        // };
        // 
        // // Convert the combined data to JSON format and send to WebSocket client
        // string jsonData = JsonUtility.ToJson(combinedData);
        // 
        // wsClient.SendJsonData(jsonData);
    }
}
