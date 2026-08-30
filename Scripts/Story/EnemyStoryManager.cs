using UnityEngine;

public class EnemyStoryManager : SceneSingleton<EnemyStoryManager>
{
    public static int PursuadePoint = 0;
    private bool isStoryEnd = false;
    private int currentStoryIndex = 0;

    private bool isPursuadeSuccess = false;
    void Update()
    {
        if (isStoryEnd && Input.GetMouseButtonDown(0))
        {
            isStoryEnd = false;
            NextEnemyStory(currentStoryIndex); // 스토리 종료 후 다음 텍스트로 이동
        }
    }
    public void GetPursuadePoint(int point)
    {
        PursuadePoint += point;
        Debug.Log("Current Persuade Points: " + PursuadePoint);

        if (PursuadePoint >= 2)
        {
            // 다음 텍스트로 이동. -> 설득 성공
            currentStoryIndex = 29;
            isPursuadeSuccess = true;
            EndEnemyStory();
            return;
        }

        if (StoryCardManager.Instance.cards.Count <= 0)
        {
            // 다음 텍스트로 이동 -> 설득 실패
            currentStoryIndex = 32;
            isPursuadeSuccess = false;
            EndEnemyStory();
            return;
        }

        Debug.Log("Remaining Cards: " + StoryCardManager.Instance.cards.Count);
    }

    public void NoCardPursuadeFail()
    {
        // 카드가 없을 때 설득 실패 처리
        currentStoryIndex = 32;
        isPursuadeSuccess = false;
        EndEnemyStory();
    }

    private void NextEnemyStory(int nextIndex)
    {
        var enemyTextDTO = InGameData.Instance.EnemyTexts.Find(x => x.Id == nextIndex);
        EnemyText.Instance?.SetTextBox(enemyTextDTO);
    }

    private void EndEnemyStory()
    {
        // 종료 처리
        isStoryEnd = true;
        // 카드 없앰.
        StoryCardManager.Instance.DeleteAllCards();

        // 설득 성공/실패 처리
        SupabaseMainScenarioStoryUpdate.Instance.UpdateMainScenarioStoryClearData(EnumTypes.MainStoryType.onu_trust, isPursuadeSuccess);
    }
}
