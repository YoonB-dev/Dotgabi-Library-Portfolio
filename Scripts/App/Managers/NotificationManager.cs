using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviorSingleton<NotificationManager>
{
    [SerializeField] private GameObject CheckNotificationCanvas;
    [SerializeField] private GameObject ShownNotificationCanvas;
    [SerializeField] private GameObject TextBox;
    private Coroutine TextUpCoroutine;
    private bool isTextBoxActive = false;
    private Tween textBoxTween;
    public void SetCheckNotification(string text)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        CheckNotificationCanvas.SetActive(true);
        CheckNotificationCanvas.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        ButtonAnim.Instance.ButtonScaleIn(CheckNotificationCanvas.transform.GetChild(1).gameObject, 0f, 1f);
    }
    public void CloseNotification()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        CheckNotificationCanvas.SetActive(false);
    }

    // 위로 사라지면서 알림을 보여주는 메서드
    public void SetShownNotification(string text)
    {
        ShownNotificationCanvas.SetActive(true);
        ShownNotificationCanvas.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        // 알림 애니메이션
        if (TextUpCoroutine != null) { StopCoroutine(TextUpCoroutine); }
        TextUpCoroutine = StartCoroutine(TextUp(1f));
    }
    IEnumerator TextUp(float time)
    {
        var color = ShownNotificationCanvas.transform.GetChild(0).GetComponent<Image>().color;
        color.a = 1f;
        var image = ShownNotificationCanvas.transform.GetChild(0).GetComponent<Image>();
        var rect = ShownNotificationCanvas.transform.GetChild(0);

        image.DOKill(); // 기존 Tween 모두 제거
        rect.DOKill();  // 위치 Tween도 제거

        // 초기화
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        rect.localPosition = Vector3.zero;

        // 새 Tween 시작
        var tween = rect.DOLocalMoveY(rect.localPosition.y + 100, time);
        image.DOFade(0f, time).SetEase(Ease.InCirc);
        yield return tween.WaitForCompletion();
        ShownNotificationCanvas.SetActive(false);
    }

    public void Update()
    {
        if (isTextBoxActive && Input.GetMouseButtonDown(0))
        {
            TextBox.SetActive(false);
            isTextBoxActive = false;
        }
    }

    public void SetTextBox(string text, Vector3 worldPosition, EnumTypes.TextMotionType type)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        TextBox.SetActive(true);
        if (textBoxTween != null)
        {
            textBoxTween.Kill();
        }


        TextBox.transform.position = worldPosition;

        if (TextBox.transform.localPosition.x > 250f)
        {
            TextBox.transform.localPosition = new Vector3(250f, TextBox.transform.localPosition.y, TextBox.transform.localPosition.z);
        } else if (TextBox.transform.localPosition.x < -250f)
        {
            TextBox.transform.localPosition = new Vector3(-250f, TextBox.transform.localPosition.y, TextBox.transform.localPosition.z);
        }

        switch (type)
        {
            case EnumTypes.TextMotionType.up:
                textBoxTween = TextBox.transform.DOLocalMoveY(TextBox.transform.localPosition.y + 50f, 0.5f).SetEase(Ease.OutCirc);
                break;
            case EnumTypes.TextMotionType.down:
                textBoxTween = TextBox.transform.DOLocalMoveY(TextBox.transform.localPosition.y - 50f, 0.5f).SetEase(Ease.OutCirc);
                break;
        }

        TextBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        isTextBoxActive = true;
    }
}
