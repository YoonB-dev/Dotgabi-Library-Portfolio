using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneLifeCycleManager : SceneSingleton<MainSceneLifeCycleManager>
{
    [SerializeField] private Canvas BlackCanvas;
    void Start()
    {
        MainSceneStart();
        // 처음 시작할 때는 검은 화면으로 시작
        Debug.Log("검은 화면");
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0f);
    }
    public void MainSceneStart()
    {
        Debug.Log("MainSceneStart 시작");
        // 초기 데이터 세팅
        MainManager.Instance?.SetTextAll();

        // Fade Out
        StartCoroutine(BlackCanvasFadeIn());
    }

    public IEnumerator BlackCanvasFadeIn()
    {
        Debug.Log("검은 화면 off");
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0f);
        yield return BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(0, 1f).WaitForCompletion();
        BlackCanvas.gameObject.SetActive(false);
        yield return null;
    }

    public IEnumerator BlackcanvasFadeOut()
    {
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(0, 0f);
        yield return BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0.3f).WaitForCompletion();
    }
}
