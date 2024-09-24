using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class NaverMap : MonoBehaviour
{
    public static NaverMap Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<NaverMap>();
            }

            return m_instance;
        }
    }
    private static NaverMap m_instance;

    private string geocodeAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-geocode/v2/geocode";
    private string staticMapAPIUrl = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster?w=300&h=300&center=127.1054221,37.3591614&level=16";
    private string clientID = "m43fqgqgf6";
    private string clientSecret = "MPaASwSsykcbLnaIyfjLUl5vzsvsEIA3Lrzp8mZV";

    private static readonly HttpClient client = new HttpClient();

    private void Awake()
    {
        if(Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public async Task GetAddress(float _latitude, float _longitude)
    {
        /*
         * ���̹� Reverse Geocoding ��� ���̵�
         * curl "https://naveropenapi.apigw.ntruss.com/map-reversegeocode/v2/gc?request=coordsToaddr&
         *      coords={�Է�_��ǥ}&sourcecrs={��ǥ��}&orders={��ȯ_�۾�_�̸�}&output={���_����}"
	     *   -H "X-NCP-APIGW-API-KEY-ID: {���ø����̼� ��� �� �߱޹��� client id��}" \
	     *   -H "X-NCP-APIGW-API-KEY: {���ø����̼� ��� �� �߱޹��� client secret��}" -v
	     *   
         * �ʼ� �Ķ����
         * coords(string)(�浵,����)
         *
         * �ʿ� �Ķ����
         * orders = "roadaddr" (���θ� �ּ� ��û)
         * output = "json" (json ���� ���)
         */

        string coords = $"{_longitude},{_latitude}"; // ����, �浵
        string sourceCrs = "epsg:4326"; // ���浵 ��ǥ��(�⺻��, 4326)
        string orders = "roadaddr";
        string output = "json";

        string url = $"https://naveropenapi.apigw.ntruss.com/map-reversegeocode/v2/gc?request=coordsToaddr&coords={coords}&sourcecrs={sourceCrs}&orders={orders}&output={output}";

        client.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY-ID", clientID);
        client.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY", clientSecret);

        HttpResponseMessage response = await client.GetAsync(url);

        if(response.IsSuccessStatusCode)
        {
            string resposeBody = await response.Content.ReadAsStringAsync();
            ParsingAddress(resposeBody);
        }
        else
        {
            Debug.Log($"Error: {response.StatusCode}");
        }
    }

    private void ParsingAddress(string _json)
    {
        Memo memo = new Memo();
    }
}
