using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoComp : MonoBehaviour
{
    Memo memo;
    AppManager appManager;

    private void Awake()
    {
        appManager = AppManager.Instance;
    }

    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }

    public void OnClickMarker(GameObject _readMemo)
    {
        if (_readMemo == null )Debug.Log("Parameter _readMemo null");

        _readMemo.SetActive(true);
        Debug.Log(_readMemo.activeInHierarchy);

        if (memo == null)
        {
            Debug.Log("memoData MemoComp.memo null");
            return;
        }

        foreach(var o in _readMemo.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (o.gameObject.name == "Text_RoadPos") o.text = memo.address;
            else if (o.gameObject.name == "Text_Content") o.text = memo.memo;
        }

        
        Debug.Log("Method Call");
    }

    public void SetMemo(Memo _memo)
    {
        memo = _memo;
    }
}
