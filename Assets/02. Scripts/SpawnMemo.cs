using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnMemo : SpawnOnMap
{
    protected override void Start() 
    {
        base.Start();
    }

    public void LoadMemo(Memo _memo)
    {
        GameObject i = Instantiate(_markerPrefab);
        i.GetComponentInChildren<TextMeshProUGUI>().text = _memo.memo;
        string locString = _memo.latitude + "," + _memo.longitude;
        AddMarker(i, locString, _memo.ConverToVector2d());
    }
}
// 37.465765946796,126.868248548363