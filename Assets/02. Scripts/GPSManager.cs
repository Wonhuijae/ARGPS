using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<GPSManager>();
            }

            return m_instance;
        }
    }
    private static GPSManager m_instance;

    private NaverMap mapInstance;
    private DBManager dbInstance;
    private AppManager appInstance;
    public AbstractMap abstractMap;

    public TextMeshProUGUI sampleText;

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        mapInstance = NaverMap.Instance;
        dbInstance = DBManager.Instance;
        appInstance = AppManager.Instance;


        Debug.Log(appInstance == null);
        RequestPermisson();
        
    }

    void RequestPermisson()
    {
        // 위치 정보 권한 비허용 시
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            StartCoroutine(WaitForPermission());
        }
        else
        {
            TurnOnGPS();
        }
    }

    IEnumerator WaitForPermission()
    {
        // 최초 권한 설정 확인
        yield return new WaitForSeconds(5f);

        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return new WaitForSeconds(2f);
            appInstance.ShowToastMsg("위치 권한을 허용해주세요.");
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                // 현재 액티비티
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                // 안드로이드의 작업 수행 명령어
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS");

                // 앱 패키지 이름으로 uri 생성
                string packageName = Application.identifier;
                AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").
                                        CallStatic<AndroidJavaObject>("parse", "package:" + packageName);

                intent.Call<AndroidJavaObject>("setData", uri);
                // startActivity 메서드 실행, intent 수행
                currentActivity.Call("startActivity", intent);
            }
        }

        TurnOnGPS();
    }

    void TurnOnGPS()
    {
        // GPS Off시
        if(!Input.location.isEnabledByUser)
        {
            StartCoroutine(WaitForGPS());
        }
        else
        {
            StartCoroutine(StartGPS());
        }
    }

    IEnumerator WaitForGPS()
    {
        while(!Input.location.isEnabledByUser)
        {
            appInstance.ShowToastMsg("GPS를 켜주세요");
            
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.LOCATION_SOURCE_SETTINGS");

            currentActivity.Call("startActivity", intent);
            yield return new WaitForSeconds(2f);
        }

        StartCoroutine(StartGPS());
    }

    IEnumerator StartGPS()
    {
        Input.location.Start();

        int maxWait = 20;
        while(Input.location.status == LocationServiceStatus.Initializing &&
            maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1) 
        {
            sampleText.text = "시간초과";
            yield break;
        }

        if(Input.location.status == LocationServiceStatus.Failed)
        {
            sampleText.text = "위치 정보를 가져오지 못했습니다.";
            yield break;
        }
        else
        {
            float lat = Input.location.lastData.latitude;
            float lon = Input.location.lastData.longitude;

            sampleText.text =
                "현재 위치: "+
                lat + " " +
                lon + " " +
                Input.location.lastData.horizontalAccuracy;

            // dbInstance.TestWriteDB(lat.ToString(), lon.ToString());
            mapInstance.SetCurPos(lat, lon);
            abstractMap.Options.locationOptions.latitudeLongitude = lat + "," + lon;
        }
    }
}
