using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoComp : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }
}
