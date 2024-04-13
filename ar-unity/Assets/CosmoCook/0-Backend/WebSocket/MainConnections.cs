using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainConnections : MonoBehaviour
{
    [SerializeField] string webSocketUrl;
    [SerializeField] bool autoConnectWebSocket = false;
    
    private bool websocketConnected;

    // Start is called before the first frame update
    void Start()
    {
        websocketConnected = false;

        if (autoConnectWebSocket)
        {
            StartCoroutine(_ConnectWebSocket(webSocketUrl, "", "", 0));
        }
    }

    private bool ConnectWebsocket(string connectionString, string color, string name, int num)
    {
        if (num == 0)
        {
            return transform.GetComponent<WebSocketClient>().ReConnect(connectionString);
        } else
        {
            return transform.GetComponent<WebSocketClient>().ReConnect(connectionString, color, name);
        }
    }

    IEnumerator _ConnectWebSocket(string connectionString, string color, string name, int num)
    {
        while (!ConnectWebsocket(connectionString, color, name, num))
        {
            yield return new WaitForSeconds(5f);

            Debug.Log("WebSocket: Connection Failed. Trying again in 5 seconds.");
            websocketConnected = false;
        }

        websocketConnected = true;

        Debug.Log("WebSocket: Connection Successful");
    }

    public void ConnectToWebsocket(string connectionString, string color, string name)
    {
        StartCoroutine(_ConnectWebSocket(connectionString, color, name, 1));
    }

    public string getWebsocketURL()
    {
        return webSocketUrl;
    }
}
