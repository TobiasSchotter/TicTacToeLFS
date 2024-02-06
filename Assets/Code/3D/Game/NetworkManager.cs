using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    private TcpClient tcpClient;
    private NetworkStream stream;

    public string serverAddress = "127.0.0.1";
    public int serverPort = 8888;

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient(serverAddress, serverPort);
            stream = tcpClient.GetStream();

            // Start listening for messages from the server in a separate thread or coroutine
            StartCoroutine(ReceiveData());
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
        }
    }

    IEnumerator ReceiveData()
    {
        byte[] buffer = new byte[4096];

        while (true)
        {
            int bytesRead = 0;

            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading from server: {e.Message}");
                break;
            }

            if (bytesRead == 0)
            {
                // Connection closed by the server
                Debug.Log("Server disconnected");
                break;
            }

            string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Debug.Log($"Received data from server: {receivedData}");

            // Parse and handle received JSON data
            HandleReceivedData(receivedData);
        }

        yield return null;
    }

    void SendData(string data)
    {
        if (stream == null)
        {
            Debug.LogError("Not connected to the server");
            return;
        }

        byte[] dataBytes = Encoding.ASCII.GetBytes(data);

        try
        {
            stream.Write(dataBytes, 0, dataBytes.Length);
            Debug.Log($"Sent data to server: {data}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending data to server: {e.Message}");
        }
    }

    void HandleReceivedData(string receivedData)
    {
        // Parse the received JSON and execute the corresponding action
    }

    void OnDestroy()
    {
        if (tcpClient != null)
            tcpClient.Close();
    }
}
