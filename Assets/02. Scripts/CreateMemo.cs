using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreateMemo : MonoBehaviour
{
    private DBManager dbInstance;
    private GPSManager gpsInstance;
    private NaverMap naverMap;

    public TextMeshProUGUI addressText;
    public TMP_InputField inputText;

    private void Awake()
    {
        dbInstance = DBManager.Instance;
        gpsInstance = GPSManager.Instance;
        naverMap = NaverMap.Instance;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        addressText.text = naverMap.GetCurAddress();
        inputText.text = "";
    }

    public void SaveMemo()
    {
        if (inputText.text == "") return;

        Memo newMemo = new Memo(inputText.text,
                                naverMap.GetCurLatitude(),
                                naverMap.GetCurLongitude());
        newMemo.SetAdress(naverMap.GetCurAddress());
        newMemo.road = naverMap.GetCurRoadName();

        dbInstance.WriteDB(newMemo);
    }
}
