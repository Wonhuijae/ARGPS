using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    bool waitForExit = false;

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
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (waitForExit) Application.Quit();
            else
            {
                ShowToastMsg("종료하시려면 한 번 더 누르세요");
                StartCoroutine(WaitInput());  
            }
        }
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
