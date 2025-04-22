using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSelectorController : MonoBehaviour
{
    public EnemySpawner spawner;
    private string levelName;

    public void SetLevel(string name)
    {
        levelName = name;

        // Set the button text
        TextMeshProUGUI label = GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = name;
        if (label != null)
        {
            label.text = name;
        } else
        {
            Debug.LogWarning("No Text component found on this button prefab!");
        }

        // Add click listener
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    // ✅ This MUST exist for the button to work
    private void HandleClick()
    {
        if (spawner != null)
        {
            spawner.StartLevel(levelName);
        }
    }
}
