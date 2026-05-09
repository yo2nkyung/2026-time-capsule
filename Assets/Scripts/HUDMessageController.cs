using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Manages the persistent HUD canvas
public class HUDMessageController : MonoBehaviour
{
    public static HUDMessageController Instance { get; private set; }
    public Text hudText;
    public GameObject hudPanel;
    public GameObject messageBackground;
    public GameObject resetButton;
    public event Action ResetRequested;

    private Coroutine _hideCoroutine;
    private const string WelcomeMessage = "Welcome, place the hub on the QR code";
    private const string ArSceneName = "ARScene";
    private const string MessageBackgroundName = "MessageBackground";
    private const string ResetButtonName = "ResetButton";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResolveHudReferences();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ShowMessage(WelcomeMessage);
    }

    private void ResolveHudReferences()
    {
        if (messageBackground == null)
            messageBackground = FindChildByName(transform, MessageBackgroundName);

        if (resetButton == null)
            resetButton = FindChildByName(transform, ResetButtonName);

        if (resetButton != null)
        {
            Button btn = resetButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(() => ResetRequested?.Invoke());
            }
        }
    }

    private static GameObject FindChildByName(Transform parent, string childName)
    {
        Transform found = parent.Find(childName);
        return found != null ? found.gameObject : null;
    }

    public void ShowResetButton(bool visible)
    {
        if (resetButton != null)
            resetButton.SetActive(visible);
    }

    public void OnEnterMinigame()
    {
        if (messageBackground != null)
            messageBackground.SetActive(false);

        if (resetButton != null)
            resetButton.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool inArScene = scene.name == ArSceneName;

        if (messageBackground != null)
            messageBackground.SetActive(inArScene);

        if (inArScene)
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

    //hides the HUD immediately
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
