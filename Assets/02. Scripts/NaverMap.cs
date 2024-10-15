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
    public TextMeshProUGUI text;
    public RawImage mapImage;

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

    private float curLatitude; // 위도
    private float curLongitude; // 경도
    private int zoomLevel = 15;
    
    private string staticMapAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster";
    private string geocodeAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-geocode/v2/geocode";
    private string reverseGeoAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-reversegeocode/v2/gc?request=coordsToaddr&";
    private string clientID = "m43fqgqgf6";
    private string clientSecret = "MPaASwSsykcbLnaIyfjLUl5vzsvsEIA3Lrzp8mZV";

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        dbInstance = DBManager.Instance;
        appInstance = AppManager.Instance;
    }

    public void GetMapUrlAddress()
    {
        /*
         * 네이버 Static Map API 사용 가이드
         * "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster?
         * w=(이미지 너비)&h=(이미지 높이)&
         * center=(중심점이 될 위도, 경도)
         * &level=(줌 레벨)
         * &X-NCP-APIGW-API-KEY-ID={API Gateway API Key ID}"
         */

        RectTransform t = mapImage.GetComponent<RectTransform>();
        string url = $"{staticMapAPIUrl}?w=540&h=920&center={curLongitude},{curLatitude}&level={zoomLevel}&markers=type:d|size:tiny|pos:{curLongitude} {curLatitude}";

        StartCoroutine(GetMapImage(url));
    }

    IEnumerator GetMapImage(string _url)
    {

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(_url))
        {
            request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientID);
            request.SetRequestHeader("X-NCP-APIGW-API-KEY", clientSecret);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                appInstance.ShowToastMsg(request.error);
            }
            else
            {
                Texture2D t = DownloadHandlerTexture.GetContent(request);
                mapImage.texture = t;
            }
        }
    }

    public void GetAddress(float _latitude, float _longitude)
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
        dbInstance.TestWriteDB("url", _url);

        using (UnityWebRequest request = UnityWebRequest.Get(_url))
        {
            dbInstance.TestWriteDB("urlafterreq", _url);

            request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientID);
            request.SetRequestHeader("X-NCP-APIGW-API-KEY", clientSecret);

            yield return request.SendWebRequest();

            foreach (var h in request.GetResponseHeaders())
            {
                dbInstance.TestWriteDB(h.Key, h.Value);
            }

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                dbInstance.TestWriteDB("requestFail", request.error);
            }
            else
            {
                string resposeBody = request.downloadHandler.text;
                dbInstance.TestWriteDB("responseBody", resposeBody);

                JObject json = JObject.Parse(resposeBody);

                foreach (var p in json.Properties())
                {
                    dbInstance.TestWriteDB(p.Name, p.Value.ToString());
                }
            }
        }
    }

    private void ParsingAddress(string _json)
    {
        Memo memo = new Memo();
    }

    public void SetCurPos(float _lati, float _long)
    {
        curLatitude = _lati;
        curLongitude = _long;

        GetMapUrlAddress();
        //GetAddress(curLatitude, curLongitude);
    }
}
