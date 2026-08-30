using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : SceneSingleton<EnemyManager>
{
    [SerializeField] private GameObject enemyPrefab;
    //public List<Enemy> enemyList;
    private ScenarioDTO SCENARIO_DATA;
    private int fightEnemyCount = 0;
    private float ratio = 1f;
    public List<Enemy> enemies;
    private int activeDeathCount = 0;
    public GameObject CurrTalkEnemyObj;
    [SerializeField] private Image storyImage;
    void Start()
    {
        storyImage?.gameObject.SetActive(false);
        ratio = ButtonAnim.Instance.ratio;
    }
    public void SpawnEnemy(EnemyDTO enemyData, Vector3 position)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);

        var enemy = enemyObject.GetComponent<Enemy>();
        enemy.enemyDTO = enemyData.Copy();
        // 난이도 조절
        float magnification = 1;
        float magnification_HP = 1;

        if (SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO))
        {
            var mainData = SCENARIO_DATA as UserMainScenarioDTO;
            if ((int)mainData.Difficulty >= 2 && (int)mainData.Difficulty <= 6)
            {
                magnification = 1.2f; magnification_HP = 1.5f;
            }
            else if ((int)mainData.Difficulty >= 7)
            {
                magnification = 1.5f; magnification_HP = 2f;
            }
        }


        enemy.enemyDTO.HealthMin = (int)(enemyData.HealthMin * magnification_HP);
        enemy.enemyDTO.HealthMax = (int)(enemyData.HealthMax * magnification_HP);
        enemy.enemyDTO.AttackMin = (int)(enemyData.AttackMin * magnification);
        enemy.enemyDTO.AttackMax = (int)(enemyData.AttackMax * magnification);
        enemy.enemyDTO.DefenseMin = (int)(enemyData.DefenseMin * magnification);
        enemy.enemyDTO.DefenseMax = (int)(enemyData.DefenseMax * magnification);
        enemy.enemyDTO.HealMin = (int)(enemyData.HealMin * magnification);
        enemy.enemyDTO.HealMax = (int)(enemyData.HealMax * magnification);
        // 적 상태 체력 초기화
        enemy.Stats.maxHp = UnityEngine.Random.Range(enemyData.HealthMin, enemyData.HealthMax + 1);
        enemy.Stats.currHp = enemy.Stats.maxHp;
        enemy.player = BattleManager.Instance.player;
        enemy.enemyIndex = enemies.Count;
        enemy.enemyID = enemies.Count + 1;
        enemy.ratio = ratio;

        enemies.Add(enemy);
    }

    private void SetScenarioData()
    {
        SCENARIO_DATA = BattleManager.Instance.SCENARIO_DATA;
    }

    /// <summary>
    /// Summons on enemy based on the current game stage and story.
    /// </summary>
    public void SummonEnemy()
    {
        SetScenarioData();

        // 다음 몬스터가 지정되어 있으면 그 몬스터 소환
        //GameManager.Instance.nextEnemyId = 38; // test

        // 메인 스토리 최종 보스 지정
        if (SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO) && SCENARIO_DATA.CurrStageLevel == 4)
        {
            GameManager.Instance.nextEnemyId = 39;
        }

        if (GameManager.Instance.nextEnemyId != null)
        {
            var enemyData = InGameData.Instance.Enemys.Find(e => e.Id == GameManager.Instance.nextEnemyId);
            if (enemyData != null)
            {
                SpawnEnemy(enemyData, Vector3.zero);
                EnemyAlignment();
                return;
            }
            else
            {
                Debug.LogWarning("No enemy found for the specified nextEnemyId.");
            }
        }

        System.Random rand = new System.Random(SCENARIO_DATA.GenerateSeed);
        var stageName = SCENARIO_DATA.StageList[SCENARIO_DATA.CurrStage - 1];
        List<EnemyDTO> enemyStageList = null;
        bool isStory = false;
        bool isBossOrElite = false;

        // 몬스터 타입별 리스트 추출
        if (GameManager.Instance.nextEnemyType == EnumTypes.EnemyType.elite)
        {
            enemyStageList = InGameData.Instance.Enemys.FindAll(e => e.Stage == ("elite_" + stageName));
            isBossOrElite = true;
        }
        else if (GameManager.Instance.nextEnemyType == EnumTypes.EnemyType.boss)
        {
            enemyStageList = InGameData.Instance.Enemys.FindAll(e => e.Stage == ("boss_" + stageName));
            isBossOrElite = true;
        }
        else if (SCENARIO_DATA is UserMainScenarioDTO storyData && storyData.IsNextEnemyStory)
        {
            enemyStageList = InGameData.Instance.Enemys.FindAll(e => e.Stage == ("story_" + stageName));
            isStory = true;
        }
        else
        {
            enemyStageList = InGameData.Instance.Enemys.FindAll(e => e.Stage == ("public_" + stageName) || e.Stage == ("story_" + stageName));
        }

        // 일반(monster)만 셔플
        if (enemyStageList != null && !isStory)
        {
            for (int i = enemyStageList.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                var temp = enemyStageList[i];
                enemyStageList[i] = enemyStageList[j];
                enemyStageList[j] = temp;
            }
        }

        // 스토리 몬스터: 리스트 섞지 않고, isEliteClear에 따라 0/1번 인덱스만 소환
        if (isStory && enemyStageList != null && enemyStageList.Count > 0)
        {
            if (SCENARIO_DATA is not UserMainScenarioDTO storyData) return;
            var mainData = SCENARIO_DATA as UserMainScenarioDTO;
            int index = mainData.IsEliteClear ? 1 : 0;
            if (index < enemyStageList.Count)
            {
                SpawnEnemy(enemyStageList[index], Vector3.zero);
            }
            else
            {
                Debug.LogWarning("No valid story enemy for the index.");
            }
        }
        // 엘리트/보스/일반 몬스터: 전체 리스트 소환
        else if (enemyStageList != null && enemyStageList.Count > 0)
        {
            if (isBossOrElite)
            {
                // 보스/엘리트는 리스트 전체 소환
                foreach (var enemy in enemyStageList)
                {
                    SpawnEnemy(enemy, Vector3.zero);
                }
            }
            else
            {
                // 일반 몬스터는 count에 따라 1~3마리 소환 (0번째 몬스터 기준)
                int count = enemyStageList[SCENARIO_DATA.FightTime].Count;
                if (count == 2)
                {
                    var ran2 = new System.Random(SCENARIO_DATA.GenerateSeed + SCENARIO_DATA.SelectList.Count * 3);
                    count = ran2.Next(1, 3) + 1;
                }
                for (int i = 0; i < count; i++)
                {
                    SpawnEnemy(enemyStageList[SCENARIO_DATA.FightTime], Vector3.zero);
                }
            }

        }
        else
        {
            Debug.LogWarning("No enemy found for the current stage.");
        }
        // 적 오브젝트 정렬
        EnemyAlignment();
    }

    /// <summary>
    /// Aligns the enemies in the scene based on the number of enemies.
    /// </summary>
    public void EnemyAlignment()
    {
        Debug.Log("Aligning enemies in the scene." + enemies.Count);

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].moveTween != null && enemies[i].moveTween.IsActive())
            {
                enemies[i].moveTween.Kill();
            }
            enemies[i].moveTween = DOTween.Sequence();
            enemies[i].moveTween.SetLink(enemies[i].gameObject);
        }

        switch (enemies.Count)
        {
            case 1:
                enemies[0].moveTween.Append(enemies[0].transform.DOLocalMove(new Vector2(0, 2), 0.3f));
                enemies[0].moveTween.Join(enemies[0].transform.DOScale(new Vector3(1f, 1f, 1f) * ratio, 0.3f));
                break;
            case 2:
                enemies[0].moveTween.Append(enemies[0].transform.DOLocalMove(new Vector2(-2.5f * ratio, 2), 0.3f));
                enemies[0].moveTween.Join(enemies[0].transform.DOScale(new Vector3(1f, 1f, 1f) * ratio, 0.3f));

                enemies[1].moveTween.Append(enemies[1].transform.DOLocalMove(new Vector2(2.5f * ratio, 2), 0.3f));
                enemies[1].moveTween.Join(enemies[1].transform.DOScale(new Vector3(1f, 1f, 1f) * ratio, 0.3f));
                break;
            case 3:
                enemies[0].moveTween.Append(enemies[0].transform.DOLocalMove(new Vector2(-3f * ratio, 3.2f), 0.3f));
                enemies[0].moveTween.Join(enemies[0].transform.DOScale(new Vector3(0.8f, 0.8f, 1f) * ratio, 0.3f));

                enemies[1].moveTween.Append(enemies[1].transform.DOLocalMove(new Vector3(0, 2f, -1f), 0.3f));
                enemies[1].moveTween.Join(enemies[1].transform.DOScale(new Vector3(1f, 1f, 1f) * ratio, 0.3f));

                enemies[2].moveTween.Append(enemies[2].transform.DOLocalMove(new Vector2(3f * ratio, 3.2f), 0.3f));
                enemies[2].moveTween.Join(enemies[2].transform.DOScale(new Vector3(0.8f, 0.8f, 1f) * ratio, 0.3f));
                break;
        }
    }

    public IEnumerator CheckEnemyDie()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].isDie)
            {
                enemies[i].DeleteObject();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
        yield return null;
    }
    public void EnemyDie(Enemy targetEnemy)
    {
        StartCoroutine(EnemyDieCo(targetEnemy));
    }


    private IEnumerator EnemyDieCo(Enemy targetEnemy)
    {
        if (targetEnemy == null || !enemies.Contains(targetEnemy)) yield break;

        activeDeathCount++;
        targetEnemy.StopCoroutine();
        targetEnemy.transform.GetChild(0).GetChild(6).gameObject.SetActive(false);
        targetEnemy.isDie = true;

        var enemyImg = targetEnemy.transform.GetChild(1);
        var enemyImgRenderer = enemyImg.GetComponent<MeshRenderer>();
        // 적이 죽으면 바로 리스트 제거
        enemies.Remove(targetEnemy);

        if (SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO) && enemies.Count == 0 && targetEnemy.isText)
        {
            Debug.Log("대화합니다");
            CurrTalkEnemyObj = targetEnemy.gameObject;
            yield return new WaitForSecondsRealtime(0.5f);
            CardSystem.Instance?.CardGroupSetActive(false);
            targetEnemy.transform.GetChild(0).gameObject.SetActive(false);
            enemyImgRenderer.sortingLayerName = "Ui_Victory";
            enemyImgRenderer.sortingOrder = 2;
            EnemyText.Instance.SetEnemyText(targetEnemy.enemyDTO, targetEnemy.gameObject);

            yield break;
        }

        var skeleton = enemyImg.GetComponent<SkeletonAnimation>().skeleton; // enemy는 SkeletonAnimation 컴포넌트
        Color startColor = skeleton.GetColor(); // 현 색상+알파(보통 1)

        var fadeoutTween = DOTween.To(
            () => skeleton.GetColor().a,// 현재 알파값을 읽음
            a => {
                // 현재 색상에서 알파만 변경
                Color newColor = skeleton.GetColor();
                newColor.a = a;
                skeleton.SetColor(newColor);
            },
            0f, // 목표 알파값(완전 투명)
            0.7f // 트윈 지속 시간(0.7초)
        );

        yield return fadeoutTween.WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);
        Destroy(targetEnemy.gameObject);

        activeDeathCount--;
        if (activeDeathCount == 0)
        {
            EnemyAlignment();

            if (enemies.Count <= 0)
            {
                CardSystem.Instance.canActive = false;
                CardSystem.Instance.canDrag = false;
                BattleManager.Instance.EndBattle();
            }
        }
    }

    public void ChangeEnemySpine(string imgPath)
    {
        storyImage.gameObject.SetActive(false);
        CurrTalkEnemyObj.SetActive(true);

        var enemySpine = CurrTalkEnemyObj.transform.GetChild(1).GetComponent<SkeletonAnimation>();
        var skeletonDataAsset = Resources.Load<SkeletonDataAsset>(imgPath + "_SkeletonData");
        Debug.Log(imgPath + "_SkeletonData");
        StartCoroutine(SpineUtils.Instance.FadeOutInChangeSpine(enemySpine, skeletonDataAsset, 0.5f));
    }

    // 단순 이미지 변경
    public void ChangeEnemyImage(string imgPath, bool isFirst)
    {
        storyImage.gameObject.SetActive(true);
        CurrTalkEnemyObj.SetActive(false);
        if (isFirst)
        {
            ButtonAnim.Instance.ButtonScaleIn(storyImage.gameObject, 0.2f, 1f, 0.3f);
        }
        storyImage.sprite = Resources.Load<Sprite>(imgPath);
        storyImage.SetNativeSize();
    }
}

