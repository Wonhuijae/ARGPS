using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

    void Awake()
    {
        if(Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        mapInstance = NaverMap.Instance;
        dbInstance = DBManager.Instance;
    }

    IEnumerator Start()
    {
        // 위치 정보 권한 비허용 또는 GPS 서비스 비활성화
        if (!Input.location.isEnabledByUser) yield break;

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
