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

    public void HandleInitialData(InitialData data, string use)
    {
        if (use == "GET")
        {
            if (debugMode) Debug.Log("(GET) WebsocketDataHandler.cs: Sending INITIAL data");

            // Create a new CombinedData instance
            InitialData combinedData = new InitialData
            {
                type = "Initial",
                use = "PUT",
                color = data.color,
                name = data.name
            };

            // Convert the combined data to JSON format and send to WebSocket client
            string jsonData = JsonUtility.ToJson(combinedData);

            wsClient.SendJsonData(jsonData);
        } 
        else
        {
            try
            {
                InitialData combinedData = new InitialData
                {
                    type = "Initial",
                    use = "PUT",
                    color = data.color,
                    name = data.name
                };

                // Convert the combined data to JSON format and send to WebSocket client
                string jsonData = JsonUtility.ToJson(combinedData);

                wsClient.SendJsonData(jsonData);
            }
            catch (Exception ex)
            {
                Debug.LogError("An exception occurred: " + ex.Message);
                try
                {
                    // Create a new CombinedData instance
                    InitialData combinedData = new InitialData
                    {
                        id = 0,
                        type = "INITIAL",
                        data = "FAILURE"
                    };

                    // Convert the combined data to JSON format and send to WebSocket client
                    string jsonData = JsonUtility.ToJson(combinedData);

                    wsClient.SendJsonData(jsonData);
                }
                catch (Exception ex1)
                {
                    Debug.LogError("An exception occurred: " + ex1.Message);
                }
            }
        }
    }

    // Public functions for to call to send data
    public void SendInitialData(string color, string name)
    {
        InitialData emptyInitials = new InitialData();
        emptyInitials.color = color;
        emptyInitials.name = name;
        HandleInitialData(emptyInitials, "GET");
    }
}
