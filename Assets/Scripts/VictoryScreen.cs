using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Victory screen that shows the collected time capsule items.
public class VictoryScreen : MonoBehaviour
{
    [Tooltip("Text component that shows the congratulatory header.")]
    public Text titleText;

    [Tooltip("Text component that lists the collected items.")]
    public Text itemListText;

    [Tooltip("Button that restarts the entire game and clears all stored data.")]
    public Button restartButton;

    private const string TitleMessage = "Congratulations!\nYour Time Capsule is Complete!";
    private const string EmptyMessage = "No memories were added.";
    private const string ArSceneName = "ARScene";

    private void Start()
    {
        DisplayVictoryScreen();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
    }

    private void DisplayVictoryScreen()
    {
        if (titleText != null)
            titleText.text = TitleMessage;

        if (itemListText == null)
            return;

        var items = TimeCapsuleGameData.CollectedItems;
        if (items.Count == 0)
        {
            itemListText.text = EmptyMessage;
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Items in your Time Capsule:");
        sb.AppendLine();

        for (int i = 0; i < items.Count; i++)
            sb.AppendLine($"  {i + 1}. {items[i]}");

        itemListText.text = sb.ToString();
    }
    public void RestartGame()
    {
        TimeCapsuleGameData.Reset();

        PlayerPrefs.DeleteKey("HubPlaced");
        PlayerPrefs.DeleteKey("HubX");
        PlayerPrefs.DeleteKey("HubY");
        PlayerPrefs.DeleteKey("HubZ");
        PlayerPrefs.DeleteKey("HubRotX");
        PlayerPrefs.DeleteKey("HubRotY");
        PlayerPrefs.DeleteKey("HubRotZ");
        PlayerPrefs.DeleteKey("HubRotW");
        PlayerPrefs.Save();

        SceneManager.LoadScene(ArSceneName);
    }
}
