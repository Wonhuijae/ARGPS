using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    bool waitForExit = false;
    int _layerMask;
    public GameObject readMemo;
    private NaverMap naverMap;
    public static AppManager Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<AppManager>();
            }
            return m_instance;
        }
    }
    private static AppManager m_instance;

    private void Awake()
    {
        if(Instance != this)
        {
            Destroy(gameObject);
        }
        // DontDestroyOnLoad(gameObject);

        naverMap = NaverMap.Instance;

        _layerMask = 1 << 7;

#if UNITY_ANDROID
        ApplicationChrome.statusBarState = ApplicationChrome.States.VisibleOverContent;
        RequestPermisson();
#elif UNITY_EDITOR
        SceneManager.LoadScene("MainScene");
#endif

        readMemo.SetActive(false);
    }

    private void Update()
    {
        Debug.Log("Update call");
        if (SceneManager.GetActiveScene().name == "SplashScene") return;

#if UNITY_ANDROID
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (waitForExit) Application.Quit();
            else
            {
                ShowToastMsg("종료하시려면 한 번 더 누르세요");
                StartCoroutine(WaitInput());  
            }
        }
#endif

        if (Input.GetMouseButtonDown(0))  // 마우스 왼쪽 버튼 클릭
        {
            Debug.Log("touch screen");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
            {
                Debug.Log("hit");
                MemoComp memo = hit.transform.GetComponentInChildren<MemoComp>();
                if (memo != null)
                {
                    memo.OnClickMarker(readMemo);
                }
                else
                {
                    Debug.Log("Is not Memo");
                }
            }
        }
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
            ShowToastMsg("위치 권한을 허용해주세요.");
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
        if (!Input.location.isEnabledByUser)
        {
            StartCoroutine(WaitForGPS());
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "TitleScene")  SceneManager.LoadScene("MainScene");
        }
    }

    IEnumerator WaitForGPS()
    {
        while (!Input.location.isEnabledByUser)
        {
            ShowToastMsg("GPS를 켜주세요");

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.LOCATION_SOURCE_SETTINGS");

            currentActivity.Call("startActivity", intent);
            yield return new WaitForSeconds(2f);
        }

        if (SceneManager.GetActiveScene().name == "TitleScene") SceneManager.LoadScene("MainScene");
    }

    IEnumerator WaitInput()
    {
        waitForExit = true;
        yield return new WaitForSeconds(2.5f);
        waitForExit = false;
    }

    public void ShowToastMsg(string msg)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (currentActivity != null) 
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject 
                = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, msg, 0);
                toastObject.Call("show");
            }));
        }
    }
}
