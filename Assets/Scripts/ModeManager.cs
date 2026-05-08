using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeManager : MonoBehaviour
{
    [Header("Mode Scripts")]
    public SoccerKick soccerKick;
    public GoalkeeperMode goalkeeperMode;

    [Header("UI")]
    public GameObject modeSelectPanel;
    public TextMeshProUGUI currentModeText;

    private bool menuOpen = false;

    public Transform xrOrigin;
    public Transform freeKickSpawnPoint;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soccerKick.enabled = false;
        goalkeeperMode.enabled = false;
        modeSelectPanel.SetActive(false);
        currentModeText.text = "Select Mode";
    }

    public void ToggleModeMenu()
    {
        menuOpen = !menuOpen;
        modeSelectPanel.SetActive(menuOpen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFreeKickMode()
    {
        xrOrigin.position = freeKickSpawnPoint.position;
        xrOrigin.rotation = freeKickSpawnPoint.rotation;
        
        soccerKick.enabled = true;
        goalkeeperMode.enabled = false;

        currentModeText.text = "Free Kick Mode";

        menuOpen = false;
        modeSelectPanel.SetActive(false);
    }

    public void SetGoalkeeperMode()
    {
        soccerKick.enabled = false;
        goalkeeperMode.enabled = true;

        currentModeText.text = "Goalkeeper Mode";

        menuOpen = false;
        modeSelectPanel.SetActive(false);
    }
    
}
