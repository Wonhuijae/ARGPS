using Mapbox.Examples;
using Mapbox.Unity.Location;
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

    public GameObject curPosText;
    public GameObject panelWriteMemo;

    public GameObject player;

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
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                yield break;
            }
            else
            {
                double lat = Input.location.lastData.latitude;
                double lon = Input.location.lastData.longitude;

                mapInstance.SetCurPos(lat, lon);
            }

            yield return new WaitForSeconds(5);
        }
    }

    public void OnWriteMemo()
    {
        player.GetComponent<RotateWithLocationProvider>().enabled = false;
        curPosText.SetActive(false);
        panelWriteMemo.SetActive(true);
    }

    public void OffWriteMemo()
    {
        player.GetComponent<RotateWithLocationProvider>().enabled = true;
        curPosText.SetActive(true);
        panelWriteMemo.SetActive(false);
    }
}
