using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Simple HUD manager for showing messages on screen.
public class HUDMessageController : MonoBehaviour
{
    public static HUDMessageController Instance { get; private set; }

    [Tooltip("The Text component to write messages into.")]
    public Text hudText;

    [Tooltip("The panel GameObject to show/hide (parent of hudText). If unassigned, falls back to hudText's own GameObject.")]
    public GameObject hudPanel;

    private Coroutine _hideCoroutine;
    private const string WelcomeMessage = "Welcome, place the hub on the brightest blue mapping";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ShowMessage(WelcomeMessage);
    }

    // show a message until another message replaces it
    public void ShowMessage(string message)
    {
        if (hudText == null)
            return;

        StopHideCoroutine();
        hudText.text = message;
        Panel().SetActive(true);
    }

    // show a message for a fixed duration, then hide it
    public void ShowMessage(string message, float duration)
    {
        ShowMessage(message);
        StopHideCoroutine();
        _hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    // hide the HUD immediately
    public void Hide()
    {
        StopHideCoroutine();
        Panel()?.SetActive(false);
    }

    private GameObject Panel() => hudPanel != null ? hudPanel : hudText != null ? hudText.gameObject : null;

    private void StopHideCoroutine()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
        _hideCoroutine = null;
    }
}
