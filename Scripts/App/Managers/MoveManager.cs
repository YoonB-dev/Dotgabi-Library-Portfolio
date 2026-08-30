using UnityEngine;

public class MoveManager : SceneSingleton<MoveManager>
{
    [SerializeField] private GameObject forwardButton;

    public void SetForwardButtonActive(bool isActive)
    {
        if (forwardButton != null)
        {
            forwardButton.SetActive(isActive);
        } else
        {
            Debug.LogWarning("Forward button is not assigned in the inspector.");
        }
    }
}
