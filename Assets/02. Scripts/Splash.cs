using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Image>().DOFade(1, 1.5f);

        Invoke("LoadTitleScene", 1.7f);
    }

    void LoadTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
