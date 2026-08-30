using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneLifeCycleManager : SceneSingleton<GameSceneLifeCycleManager>
{
    [SerializeField] private Canvas BlackCanvas;
    [SerializeField] private bool isMainStoryFinalBoss = false;
    void Start()
    {
        _ = GameSceneStart();
        // 처음 시작할 때는 검은 화면으로 시작
        Debug.Log("검은 화면");
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0f);
    }
    public async Task GameSceneStart()
    {
        Debug.Log("GameSceneStart 시작");
        // 기본적인 MoveSystem 세팅
        await MoveSystem.Instance?.TriggerStart();
        CheckFinalBoss();
        Debug.Log("MoveSystem TriggerStart 완료");

        StageManager.Instance?.SetTree();
        Debug.Log("트리 생성 완료");
        // MoveSystem에서 다음 스테이지 유무 체크
        MoveSystem.Instance?.CheckNextStage();
        Debug.Log("트리 생성 및 다음 스테이지 체크 완료");
        // Fade Out
        StartCoroutine(BlackCanvasFadeIn());

        // 텍스트 스토리 진행
        MoveSystem.Instance?.SetStartStory();
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

    private void CheckFinalBoss()
    {
        if (MoveSystem.Instance?.SCENARIO_DATA != null && MoveSystem.Instance?.SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO))
        {
            var mainScenario = (UserMainScenarioDTO)MoveSystem.Instance?.SCENARIO_DATA;

            if ((int)mainScenario.Difficulty >= 3 && mainScenario.CurrStageLevel == 4)
            {
                isMainStoryFinalBoss = true;
            }
        }
    }
}
