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

public class NaverMap : MonoBehaviour
{
    public TextMeshProUGUI text;

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

    private float curLatitude; // 위도
    private float curLongitude; // 경도

    private string geocodeAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-geocode/v2/geocode";
    private string staticMapAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster?w=300&h=300&center=127.1054221,37.3591614&level=16";
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

        string url = $"https://naveropenapi.apigw.ntruss.com/map-reversegeocode/v2/gc?request=coordsToaddr&coords={coords}&sourcecrs={sourceCrs}&orders={orders}&output={output}";

        StartCoroutine(GetRequest(url));
    }

    IEnumerator GetRequest(string _url)
    {
        dbInstance.WriteDB("url", _url);

        using (UnityWebRequest request = UnityWebRequest.Get(_url))
        {
            dbInstance.WriteDB("urlafterreq", _url);

            request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientID);
            request.SetRequestHeader("X-NCP-APIGW-API-KEY", clientSecret);

            yield return request.SendWebRequest();

            foreach (var h in request.GetResponseHeaders())
            {
                dbInstance.WriteDB(h.Key, h.Value);
            }

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                dbInstance.WriteDB("requestFail", request.error);
            }
            else
            {
                string resposeBody = request.downloadHandler.text;
                dbInstance.WriteDB("responseBody", resposeBody);

                JObject json = JObject.Parse(resposeBody);

                foreach (var p in json.Properties())
                {
                    dbInstance.WriteDB(p.Name, p.Value.ToString());
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

        GetAddress(curLatitude, curLongitude);
    }
}
