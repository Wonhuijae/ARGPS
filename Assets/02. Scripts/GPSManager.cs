using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
using UnityEngine.InputSystem;
#endif

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

    public TextMeshProUGUI sampleText;

    public GameObject popUP;

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        mapInstance = NaverMap.Instance;
        dbInstance = DBManager.Instance;

        if (!Input.location.isEnabledByUser) popUP.SetActive(true);
        else popUP.SetActive(false);

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
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return new WaitForSeconds(10f);

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
            popUP.SetActive(true);
            StartCoroutine(WaitForGPS());
        }
        else
        {
            popUP.SetActive(false);
            StartCoroutine(StartGPS());
        }
    }

    IEnumerator WaitForGPS()
    {
        while(!Input.location.isEnabledByUser)
        {
            yield return new WaitForSeconds(5f);
            popUP.SetActive(true);
        }

        popUP.SetActive(false);
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

            // dbInstance.WriteDB(lat.ToString(), lon.ToString());
            // mapInstance.SetCurPos(lat, lon);
        }
    }
}
