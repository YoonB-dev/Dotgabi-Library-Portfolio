using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class EnemyManagerTu : SceneSingleton<EnemyManagerTu>
{
    [SerializeField] private GameObject enemyPrefab;
    private float ratio = 1f;
    public List<EnemyTu> enemies;
    private int activeDeathCount = 0;

    void Start()
    {
        ratio = ButtonAnim.Instance.ratio;
    }

    public void SpawnEnemy(EnemyDTO enemyData, Vector3 position)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);

        var enemy = enemyObject.GetComponent<EnemyTu>();
        enemy.enemyDTO = enemyData.Copy();
        // 난이도 조절
        float magnification = 1;
        float magnification_HP = 1;

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
        enemy.player = TutorialBattle.Instance.player;
        enemy.enemyIndex = enemies.Count;
        enemy.enemyID = enemies.Count + 1;
        enemy.ratio = ratio;

        enemies.Add(enemy);
    }

    /// <summary>
    /// Summons on enemy based on the current game stage and story.
    /// </summary>
    public void SummonEnemy()
    {
        // 다음 몬스터가 지정되어 있으면 그 몬스터 소환
        GameManager.Instance.nextEnemyId = 8; // test
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
    public void EnemyDie(EnemyTu targetEnemy)
    {
        StartCoroutine(EnemyDieCo(targetEnemy));
    }


    private IEnumerator EnemyDieCo(EnemyTu targetEnemy)
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
            CardSystemTu.Instance.tuBattle.GetComponent<TutorialBattle>().SetVictoryBox();
        }
    }
}
