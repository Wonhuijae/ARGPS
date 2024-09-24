using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public TextMeshProUGUI sampleText;

    void Awake()
    {
        if(Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        // ��ġ ���� ���� ����� �Ǵ� GPS ���� ��Ȱ��ȭ
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
            sampleText.text = "�ð��ʰ�";
            yield break;
        }

        if(Input.location.status == LocationServiceStatus.Failed)
        {
            sampleText.text = "��ġ ������ �������� ���߽��ϴ�.";
            yield break;
        }
        else
        {
            sampleText.text =
                "���� ��ġ: "+
                Input.location.lastData.latitude + " " +
                Input.location.lastData.longitude + " " +
                Input.location.lastData.horizontalAccuracy;
        }
    }
}
