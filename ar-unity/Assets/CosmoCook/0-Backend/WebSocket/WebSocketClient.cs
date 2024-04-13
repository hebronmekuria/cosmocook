using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;
using PimDeWitte.UnityMainThreadDispatcher;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket ws;
    private WebsocketDataHandler dataHandler;
    private string webSocketUrl;

    private void Start()
    {
        dataHandler = GetComponent<WebsocketDataHandler>();
    }

    public void Disconnect()
    {
        try
        {
            if (ws != null && ws.IsAlive)
            {
                #if !UNITY_WEBGL
                    ws.Close();
                #endif
            }
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            //Debug.LogError("Error occurred in ReConnect method: " + ex.Message);
        }

    }

    [ContextMenu("func ReConnect")]
    public bool ReConnect()
    {
        try
        {
            if (ws != null && ws.IsAlive)
            {
                #if !UNITY_WEBGL
                    ws.Close();
                #endif
            }

            #if !UNITY_WEBGL
                ws = new WebSocket(webSocketUrl);
                ws.OnMessage += OnWebSocketMessage;
                ws.Connect();
                return ws.IsAlive;
            #endif

            return true;
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            //Debug.LogError("Error occurred in ReConnect method: " + ex.Message);
            return false;
        }
    }

    public bool ReConnect(string connectionString)
    {
        try
        {
            if (ws != null && ws.IsAlive)
            {
                #if !UNITY_WEBGL
                    ws.Close();
                #endif
            }

            webSocketUrl = connectionString;
            #if !UNITY_WEBGL
                ws = new WebSocket(webSocketUrl);
                ws.OnMessage += OnWebSocketMessage;
                ws.Connect();
                return ws.IsAlive;
            #endif

            return true;
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            Debug.LogError("Error occurred in ReConnect method: " + ex.Message);
            return false;
        }
    }

    private void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }

    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        if (e.Data != null)
        {
            try
            {
                UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread(e.Data));
            }
            catch (Exception ex)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError("Error handling JSON message: " + ex.Message));
            }
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogWarning("Received empty JSON data."));
        }
    }

    public IEnumerator ThisWillBeExecutedOnTheMainThread(string jsonData)
    {
        //Debug.Log("This is executed from the main thread");
        HandleJsonMessage(jsonData);
        yield return null;
    }


    public void HandleJsonMessage(string jsonData)
    {
        // Deserialize the JSON into JsonMessage class
        JsonMessage jsonMessage = JsonUtility.FromJson<JsonMessage>(jsonData);

        Debug.Log("Received json :" + jsonData);

        // Determine the type of data
        string messageType = jsonMessage.type;

        switch (messageType)
        {
            case "RECIPE":
                RecipeData json = JsonUtility.FromJson<RecipeData>(jsonData);
                dataHandler.HandleRecipeData(json);
                break;
            // Handle other message types similarly
            default:
                Debug.LogWarning("Unknown message type: " + messageType);
                break;
        }
    }

    public void SendJsonData(string jsonData)
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Send(jsonData);
        }
    }
}

[Serializable]
public class JsonMessage
{
    public string type;
}
