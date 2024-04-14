using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainConnections : MonoBehaviour
{
    [SerializeField] string webSocketUrl;
    [SerializeField] bool autoConnectWebSocket = false;
    
    private bool websocketConnected;

    void Start()
    {
        websocketConnected = false;

        if (autoConnectWebSocket)
        {
            Connect();
        }
    }

    [ContextMenu("func Connect")]
    public void Connect()
    {
        StartCoroutine(_ConnectWebSocket(webSocketUrl));
    }

    private bool ConnectWebsocket(string connectionString)
    {
        return transform.GetComponent<WebSocketClient>().ReConnect(connectionString);
    }

    IEnumerator _ConnectWebSocket(string connectionString)
    {
        int i = 0;
        while (!ConnectWebsocket(connectionString) && i < 3)
        {
            Debug.Log("WebSocket: Connection Failed. Trying again in 2 seconds.");
            websocketConnected = false;

            yield return new WaitForSeconds(2f);
            i += 1;
        }

        websocketConnected = true;

        Debug.Log("WebSocket: Connection Successful");
    }

    public string getWebsocketURL()
    {
        return webSocketUrl;
    }
}
