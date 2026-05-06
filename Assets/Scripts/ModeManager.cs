using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeManager : MonoBehaviour
{
    [Header("Mode Scripts")]
    public SoccerKick soccerKick;
    public GoalkeeperMode goalkeeperMode;

    [Header("UI")]
    public GameObject menuPanel; 
    public GameObject menuButton;
    public TextMeshProUGUI currentModeText;

    private bool menuOpen = false;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuPanel.SetActive(false);
        SetFreeKickMode();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);

            if (IsButtonTouched(menuButton, touch.position))
            {
                menuOpen = !menuOpen;
                menuPanel.SetActive(menuOpen);
                return;
            }

            if (menuOpen)
            {
                Transform freeKick = menuPanel.transform.Find("FreeKickButton");
                Transform goalkeeper = menuPanel.transform.Find("GoalkeeperButton");

                if (freeKick != null && IsButtonTouched(freeKick.gameObject, touch.position))
                {
                    SetFreeKickMode();
                    return;
                }
                if (goalkeeper != null && IsButtonTouched(goalkeeper.gameObject, touch.position))
                {
                    SetGoalkeeperMode();
                    return;
                }
            }
        }
    }
    bool IsButtonTouched(GameObject button, Vector2 touchPos)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(rect, touchPos);
    }
    public void SetFreeKickMode()
    {
        soccerKick.enabled = true;
        goalkeeperMode.enabled = false;
        currentModeText.text = "👟 Free Kick";
        menuPanel.SetActive(false);
        menuOpen = false;
    }

    public void SetGoalkeeperMode()
    {
        soccerKick.enabled = false;
        goalkeeperMode.enabled = true;
        currentModeText.text = "🧤 Goalkeeper";
        menuPanel.SetActive(false);
        menuOpen = false;
    }
    
}
