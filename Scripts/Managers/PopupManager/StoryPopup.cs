using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryPopup : MonoBehaviour
{
    [Header("Story Popup")]
    [SerializeField] private Canvas StoryCanvas;
    [SerializeField] private GameObject StoryBox;
    [SerializeField] private GameObject StoryContentPrefab;
    [SerializeField] private Transform StoryContentPos;

    [Header("Story Detail Popup")]
    [SerializeField] private GameObject StoryDetailCanvas;
    [SerializeField] private GameObject StoryDetailBox;
    [SerializeField] private TextMeshProUGUI StoryDetailTitle;
    [SerializeField] private Image StoryDetailImage;
    [SerializeField] private Transform BossContentPos;
    [SerializeField] private Transform EliteContentPos;
    [SerializeField] private Transform MonsterContentPos;
    [SerializeField] private RectTransform StoryContent; // 몬스터 개수에 따른 높이 조절
    [SerializeField] private GameObject EnemyButtonPrefab;
    [SerializeField] private RectTransform StoryTextBox;

    [Header("EnemyDetail")]
    [SerializeField] private GameObject EnemyDetailCanvas;
    [SerializeField] private GameObject EnemyDetailBox;
    [SerializeField] private GameObject PassivePos;
    [SerializeField] private GameObject PassivePrefab;

    [Header("EnemyDetail - Passive")]
    [SerializeField] private GameObject PassiveDetailCanvas;
    [SerializeField] private GameObject PassiveDetailBox;

    public void ShowStoryPopup()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        //Camera 움직임 비활성화
        MainManager.Instance.cambox.SetCanMove(false);
        //Canvas 활성화
        StoryCanvas.gameObject.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(StoryBox, 0f, 1f);
        //스토리 오브젝트 설정
        SetStoryObj();
    }

    public void CloseStory()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        //카메라 움직임 활성화
        MainManager.Instance.cambox.SetCanMove(true);
        //Canvas 비활성화
        StoryCanvas.gameObject.SetActive(false);
    }

    private void SetStoryObj()
    {
        //스토리 오브젝트 설정
        foreach (Transform child in StoryContentPos)
        {
            child.gameObject.SetActive(false);
        }

        var storyList = InGameData.Instance.Stories;

        for (int i = StoryContentPos.childCount; i < storyList.Count; i++)
        {
            var obj = Instantiate(StoryContentPrefab, StoryContentPos);
            obj.SetActive(false);
        }

        // 스토리 오브젝트 설정
        foreach (Transform child in StoryContentPos)
        {
            child.gameObject.SetActive(true);
            int index = child.GetSiblingIndex();
            var story = InGameData.Instance.Stories[index];
            child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = story.Name;

            child.GetComponent<Button>().onClick.RemoveAllListeners();
            child.GetComponent<Button>().onClick.AddListener(() => {
                //SFX
                AudioManager.Instance.ButtonClickSound1();
                OpenStoryDetail(story);
            });
        }
    }


    private void OpenStoryDetail(StoryDTO storyDTO)
    {
        StoryDetailCanvas.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(StoryDetailBox, 0f, 1f);
        StoryDetailTitle.text = storyDTO.Name;
        StoryDetailImage.sprite = Resources.Load<Sprite>(storyDTO.ImgPath);

        // BOSS
        var bosses = InGameData.Instance.Enemys.FindAll(b =>
            b.Stage.Split("_")[0] == "boss" && b.Stage.Split("_")[1] == storyDTO.StoryId.ToString()
        );

        BossContentPos.transform.GetChild(0).gameObject.SetActive(false);
        BossContentPos.transform.GetChild(1).gameObject.SetActive(false);

        for (int i = 0; i < bosses.Count; i++)
        {
            var bossObj = BossContentPos.GetChild(i).gameObject;
            if (i < bosses.Count)
            {
                bossObj.SetActive(true);
                bossObj.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(bosses[i].ImgFacePath);
                int index = i;
                bossObj.GetComponent<Button>().onClick.RemoveAllListeners();
                bossObj.GetComponent<Button>().onClick.AddListener(() => {
                    OpenEnemyDetail(bosses[index]);
                });
                // x좌표 위치 설정
                Vector3 pos = bossObj.transform.localPosition;
                if (bosses.Count == 1)
                {
                    pos.x = 0;
                }
                else if (bosses.Count == 2)
                {
                    pos.x = (i == 0) ? -200 : 200;
                }
                bossObj.transform.localPosition = pos;
            }
        }

        // Elite
        var elites = InGameData.Instance.Enemys.FindAll(b =>
            b.Stage.Split("_")[0] == "elite" && b.Stage.Split("_")[1] == storyDTO.StoryId.ToString()
        );

        EliteContentPos.transform.GetChild(0).gameObject.SetActive(false);
        EliteContentPos.transform.GetChild(1).gameObject.SetActive(false);

        for (int i = 0; i < elites.Count; i++)
        {
            var eliteObj = EliteContentPos.GetChild(i).gameObject;
            if (i < elites.Count)
            {
                eliteObj.SetActive(true);
                eliteObj.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(elites[i].ImgFacePath);
                int index = i;
                eliteObj.GetComponent<Button>().onClick.RemoveAllListeners();
                eliteObj.GetComponent<Button>().onClick.AddListener(() => {
                    OpenEnemyDetail(elites[index]);
                });

                // x좌표 위치 설정
                Vector3 pos = eliteObj.transform.localPosition;
                if (elites.Count == 1)
                {
                    pos.x = 0;
                }
                else if (elites.Count == 2)
                {
                    pos.x = (i == 0) ? -200 : 200;
                }
                eliteObj.transform.localPosition = pos;
                Debug.Log(elites.Count);
            }
        }

        // Monster
        var monsters = InGameData.Instance.Enemys.FindAll(b =>
            (b.Stage.Split("_")[0] == "story" || b.Stage.Split("_")[0] == "public") && b.Stage.Split("_")[1] == storyDTO.StoryId.ToString()
        );
        //오브젝트 폴링
        for (int i = MonsterContentPos.childCount; i < monsters.Count; i++)
        {
            var obj = Instantiate(EnemyButtonPrefab, MonsterContentPos);
            obj.SetActive(false);
        }
        //일단 비활성화
        foreach (Transform child in MonsterContentPos)
        {
            child.gameObject.SetActive(false);
        }
        //맞는 애들만 보여주기
        for (int i = 0; i < monsters.Count; i++)
        {
            var monsterObj = MonsterContentPos.GetChild(i).gameObject;
            monsterObj.SetActive(true);
            monsterObj.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(monsters[i].ImgFacePath);
            int index = i;
            monsterObj.GetComponent<Button>().onClick.RemoveAllListeners();
            monsterObj.GetComponent<Button>().onClick.AddListener(() => {
                OpenEnemyDetail(monsters[index]);
            });
        }

        int rowCount = Mathf.CeilToInt((float)monsters.Count / 3);
        StoryContent.sizeDelta = new Vector2(StoryContent.sizeDelta.x, 2200 + ((rowCount + 1) * 256) + 500f);
        Debug.Log("rowCount: " + rowCount + ", Height: " + StoryContent.sizeDelta.y);

        StoryTextBox.localPosition = new Vector2(0, -(2200 + ((rowCount + 1) * 256) - 300f));
        StoryTextBox.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = storyDTO.Description;

        //스크롤 위치 초기화
        StoryContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(StoryContent.GetComponent<RectTransform>().anchoredPosition.x, 0);
    }

    public void CloseStoryDetail()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        StoryDetailCanvas.SetActive(false);
    }

    private void OpenEnemyDetail(EnemyDTO enemyDTO)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        EnemyDetailCanvas.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(EnemyDetailBox, 0f, 1f);

        //적 정보 설정
        EnemyDetailBox.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(enemyDTO.ImgPath);
        EnemyDetailBox.transform.GetChild(0).GetComponent<Image>().SetNativeSize();

        EnemyDetailBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = enemyDTO.Name;
        EnemyDetailBox.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = enemyDTO.Description;
        EnemyDetailBox.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = enemyDTO.HealthMin + " ~ " + enemyDTO.HealthMax;
        EnemyDetailBox.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = enemyDTO.HealMin + " ~ " + enemyDTO.HealMax;
        EnemyDetailBox.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = enemyDTO.AttackMin + " ~ " + enemyDTO.AttackMax;
        EnemyDetailBox.transform.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>().text = enemyDTO.DefenseMin + " ~ " + enemyDTO.DefenseMax;

        // 패시브 보유 시
        if (enemyDTO.Passive != null)
        {
            PassivePos.SetActive(true);
            //오브젝트 폴링
            foreach (Transform child in PassivePos.transform)
            {
                child.gameObject.SetActive(false);
            }
            for (int i = PassivePos.transform.childCount; i < enemyDTO.Passive.Count; i++)
            {
                var obj = Instantiate(PassivePrefab, PassivePos.transform);
                obj.SetActive(false);
            }

            for (int i = 0; i < enemyDTO.Passive.Count; i++)
            {
                var passiveObj = PassivePos.transform.GetChild(i).gameObject;
                passiveObj.SetActive(true);
                passiveObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(enemyDTO.Passive[i].PassiveImgPath);

                int index = i;
                passiveObj.GetComponent<Button>().onClick.RemoveAllListeners();
                passiveObj.GetComponent<Button>().onClick.AddListener(() => {
                    OpenPassiveDetail(enemyDTO.Passive[index]);
                });
            }
        }
        else
        {
            PassivePos.SetActive(false);
        }
    }

    public void CloseEnemyDetail()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        EnemyDetailCanvas.SetActive(false);
    }

    public void OpenPassiveDetail(EnemyPassiveDTO passiveDTO)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        PassiveDetailCanvas.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(PassiveDetailBox, 0f, 1f);

        //패시브 정보 설정
        PassiveDetailBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = passiveDTO.PassiveText;
    }

    public void ClosePassiveDetail()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        PassiveDetailCanvas.SetActive(false);
    }
}
