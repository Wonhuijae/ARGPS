using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.Networking;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;

public class NaverMap : MonoBehaviour
{
    string curAddress;
    Dictionary<string, string> responseSave;

    public static NaverMap Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<NaverMap>();
            }

            return m_instance;
        }
    }
    private static NaverMap m_instance;

    private DBManager dbInstance;
    private AppManager appInstance;

    private double curLatitude; // 위도
    private double curLongitude; // 경도

    // private int zoomLevel = 15;
    
    // private string staticMapAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster";
    // private string geocodeAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-geocode/v2/geocode";
    private string reverseGeoAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-reversegeocode/v2/gc?request=coordsToaddr&";
    private string clientID = "m43fqgqgf6";
    private string clientSecret = "MPaASwSsykcbLnaIyfjLUl5vzsvsEIA3Lrzp8mZV";

    public TextMeshProUGUI adressText;

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        dbInstance = DBManager.Instance;
        appInstance = AppManager.Instance;

        responseSave = new();
    }

    #region 맵박스 도입하여 제거
    //public void GetMapUrlAddress()
    //{
    //    /*
    //     * 네이버 Static Map API 사용 가이드
    //     * "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster?
    //     * w=(이미지 너비)&h=(이미지 높이)&
    //     * center=(중심점이 될 위도, 경도)
    //     * &level=(줌 레벨)
    //     * &X-NCP-APIGW-API-KEY-ID={API Gateway API Key ID}"
    //    */

    //    RectTransform t = mapImage.GetComponent<RectTransform>();
    //    string url = $"{staticMapAPIUrl}?w=540&h=920&center={curLongitude},{curLatitude}&level={zoomLevel}&markers=type:d|size:tiny|pos:{curLongitude} {curLatitude}";

    //    StartCoroutine(GetMapImage(url));
    //}
    

    //IEnumerator GetMapImage(string _url)
    //{

    //    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(_url))
    //    {
    //        request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientID);
    //        request.SetRequestHeader("X-NCP-APIGW-API-KEY", clientSecret);

    //        yield return request.SendWebRequest();

    //        if (request.result == UnityWebRequest.Result.ConnectionError ||
    //            request.result == UnityWebRequest.Result.ProtocolError)
    //        {
    //            appInstance.ShowToastMsg(request.error);
    //        }
    //        else
    //        {
    //            Texture2D t = DownloadHandlerTexture.GetContent(request);
    //            mapImage.texture = t;
    //        }
    //    }
    //}
    #endregion
    public void GetAddress(double _latitude, double _longitude)
    {
        /*
         * 네이버 Reverse Geocoding 사용 가이드
         * curl "https://naveropenapi.apigw.ntruss.com/map-reversegeocode/v2/gc?request=coordsToaddr&
         *      coords={입력_좌표}&sourcecrs={좌표계}&orders={변환_작업_이름}&output={출력_형식}"
	     *   -H "X-NCP-APIGW-API-KEY-ID: {애플리케이션 등록 시 발급받은 client id값}" \
	     *   -H "X-NCP-APIGW-API-KEY: {애플리케이션 등록 시 발급받은 client secret값}" -v
	     *   
         * 필수 파라미터
         * coords(string)(경도,위도)
         *
         * 필요 파라미터
         * orders = "roadaddr" (도로명 주소 요청)
         * output = "json" (json 형식 출력)
         */

        string coords = $"{_longitude},{_latitude}"; // 위도, 경도
        string sourceCrs = "epsg:4326"; // 위경도 좌표계(기본값, 4326)
        string orders = "roadaddr";
        string output = "json";

        string url = $"{reverseGeoAPIUrl}coords={coords}&sourcecrs={sourceCrs}&orders={orders}&output={output}";

        StartCoroutine(GetRequest(url));
    }

    IEnumerator GetRequest(string _url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(_url))
        {
            request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientID);
            request.SetRequestHeader("X-NCP-APIGW-API-KEY", clientSecret);

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                dbInstance.TestWriteDB("requestFail", request.error);
            }
            else
            {
                string resposeBody = request.downloadHandler.text;
                var json = JObject.Parse(resposeBody);

                ParsingAddress(json);
            }
        }
    }

    private void ParsingAddress(JToken _json)
    {
        var resultsArray = _json["results"] as JArray;
        curAddress = ""; // 초기화

        if (resultsArray != null && resultsArray.Count > 0)
        {
            responseSave.TryAdd("area1", resultsArray[0]["region"]["area1"]["name"].ToString());
            responseSave.TryAdd("area2", resultsArray[0]["region"]["area2"]["name"].ToString());
            responseSave.TryAdd("area3", resultsArray[0]["region"]["area3"]["name"].ToString());
            responseSave.TryAdd("area4", resultsArray[0]["region"]["area4"]["name"].ToString());
            responseSave.TryAdd("road", resultsArray[0]["land"]["name"].ToString());
            responseSave.TryAdd("number", resultsArray[0]["land"]["number1"].ToString());

            if (responseSave["area1"] != "") curAddress += responseSave["area1"] + " ";
            if (responseSave["area2"] != "") curAddress += responseSave["area2"] + " ";
            if (responseSave["area3"] != "") curAddress += responseSave["area3"] + " ";
            if (responseSave["area4"] != "") curAddress += responseSave["area4"] + " ";
            if (responseSave["road"] != "") curAddress += responseSave["road"] + " ";
            if (responseSave["number"] != "") curAddress += responseSave["number"] + " ";

            adressText.text = curAddress;
            Debug.Log(curAddress);
        }
    }

    public void SetCurPos(double _lati, double _long)
    {
        curLatitude = _lati;
        curLongitude = _long;

        // GetMapUrlAddress();
        GetAddress(curLatitude, curLongitude);
    }

    public string GetCurAddress()
    {
        return curAddress;
    }
    public string GetCurRoadName()
    {
        return responseSave["road"];
    }

    public int GetCurRoadNum()
    {
        return Convert.ToInt32(responseSave["number"]);
    }

    public double GetCurLatitude() { return curLatitude; }
    public double GetCurLongitude() {  return curLongitude; }
}
