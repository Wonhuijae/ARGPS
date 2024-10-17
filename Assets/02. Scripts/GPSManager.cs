using Mapbox.Unity.Map;
using Mapbox.Utils;
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

        StartCoroutine(StartGPS());
    }

    IEnumerator StartGPS()
    {
        Input.location.Start();

        int maxWait = 20;
        while (true)
        {
            while (Input.location.status == LocationServiceStatus.Initializing &&
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

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                sampleText.text = "위치 정보를 가져오지 못했습니다.";
                yield break;
            }
            else
            {
                double lat = Input.location.lastData.latitude;
                double lon = Input.location.lastData.longitude;

                // sampleText.text =
                //    "현재 위치: " +
                //    lat + " " +
                //    lon + " " +
                //    Input.location.lastData.horizontalAccuracy;

                // dbInstance.TestWriteDB(lat.ToString(), lon.ToString());
                mapInstance.SetCurPos(lat, lon);
                if (abstractMap.InitializeOnStart == true)
                {
                    Vector2d initPos = new Vector2d(lat, lon);
                    abstractMap.Initialize(initPos, 15);
                }
                abstractMap.Options.locationOptions.latitudeLongitude = lat + "," + lon;
                abstractMap.UpdateMap();
                sampleText.text = abstractMap.Options.extentOptions.extentType.ToString();
            }

            yield return new WaitForSeconds(5);
        }
    }
}
