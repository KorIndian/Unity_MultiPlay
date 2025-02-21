using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager 
{
    public string BaseUrl { get; set; } = "https://localhost:7243/api"; //TODO config파일에서 관리

    public void SendPostRequest<T>(string url, object obj, Action<T> callBack)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest<T>(url,"POST",obj, callBack));
    }

    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> callBack)
    {
        string sendUrl = $"{BaseUrl}/{url}";

        byte[] jsonBytes = null;

        if(obj != null)
        {
            string JsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            jsonBytes = Encoding.UTF8.GetBytes(JsonString);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
			uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if(uwr.result == UnityWebRequest.Result.Success)
            {
				T resObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(uwr.downloadHandler.text);
                //T resObj = JsonUtility.FromJson<T>(uwr.downloadHandler.text);
				callBack.Invoke(resObj);
			}
            else
            {
                Debug.Log(uwr.error);
            }
		}
    }
}
