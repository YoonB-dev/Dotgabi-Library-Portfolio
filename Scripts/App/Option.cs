using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Option : MonoBehaviour
{
    public GameObject canvasOption, option;
    public void OpenOption()
    {
        AudioManager.Instance.ButtonClickSound1();
        canvasOption.SetActive(true);
        canvasOption.transform.GetChild(0).gameObject.SetActive(true); // 배경
        option.SetActive(true);
    }
    public void CloseOption()
    {
        AudioManager.Instance.ButtonClickSound2();
        canvasOption.SetActive(false);
        option.SetActive(false);
    }
}
