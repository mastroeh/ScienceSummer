using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketReceiver : MonoBehaviour
{
    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;
    private float receivedData; // For storing the received data


    void Start()
    {
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncomingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    private void ListenForIncomingRequests()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            tcpListener.Start();
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);
                            string dataString = Encoding.ASCII.GetString(incomingData);
                            UnityEngine.Debug.Log("Received data: " + dataString);

                            // Process the received data
                            ProcessReceivedData(dataString);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            UnityEngine.Debug.Log("SocketException: " + socketException.ToString());
        }
    }

    private void ProcessReceivedData(string dataString)
    {
        UnityEngine.Debug.Log("Raw received data: " + dataString); // Print the original received string

        if (float.TryParse(dataString, out float data))
        {
            receivedData = data;
            UnityEngine.Debug.Log("Received data: " + receivedData);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Invalid data received: " + dataString);
        }
    }


    public float GetReceivedData()
    {
        // Return the received data
        return receivedData;
    }

    private void OnDestroy()
    {
        tcpListenerThread?.Abort();  // Terminate thread immediately
        connectedTcpClient?.Close(); // Close TCP client connection
        tcpListener?.Stop();         // Stop TCP listener
    }

}

