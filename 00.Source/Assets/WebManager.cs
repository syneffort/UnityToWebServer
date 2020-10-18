using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum WebMethod
{
    POST = 0,
    GET,
    PUT,
    DELETE,
}

public class GameResult
{
    public string UserName;
    public int Score;
}


// Login => Auth Token
// Game Start => Check Auth Token => DB(Redis, RDBMS)
// Game Result => Check Auth Token => Check Game Start => DB
public class WebManager : MonoBehaviour
{
    private string _baseUrl = "http://localhost:50573/api";

    // Start is called before the first frame update
    void Start()
    {
        GameResult res = new GameResult()
        {
            UserName = "UnitySynK",
            Score = 999
        };

        SendPostRequest("ranking", res, (uwr) =>
        {
            Debug.Log("TODO : UI 갱신하기");
        });

        SendGetAllRequest("ranking", (uwr) =>
        {
            Debug.Log("TODO : UI 갱신하기");
        });
    }

    // 자작서버를 외부 Api 공개할 계획이 아니라면,
    // REST 표준이 아니라 POST 등 하나의 방식으로 전송하고
    // 웹서버 상에서 CRUD를 알아서 처리해줘도 됨
    public void SendPostRequest(string url, object param, Action<UnityWebRequest> callback)
    {
        StartCoroutine(CoSendWebRequest(url, WebMethod.POST, param, callback));
    }

    public void SendGetAllRequest(string url, Action<UnityWebRequest> callback)
    {
        StartCoroutine(CoSendWebRequest(url, WebMethod.GET, null, callback));
    }

    private IEnumerator CoSendWebRequest(string url, WebMethod method, object param, Action<UnityWebRequest> callback)
    {
        string sendUrl = $"{_baseUrl}/{url}";

        byte[] jsonByte = null;

        if (param != null)
        {
            string jsonStr = JsonUtility.ToJson(param);
            jsonByte = Encoding.UTF8.GetBytes(jsonStr);
        }

        UnityWebRequest uwr = new UnityWebRequest(sendUrl, method.ToString());
        uwr.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
        uwr.uploadHandler = new UploadHandlerRaw(jsonByte);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError || uwr.isHttpError)
            Debug.Log(uwr.error + uwr.responseCode);
        else
        {
            Debug.Log($"Recv : {uwr.downloadHandler.text}");
            callback.Invoke(uwr);
        }
    }
}
