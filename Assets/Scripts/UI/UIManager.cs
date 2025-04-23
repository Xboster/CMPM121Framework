using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject nextWaveButton;
    public GameObject rewardScreen;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Hide at the start
        if (nextWaveButton != null)
            nextWaveButton.SetActive(false);
    }

    public void ShowNextWaveButton()
    {
        if (nextWaveButton != null)
            nextWaveButton.SetActive(true);
        if (rewardScreen != null)
            rewardScreen.SetActive(true);
    }

    public void HideNextWaveButton()
    {
        if (nextWaveButton != null)
            nextWaveButton.SetActive(false);
        if (rewardScreen != null)
            rewardScreen.SetActive(false);
    }
}


