using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoComp : MonoBehaviour
{
    public TextMeshProUGUI textUI;

    private void Start()
    {
        textUI = GPSManager.Instance.sampleText;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))  // 마우스 왼쪽 버튼 클릭
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Hit 3D Object: " + hit.collider.name);

                if (hit.transform.GetComponentInChildren<MemoComp>() != null)
                {
                    textUI.text =  hit.transform.gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
                }
            }
        }
    }
}
