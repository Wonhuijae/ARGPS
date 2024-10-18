using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoComp : MonoBehaviour
{
    Memo memo;

    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }

    public void OnClickMarker(GameObject _readMemo)
    {
        if (_readMemo == null )Debug.Log("Parameter _readMemo null");


        if (memo == null)
        {
            Debug.Log("memoData MemoComp.memo null");
            return;
        }

        foreach(var o in _readMemo.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (o.gameObject.name == "Text_RoadPos") o.text = memo.adress;
            else if (o.gameObject.name == "Text_Content") o.text = memo.memo;
        }

        _readMemo.SetActive(true);
    }

    public void SetMemo(Memo _memo)
    {
        memo = _memo;
    }
}
