using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Diagnostics;

public class DataSender : MonoBehaviour
{
    private string baseApiUrl = "http://localhost:5000/kickrbikeconnector/grade/";

    public void SendData(float value)
    {
        StartCoroutine(SendRequest((int)value));
    }

    IEnumerator SendRequest(int value)
    {
        string fullUrl = baseApiUrl + value; // Dynamically build a complete URL
        using (UnityWebRequest webRequest = UnityWebRequest.Get(fullUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                UnityEngine.Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                UnityEngine.Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }
}
